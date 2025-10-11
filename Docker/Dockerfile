FROM ubuntu:latest

# Set environment variables to avoid interactive prompts during installation
ENV DEBIAN_FRONTEND=noninteractive

# Expected arguments
ARG SSH_USER_NAME
ARG SSH_USER_PASSWORD
ARG HOSTNAME
ARG TIMEZONE
ARG CERTNAME

RUN if [ -z "$SSH_USER_NAME" ]; then \
    echo "Error: SSH_USER_NAME must be defined!" && exit 1; \
    fi

RUN if [ -z "$SSH_USER_PASSWORD" ]; then \
    echo "Error: SSH_USER_PASSWORD must be defined!" && exit 1; \
    fi

RUN if [ -z "$HOSTNAME" ]; then \
    echo "Error: HOSTNAME must be defined!" && exit 1; \
    fi

RUN if [ -z "$TIMEZONE" ]; then \
    echo "Error: TIMEZONE must be defined!" && exit 1; \
    fi

RUN if [ -z "$CERTNAME" ]; then \
    echo "Error: CERTNAME must be defined!" && exit 1; \
    fi

# Base packages
RUN apt-get update && apt-get install -y \
    sudo syslog-ng nano tzdata cron openssh-server curl git jq

# Node.js 20
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get update && apt-get install -y nodejs

# .NET 8 SDK
RUN wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update && apt-get install -y dotnet-sdk-8.0

RUN rm -rf /var/lib/apt/lists/*

# Clone repo
WORKDIR /opt
RUN git clone https://github.com/mekatrol/secretsanta.git

# Build API
WORKDIR /opt/secretsanta
RUN dotnet restore
RUN dotnet build -c Release

# Build SPA
WORKDIR /opt/secretsanta/SecretSanta.Spa
RUN npm i -g npm@latest && \
    npm install --save-dev @vitejs/plugin-basic-ssl@^2 && \
    if [ -f package-lock.json ]; then npm ci --no-audit --no-fund; else npm install --no-audit --no-fund; fi && \
    npm run build

# Publish backend
WORKDIR /opt/secretsanta
RUN dotnet publish -c Release -o /opt/secretsanta/published

# Copy origins settings
COPY origins.txt /opt/origins.txt

# Update CORS settings
# Convert lines -> JSON array, ignoring blanks and comments
RUN ORIGINS_JSON="$(jq -R -s '[split("\n")[] | gsub("^\\s+|\\s+$";"") | select(length>0 and (test("^#")|not))]' /opt/origins.txt)" \
    && jq --argjson origins "$ORIGINS_JSON" \
    '.Cors = (.Cors // {}) | .Cors.AllowedOrigins = $origins' \
    /opt/secretsanta/published/appsettings.json \
    > /tmp/appsettings.json \
    && mv /tmp/appsettings.json /opt/secretsanta/published/appsettings.json

# The ports to expose:
#   443 - secretsanta site
EXPOSE 443

# Create the SSH user and set a password
RUN useradd -m -s /bin/bash $SSH_USER_NAME && echo "$SSH_USER_NAME:$SSH_USER_PASSWORD" | chpasswd

# Add the SSH user to the sudo group
RUN usermod -aG sudo $SSH_USER_NAME

# Create SSH run file directory
RUN mkdir /var/run/sshd 

# Exclude core messages in docker cotnainer
RUN sed -i 's/system()/system(exclude-kmsg(yes))/g' /etc/syslog-ng/syslog-ng.conf

# Copy all certs/keys from build context
COPY ssl/certs/ /tmp/ssl/certs/
COPY ssl/priv/  /tmp/ssl/priv/

# Ensure target dirs exist with correct perms
RUN mkdir -p /etc/ssl/certs /etc/ssl/private \
    && chmod 755 /etc/ssl/certs \
    && chmod 700 /etc/ssl/private

COPY ssl/certs/${CERTNAME}.pfx /etc/ssl/certs/${CERTNAME}.pfx

# Tell ASP.NET to use pfx and listen on all interfaces 443
ENV ASPNETCORE_URLS="https://0.0.0.0:443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/etc/ssl/certs/$CERTNAME.pfx"

# Set timezone
RUN  ln -fs /usr/share/zoneinfo/$TIMEZONE /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata

WORKDIR /opt/secretsanta/published

# Create container startup script
RUN echo "#!/bin/bash\n\
    service syslog-ng start\n\
    service cron start\n\
    service ssh start\n\    
    nohup dotnet /opt/secretsanta/published/SecretSanta.Api.dll >>/var/log/secretsanta.log 2>&1 &\n\
    tail -f /var/log/syslog" > /start.sh && chmod +x /start.sh

# Run container start up script
CMD ["/bin/bash", "/start.sh"]
