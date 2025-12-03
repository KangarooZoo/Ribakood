# Deploying License Server on Hetzner with HTTPS

This guide walks you through deploying the license server on Hetzner Cloud with HTTPS using Let's Encrypt.

## Prerequisites

- Hetzner Cloud account
- Domain name pointing to your Hetzner server IP
- SSH access to your server

## Step 1: Create Hetzner Cloud Server

1. Log in to [Hetzner Cloud Console](https://console.hetzner.cloud/)
2. Create a new project (or use existing)
3. Click "Add Server"
4. Choose:
   - **Location**: Choose closest to your users
   - **Image**: Ubuntu 22.04 or Debian 12
   - **Type**: CX11 (1 vCPU, 2GB RAM) is sufficient for testing, CX21+ for production
   - **SSH Keys**: Add your SSH key or set password
5. Click "Create & Buy Now"

## Step 2: Initial Server Setup

### Connect to your server:
```bash
ssh root@YOUR_SERVER_IP
```

### Update system:
```bash
apt update && apt upgrade -y
```

### Install required software:
```bash
# Install Node.js (Option 1 - Recommended)
curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
apt install -y nodejs

# OR Install Python (Option 2)
apt install -y python3 python3-pip python3-venv

# Install nginx and certbot
apt install -y nginx certbot python3-certbot-nginx

# Install firewall
apt install -y ufw
```

### Configure firewall:
```bash
# Allow SSH, HTTP, and HTTPS
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw enable
```

## Step 3: Deploy License Server

### Option A: Node.js Server

1. Create application directory:
```bash
mkdir -p /opt/license-server
cd /opt/license-server
```

2. Create `package.json`:
```bash
cat > package.json << 'EOF'
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
EOF
```

3. Create `server.js` (production version):
```bash
cat > server.js << 'EOF'
const express = require('express');
const app = express();

app.use(express.json());

// Production: Use environment variables or a database
const licenses = {
    'TEST-LICENSE-001': {
        isValid: true,
        expiryDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000),
        maxMachines: 1,
        machines: []
    }
};

// Security: Rate limiting (install: npm install express-rate-limit)
// const rateLimit = require('express-rate-limit');
// const limiter = rateLimit({
//     windowMs: 15 * 60 * 1000,
//     max: 10
// });
// app.use('/api/validateKey', limiter);

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
EOF
```

4. Install dependencies:
```bash
npm install
```

### Option B: Python Server

1. Create application directory:
```bash
mkdir -p /opt/license-server
cd /opt/license-server
```

2. Create virtual environment:
```bash
python3 -m venv venv
source venv/bin/activate
```

3. Create `requirements.txt`:
```bash
cat > requirements.txt << 'EOF'
Flask==3.0.0
gunicorn==21.2.0
EOF
```

4. Install dependencies:
```bash
pip install -r requirements.txt
```

5. Create `app.py`:
```bash
cat > app.py << 'EOF'
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

if __name__ == '__main__':
    app.run(host='127.0.0.1', port=3000)
EOF
```

## Step 4: Configure Nginx as Reverse Proxy

1. Create nginx configuration:
```bash
cat > /etc/nginx/sites-available/license-server << 'EOF'
server {
    listen 80;
    server_name your-domain.com;  # Replace with your domain

    # Redirect HTTP to HTTPS (will be configured by certbot)
    location / {
        return 301 https://$host$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;  # Replace with your domain

    # SSL certificates will be added by certbot
    # ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    # ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # Proxy to Node.js server
    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # OR for Python/Gunicorn:
    # location / {
    #     proxy_pass http://127.0.0.1:8000;
    #     proxy_set_header Host $host;
    #     proxy_set_header X-Real-IP $remote_addr;
    #     proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    #     proxy_set_header X-Forwarded-Proto $scheme;
    # }
}
EOF
```

2. Enable the site:
```bash
ln -s /etc/nginx/sites-available/license-server /etc/nginx/sites-enabled/
rm /etc/nginx/sites-enabled/default  # Remove default site
```

3. Test nginx configuration:
```bash
nginx -t
```

4. Start nginx:
```bash
systemctl start nginx
systemctl enable nginx
```

## Step 5: Set Up HTTPS with Let's Encrypt

1. **Important**: Make sure your domain DNS A record points to your Hetzner server IP:
   ```
   your-domain.com  A  YOUR_SERVER_IP
   ```

2. Obtain SSL certificate:
```bash
# Replace your-domain.com with your actual domain
certbot --nginx -d your-domain.com
```

3. Certbot will:
   - Automatically configure nginx
   - Set up automatic renewal
   - Configure HTTP to HTTPS redirect

4. Test certificate renewal:
```bash
certbot renew --dry-run
```

## Step 6: Set Up Systemd Service

### For Node.js:

1. Create systemd service:
```bash
cat > /etc/systemd/system/license-server.service << 'EOF'
[Unit]
Description=License Server
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=/opt/license-server
Environment="PORT=3000"
ExecStart=/usr/bin/node server.js
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF
```

2. Start and enable service:
```bash
systemctl daemon-reload
systemctl start license-server
systemctl enable license-server
systemctl status license-server
```

### For Python/Gunicorn:

1. Create systemd service:
```bash
cat > /etc/systemd/system/license-server.service << 'EOF'
[Unit]
Description=License Server
After=network.target

[Service]
Type=notify
User=root
WorkingDirectory=/opt/license-server
Environment="PATH=/opt/license-server/venv/bin"
ExecStart=/opt/license-server/venv/bin/gunicorn --bind 127.0.0.1:8000 app:app
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF
```

2. Start and enable service:
```bash
systemctl daemon-reload
systemctl start license-server
systemctl enable license-server
systemctl status license-server
```

## Step 7: Verify Deployment

1. Check server status:
```bash
systemctl status license-server
systemctl status nginx
```

2. Test the endpoint:
```bash
curl https://your-domain.com/health
```

3. Test license validation:
```bash
curl -X POST https://your-domain.com/api/validateKey \
  -H "Content-Type: application/json" \
  -d '{
    "Key": "TEST-LICENSE-001",
    "MachineId": "test-machine-id",
    "AppVersion": "1.0"
  }'
```

## Step 8: Update Client Application

Update `BarcodeApp/Services/LicensingService.cs`:

```csharp
private const string LicenseServerUrl = "https://v0-license-server-setup.vercel.app/api/validateKey";
```

## Security Enhancements

### 1. Add Rate Limiting (Node.js)

```bash
cd /opt/license-server
npm install express-rate-limit
```

Update `server.js`:
```javascript
const rateLimit = require('express-rate-limit');

const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutes
    max: 10, // limit each IP to 10 requests per windowMs
    message: 'Too many requests, please try again later.'
});

app.use('/api/validateKey', limiter);
```

### 2. Add Database (Recommended for Production)

Install PostgreSQL or MySQL:
```bash
apt install -y postgresql postgresql-contrib
```

Or use SQLite for simplicity:
```bash
npm install sqlite3  # Node.js
# or
pip install sqlalchemy  # Python
```

### 3. Set Up Logging

Create log directory:
```bash
mkdir -p /var/log/license-server
chmod 755 /var/log/license-server
```

Update systemd service to log to file:
```ini
StandardOutput=append:/var/log/license-server/output.log
StandardError=append:/var/log/license-server/error.log
```

### 4. Firewall Hardening

```bash
# Only allow specific IPs if needed
ufw allow from YOUR_IP to any port 22

# Or use fail2ban for SSH protection
apt install -y fail2ban
```

## Monitoring

### View logs:
```bash
# Application logs
journalctl -u license-server -f

# Nginx logs
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log
```

### Set up monitoring (optional):
- Use Hetzner Cloud Monitoring
- Set up UptimeRobot or similar
- Configure email alerts for certificate expiration

## Troubleshooting

### Server not starting:
```bash
systemctl status license-server
journalctl -u license-server -n 50
```

### Nginx errors:
```bash
nginx -t
tail -f /var/log/nginx/error.log
```

### SSL certificate issues:
```bash
certbot certificates
certbot renew --force-renewal
```

### Port already in use:
```bash
netstat -tulpn | grep :3000
# Kill the process or change port
```

## Backup and Maintenance

### Backup license data:
```bash
# If using files/database
tar -czf license-backup-$(date +%Y%m%d).tar.gz /opt/license-server
```

### Update server code:
```bash
cd /opt/license-server
# Pull updates or copy new files
systemctl restart license-server
```

## Cost Estimate

- **CX11 Server**: ~€4/month
- **Domain**: ~€10-15/year
- **SSL Certificate**: Free (Let's Encrypt)
- **Total**: ~€5-6/month

## Next Steps

1. Set up a database for license storage
2. Implement proper license key generation
3. Add admin interface for license management
4. Set up automated backups
5. Configure monitoring and alerts

