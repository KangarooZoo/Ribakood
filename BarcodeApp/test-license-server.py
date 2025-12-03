"""
Simple License Server for Testing (Python/Flask version)

To run:
    1. Install Python 3.7+
    2. Install Flask: pip install flask flask-cors
    3. Run: python test-license-server.py

The server will start on http://localhost:3000

Update LicensingService.cs to use: http://localhost:3000/api/validateKey
(Note: For production, use HTTPS)
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
from datetime import datetime, timedelta
from typing import Dict, List

app = Flask(__name__)
CORS(app)  # Enable CORS for testing

# Test licenses (in production, use a database)
licenses: Dict[str, dict] = {
    # Valid test license - expires in 1 year
    'TEST-LICENSE-001': {
        'is_valid': True,
        'expiry_date': datetime.utcnow() + timedelta(days=365),
        'max_machines': 1,
        'machines': []
    },
    # Valid test license - expires in 6 months
    'TEST-LICENSE-002': {
        'is_valid': True,
        'expiry_date': datetime.utcnow() + timedelta(days=180),
        'max_machines': 3,
        'machines': []
    },
    # Expired license for testing
    'EXPIRED-LICENSE': {
        'is_valid': True,
        'expiry_date': datetime.utcnow() - timedelta(days=1),
        'max_machines': 1,
        'machines': []
    }
}


@app.route('/api/validateKey', methods=['POST'])
def validate_key():
    data = request.json
    print(f'Received validation request: {data}')
    
    if not data or 'Key' not in data or 'MachineId' not in data:
        print('Validation failed: Missing required fields')
        return jsonify({
            'IsValid': False,
            'ExpiryDate': '0001-01-01T00:00:00Z',
            'StatusMessage': 'Missing required fields'
        }), 400
    
    key = data['Key']
    machine_id = data['MachineId']
    
    if key not in licenses:
        print(f'Validation failed: Invalid license key: {key}')
        return jsonify({
            'IsValid': False,
            'ExpiryDate': '0001-01-01T00:00:00Z',
            'StatusMessage': 'Invalid license key'
        })
    
    license_info = licenses[key]
    
    # Check if expired
    if datetime.utcnow() > license_info['expiry_date']:
        print(f'Validation failed: License expired: {key}')
        return jsonify({
            'IsValid': False,
            'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
            'StatusMessage': 'License has expired'
        })
    
    # Check machine binding
    if machine_id not in license_info['machines']:
        if len(license_info['machines']) >= license_info['max_machines']:
            print(f'Validation failed: Max machines reached for: {key}')
            return jsonify({
                'IsValid': False,
                'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
                'StatusMessage': 'License already activated on maximum number of machines'
            })
        license_info['machines'].append(machine_id)
        print(f'Machine {machine_id} added to license {key}')
    
    print(f'Validation successful: {key} for machine {machine_id}')
    return jsonify({
        'IsValid': True,
        'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
        'StatusMessage': 'License activated successfully'
    })


@app.route('/health', methods=['GET'])
def health():
    return jsonify({'status': 'ok', 'timestamp': datetime.utcnow().isoformat() + 'Z'})


@app.route('/api/test-licenses', methods=['GET'])
def list_test_licenses():
    """List available test licenses (for debugging)"""
    test_licenses = []
    for key, info in licenses.items():
        test_licenses.append({
            'key': key,
            'expiryDate': info['expiry_date'].isoformat() + 'Z',
            'maxMachines': info['max_machines'],
            'activatedMachines': len(info['machines'])
        })
    return jsonify(test_licenses)


if __name__ == '__main__':
    print('=' * 50)
    print('License Server Running')
    print('=' * 50)
    print('Server: http://localhost:3000')
    print('Endpoint: http://localhost:3000/api/validateKey')
    print('Health: http://localhost:3000/health')
    print('Test Licenses: http://localhost:3000/api/test-licenses')
    print('')
    print('Available Test License Keys:')
    for key, info in licenses.items():
        is_expired = datetime.utcnow() > info['expiry_date']
        status = '[EXPIRED]' if is_expired else ''
        print(f'  - {key} (Expires: {info["expiry_date"].isoformat()}Z) {status}')
    print('=' * 50)
    print('')
    
    # Run with debug mode for development
    app.run(host='0.0.0.0', port=3000, debug=True)

