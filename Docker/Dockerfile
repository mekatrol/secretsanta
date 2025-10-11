FROM ubuntu:latest

# Set environment variables to avoid interactive prompts during installation
ENV DEBIAN_FRONTEND=noninteractive

# Expected arguments
ARG SSH_USER_NAME
ARG SSH_USER_PASSWORD
ARG HOSTNAME
ARG TIMEZONE

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

# Install packages
RUN apt update && DEBIAN_FRONTEND=noninteractive apt install -y \
    sudo \
    syslog-ng \
    nano \
    tzdata \
    cron \
    openssh-server

# Install .NET SDK (replace 8.0 with desired version)
RUN wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0 git && \
    rm -rf /var/lib/apt/lists/*

RUN rm -rf /var/lib/apt/lists/*

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
RUN sudo sed -i 's/system()/system(exclude-kmsg(yes))/g' /etc/syslog-ng/syslog-ng.conf

# Copy all certs/keys from build context
COPY ssl/certs/ /tmp/ssl/certs/
COPY ssl/priv/  /tmp/ssl/priv/

# Set timezone
RUN  ln -fs /usr/share/zoneinfo/$TIMEZONE /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata

# Create container startup script
RUN echo "#!/bin/bash\n\
    sudo service syslog-ng start\n\
    sudo service cron start\n\
    sudo service ssh start\n\
    tail -f /var/log/syslog" > /start.sh && chmod +x /start.sh

# Run container start up script
CMD ["/bin/bash", "/start.sh"]
