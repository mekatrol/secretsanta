#!/bin/bash
set -euo pipefail

CONFIG_FILE="/etc/nginx/sites-available/reverse_proxy"
ENABLED_CONFIG="/etc/nginx/sites-enabled/reverse_proxy"
INPUT_FILE="reverse_proxy_list.txt"

# Check if input file exists
[[ -f "$INPUT_FILE" ]] || { echo "Error: $INPUT_FILE not found"; exit 1; }

# Create the Nginx configuration file
cat >"$CONFIG_FILE" <<'NGINX'
# Nginx reverse proxy configuration

# Redirect all HTTP to HTTPS for any host
#server {
#    listen 80 default_server;
#    server_name _;
#    return 301 https://$host$request_uri;
#}

# Common upgrade handling
map $http_upgrade $connection_upgrade {
    default upgrade;
    ""      close;
}
NGINX

# Generate per-host reverse proxy servers
while IFS=' ' read -r HOSTNAME IP_ADDRESS PROTOCOL_SCHEME PORT || [[ -n "${HOSTNAME:-}" ]]; do
    # Skip blanks/comments
    [[ -z "${HOSTNAME:-}" || "${HOSTNAME:0:1}" == "#" ]] && continue

    # default PORT by scheme if missing
    case "${PROTOCOL_SCHEME:-}" in
        http|ws)  PORT="${PORT:-80}"  ;;
        https|wss) PORT="${PORT:-443}" ;;
        *) echo "Error: unknown PROTOCOL_SCHEME '$PROTOCOL_SCHEME' for $HOSTNAME" >&2; exit 1 ;;
    esac

    if [[ -n "${HOSTNAME:-}" && -n "${IP_ADDRESS:-}" && -n "${PROTOCOL_SCHEME:-}" && -n "${PORT:-}" ]]; then
        cat >>"$CONFIG_FILE" <<NGINX

# $HOSTNAME --> $PROTOCOL_SCHEME://$IP_ADDRESS:$PORT
server {
    listen $PORT ssl http2;
    server_name $HOSTNAME;

    ssl_certificate     /etc/ssl/certs/$HOSTNAME.crt;
    ssl_certificate_key /etc/ssl/private/$HOSTNAME.key;
    ssl_protocols       TLSv1.2 TLSv1.3;
    ssl_ciphers         HIGH:!aNULL:!MD5;

    # Enable SNI when proxying HTTPS upstreams
    proxy_ssl_server_name on;
    proxy_ssl_name \$host;

    location / {
        proxy_pass $PROTOCOL_SCHEME://$IP_ADDRESS:$PORT;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Forwarded-Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection \$connection_upgrade;
        proxy_read_timeout 60s;
        proxy_send_timeout 60s;
        proxy_connect_timeout 5s;
        proxy_redirect off;
    }
}
NGINX
    fi
done <"$INPUT_FILE"

# Enable the configuration
ln -sf "$CONFIG_FILE" "$ENABLED_CONFIG"
