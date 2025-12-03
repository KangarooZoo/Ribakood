#!/bin/bash
# Hetzner License Server Setup Script
# Run as root on a fresh Ubuntu/Debian server

set -e

echo "=========================================="
echo "Hetzner License Server Setup"
echo "=========================================="

# Configuration
DOMAIN=""
SERVER_TYPE="nodejs"  # or "python"
APP_DIR="/opt/license-server"

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root"
    exit 1
fi

# Get domain name
read -p "Enter your domain name (e.g., license.example.com): " DOMAIN
if [ -z "$DOMAIN" ]; then
    echo "Domain name is required!"
    exit 1
fi

# Choose server type
read -p "Server type (nodejs/python) [nodejs]: " SERVER_TYPE
SERVER_TYPE=${SERVER_TYPE:-nodejs}

echo ""
echo "Configuration:"
echo "  Domain: $DOMAIN"
echo "  Server Type: $SERVER_TYPE"
echo "  App Directory: $APP_DIR"
echo ""
read -p "Continue? (y/n): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    exit 1
fi

# Update system
echo "Updating system..."
apt update && apt upgrade -y

# Install basic packages
echo "Installing basic packages..."
apt install -y curl wget git ufw

# Configure firewall
echo "Configuring firewall..."
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

# Install Node.js or Python
if [ "$SERVER_TYPE" = "nodejs" ]; then
    echo "Installing Node.js..."
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
    apt install -y nodejs
    NODE_VERSION=$(node --version)
    echo "Node.js installed: $NODE_VERSION"
else
    echo "Installing Python..."
    apt install -y python3 python3-pip python3-venv
    PYTHON_VERSION=$(python3 --version)
    echo "Python installed: $PYTHON_VERSION"
fi

# Install nginx and certbot
echo "Installing nginx and certbot..."
apt install -y nginx certbot python3-certbot-nginx

# Create application directory
echo "Creating application directory..."
mkdir -p $APP_DIR
cd $APP_DIR

# Deploy server code
if [ "$SERVER_TYPE" = "nodejs" ]; then
    echo "Setting up Node.js server..."
    
    cat > package.json << 'PKGEOF'
{
  "name": "license-server",
  "version": "1.0.0",
  "description": "Barcode App License Server",
  "main": "server.js",
  "scripts": {
    "start": "node server.js"
  },
  "dependencies": {
    "express": "^4.18.2"
  }
}
PKGEOF

    cat > server.js << 'SRVEOF'
const express = require('express');
const app = express();

app.use(express.json());

const licenses = {
    'TEST-LICENSE-001': {
        isValid: true,
        expiryDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000),
        maxMachines: 1,
        machines: []
    }
};

app.post('/api/validateKey', (req, res) => {
    const { Key, MachineId, AppVersion } = req.body;
    
    if (!Key || !MachineId) {
        return res.status(400).json({
            IsValid: false,
            ExpiryDate: '0001-01-01T00:00:00Z',
            StatusMessage: 'Missing required fields'
        });
    }
    
    const license = licenses[Key];
    
    if (!license) {
        return res.json({
            IsValid: false,
            ExpiryDate: '0001-01-01T00:00:00Z',
            StatusMessage: 'Invalid license key'
        });
    }
    
    if (new Date() > license.expiryDate) {
        return res.json({
            IsValid: false,
            ExpiryDate: license.expiryDate.toISOString(),
            StatusMessage: 'License has expired'
        });
    }
    
    if (!license.machines.includes(MachineId)) {
        if (license.machines.length >= license.maxMachines) {
            return res.json({
                IsValid: false,
                ExpiryDate: license.expiryDate.toISOString(),
                StatusMessage: 'License already activated on maximum number of machines'
            });
        }
        license.machines.push(MachineId);
    }
    
    res.json({
        IsValid: true,
        ExpiryDate: license.expiryDate.toISOString(),
        StatusMessage: 'License activated successfully'
    });
});

