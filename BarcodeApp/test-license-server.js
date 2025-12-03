/**
 * Simple License Server for Testing
 * 
 * To run:
 *   1. Install Node.js (https://nodejs.org/)
 *   2. Install dependencies: npm install express
 *   3. Run: node test-license-server.js
 * 
 * The server will start on http://localhost:3000
 * 
 * Update LicensingService.cs to use: http://localhost:3000/api/validateKey
 * (Note: For production, use HTTPS)
 */

const express = require('express');
const app = express();

app.use(express.json());

// Test licenses (in production, use a database)
const licenses = {
    // Valid test license - expires in 1 year
    'TEST-LICENSE-001': {
        isValid: true,
        expiryDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000), // 1 year from now
        maxMachines: 1,
        machines: []
    },
    // Valid test license - expires in 6 months
    'TEST-LICENSE-002': {
        isValid: true,
        expiryDate: new Date(Date.now() + 180 * 24 * 60 * 60 * 1000), // 6 months from now
        maxMachines: 3,
        machines: []
    },
    // Expired license for testing
    'EXPIRED-LICENSE': {
        isValid: true,
        expiryDate: new Date(Date.now() - 24 * 60 * 60 * 1000), // Yesterday
        maxMachines: 1,
        machines: []
    }
};

// CORS middleware (for testing)
app.use((req, res, next) => {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'POST, OPTIONS');
    res.header('Access-Control-Allow-Headers', 'Content-Type');
    if (req.method === 'OPTIONS') {
        return res.sendStatus(200);
    }
    next();
});

app.post('/api/validateKey', (req, res) => {
    console.log('Received validation request:', JSON.stringify(req.body, null, 2));
    
    const { Key, MachineId, AppVersion } = req.body;
    
    // Validate input
    if (!Key || !MachineId) {
        console.log('Validation failed: Missing required fields');
        return res.status(400).json({
            IsValid: false,
            ExpiryDate: '0001-01-01T00:00:00Z',
            StatusMessage: 'Missing required fields'
        });
    }
    
    const license = licenses[Key];
    
    if (!license) {
        console.log(`Validation failed: Invalid license key: ${Key}`);
        return res.json({
            IsValid: false,
            ExpiryDate: '0001-01-01T00:00:00Z',
            StatusMessage: 'Invalid license key'
        });
    }
    
    // Check if expired
    if (new Date() > license.expiryDate) {
        console.log(`Validation failed: License expired: ${Key}`);
        return res.json({
            IsValid: false,
            ExpiryDate: license.expiryDate.toISOString(),
            StatusMessage: 'License has expired'
        });
    }
    
    // Check machine binding
    if (!license.machines.includes(MachineId)) {
        if (license.machines.length >= license.maxMachines) {
            console.log(`Validation failed: Max machines reached for: ${Key}`);
            return res.json({
                IsValid: false,
                ExpiryDate: license.expiryDate.toISOString(),
                StatusMessage: 'License already activated on maximum number of machines'
            });
        }
        license.machines.push(MachineId);
        console.log(`Machine ${MachineId} added to license ${Key}`);
    }
    
    console.log(`Validation successful: ${Key} for machine ${MachineId}`);
    res.json({
        IsValid: true,
        ExpiryDate: license.expiryDate.toISOString(),
        StatusMessage: 'License activated successfully'
    });
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

// List test licenses endpoint (for debugging)
app.get('/api/test-licenses', (req, res) => {
    const testLicenses = Object.keys(licenses).map(key => ({
        key,
        expiryDate: licenses[key].expiryDate.toISOString(),
        maxMachines: licenses[key].maxMachines,
        activatedMachines: licenses[key].machines.length
    }));
    res.json(testLicenses);
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log('='.repeat(50));
    console.log('License Server Running');
    console.log('='.repeat(50));
    console.log(`Server: http://localhost:${PORT}`);
    console.log(`Endpoint: http://localhost:${PORT}/api/validateKey`);
    console.log(`Health: http://localhost:${PORT}/health`);
    console.log(`Test Licenses: http://localhost:${PORT}/api/test-licenses`);
    console.log('');
    console.log('Available Test License Keys:');
    Object.keys(licenses).forEach(key => {
        const license = licenses[key];
        const isExpired = new Date() > license.expiryDate;
        console.log(`  - ${key} (Expires: ${license.expiryDate.toISOString()}) ${isExpired ? '[EXPIRED]' : ''}`);
    });
    console.log('='.repeat(50));
});