app.get('/health', (req, res) => {
    res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, '127.0.0.1', () => {
    console.log(`License server running on port ${PORT}`);
});
SRVEOF

    npm install
    
else
    echo "Setting up Python server..."
    
    python3 -m venv venv
    source venv/bin/activate
    
    cat > requirements.txt << 'REQEOF'
Flask==3.0.0
gunicorn==21.2.0
REQEOF

    pip install -r requirements.txt
    
    cat > app.py << 'APPEOF'
from flask import Flask, request, jsonify
from datetime import datetime, timedelta

app = Flask(__name__)

licenses = {
    'TEST-LICENSE-001': {
        'is_valid': True,
        'expiry_date': datetime.utcnow() + timedelta(days=365),
        'max_machines': 1,
        'machines': []
    }
}

@app.route('/api/validateKey', methods=['POST'])
def validate_key():
    data = request.json
    
    if not data or 'Key' not in data or 'MachineId' not in data:
        return jsonify({
            'IsValid': False,
            'ExpiryDate': '0001-01-01T00:00:00Z',
            'StatusMessage': 'Missing required fields'
        }), 400
    
    key = data['Key']
    machine_id = data['MachineId']
    
    if key not in licenses:
        return jsonify({
            'IsValid': False,
            'ExpiryDate': '0001-01-01T00:00:00Z',
            'StatusMessage': 'Invalid license key'
        })
    
    license_info = licenses[key]
    
    if datetime.utcnow() > license_info['expiry_date']:
        return jsonify({
            'IsValid': False,
            'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
            'StatusMessage': 'License has expired'
        })
    
    if machine_id not in license_info['machines']:
        if len(license_info['machines']) >= license_info['max_machines']:
            return jsonify({
                'IsValid': False,
                'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
                'StatusMessage': 'License already activated on maximum number of machines'
            })
        license_info['machines'].append(machine_id)
    
    return jsonify({
        'IsValid': True,
        'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
        'StatusMessage': 'License activated successfully'
    })

@app.route('/health', methods=['GET'])
def health():
    return jsonify({'status': 'ok', 'timestamp': datetime.utcnow().isoformat() + 'Z'})
APPEOF

    deactivate
fi

# Configure nginx
echo "Configuring nginx..."
cat > /etc/nginx/sites-available/license-server << NGINXEOF
server {
    listen 80;
    server_name $DOMAIN;

    location / {
        return 301 https://\$host\$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name $DOMAIN;

    # SSL will be configured by certbot
    # ssl_certificate /etc/letsencrypt/live/$DOMAIN/fullchain.pem;
    # ssl_certificate_key /etc/letsencrypt/live/$DOMAIN/privkey.pem;

    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
NGINXEOF

# Fix proxy_pass for Python
if [ "$SERVER_TYPE" = "python" ]; then
    sed -i 's|proxy_pass http://127.0.0.1:3000;|proxy_pass http://127.0.0.1:8000;|' /etc/nginx/sites-available/license-server
fi

ln -sf /etc/nginx/sites-available/license-server /etc/nginx/sites-enabled/
rm -f /etc/nginx/sites-enabled/default

# Test nginx config
nginx -t

# Start nginx
systemctl start nginx
systemctl enable nginx

# Create systemd service
if [ "$SERVER_TYPE" = "nodejs" ]; then
    cat > /etc/systemd/system/license-server.service << SVCEOF
[Unit]
Description=License Server
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=$APP_DIR
Environment="PORT=3000"
ExecStart=/usr/bin/node server.js
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
SVCEOF
else
    cat > /etc/systemd/system/license-server.service << SVCEOF
[Unit]
Description=License Server
After=network.target

[Service]
Type=notify
User=root
WorkingDirectory=$APP_DIR
Environment="PATH=$APP_DIR/venv/bin"
ExecStart=$APP_DIR/venv/bin/gunicorn --bind 127.0.0.1:8000 app:app
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
SVCEOF
fi

# Start license server
systemctl daemon-reload
systemctl start license-server
systemctl enable license-server

# Wait a moment for server to start
sleep 2

# Check if server is running
if systemctl is-active --quiet license-server; then
    echo "License server is running!"
else
    echo "Warning: License server may not be running. Check with: systemctl status license-server"
fi

# Get SSL certificate
echo ""
echo "=========================================="
echo "Setting up SSL certificate..."
echo "=========================================="
echo "Make sure your domain $DOMAIN points to this server's IP address!"
echo ""
read -p "Press Enter to continue with SSL setup..."

certbot --nginx -d $DOMAIN --non-interactive --agree-tos --register-unsafely-without-email

# Restart services
systemctl restart nginx
systemctl restart license-server

echo ""
echo "=========================================="
echo "Setup Complete!"
echo "=========================================="
echo ""
echo "Server URL: https://$DOMAIN"
echo "Health Check: https://$DOMAIN/health"
echo "API Endpoint: https://$DOMAIN/api/validateKey"
echo ""
echo "Test the server:"
echo "  curl https://$DOMAIN/health"
echo ""
echo "View logs:"
echo "  journalctl -u license-server -f"
echo "  tail -f /var/log/nginx/access.log"
echo ""
echo "Update your client app with:"
echo "  https://$DOMAIN/api/validateKey"
echo ""

