const https = require('https');
const http = require('http');

const BASE_URL = process.env.BASE_URL || 'https://cross-seta-web-17655.fly.dev';
const IS_HTTPS = BASE_URL.startsWith('https');

// Test Cases for Traffic Light Logic
const TEST_CASES = [
    { 
        description: '🟢 GREEN: Valid ID + Matching Surname',
        id: '0002080806082', 
        surname: 'Makaula', 
        expectedResult: 'VERIFIED' 
    },
    { 
        description: '🟡 YELLOW: Valid ID + Mismatch Surname',
        id: '0002080806082', 
        surname: 'Smith', 
        expectedResult: 'MISMATCH' 
    },
    { 
        description: '🔴 RED: Non-existent ID',
        id: '9999999999999', 
        surname: 'Any', 
        expectedResult: 'NOT_FOUND' 
    }
    // Note: To test DECEASED (RED), we need a known deceased ID in the DB.
];

function makeRequest(path) {
    return new Promise((resolve, reject) => {
        const client = IS_HTTPS ? https : http;
        const url = `${BASE_URL}${path}`;
        
        console.log(`Requesting: ${url}`);

        client.get(url, (res) => {
            let data = '';

            res.on('data', (chunk) => {
                data += chunk;
            });

            res.on('end', () => {
                try {
                    resolve({ 
                        status: res.statusCode, 
                        data: data ? JSON.parse(data) : {} 
                    });
                } catch (e) {
                    resolve({ status: res.statusCode, data, error: e });
                }
            });
        }).on('error', (e) => {
            reject(e);
        });
    });
}

async function runTests() {
    console.log(`🚦 Starting Traffic Light Test Suite on ${BASE_URL}...\n`);
    
    let passed = 0;
    let failed = 0;

    for (const test of TEST_CASES) {
        try {
            const path = `/api/verification/home-affairs/${test.id}?surname=${encodeURIComponent(test.surname)}`;
            const res = await makeRequest(path);
            
            const actualResult = res.data.verificationResult;

            if (actualResult === test.expectedResult) {
                console.log(`✅ [PASS] ${test.description}`);
                console.log(`   Result: ${actualResult}`);
                passed++;
            } else {
                console.error(`❌ [FAIL] ${test.description}`);
                console.error(`   Expected: ${test.expectedResult}`);
                console.error(`   Received: ${actualResult}`);
                console.error(`   Full Response:`, res.data);
                failed++;
            }
        } catch (e) {
            console.error(`❌ [ERROR] ${test.description}:`, e.message);
            failed++;
        }
        console.log('---');
    }

    console.log(`\nTest Summary: ${passed} Passed, ${failed} Failed.`);
    if (failed > 0) process.exit(1);
}

runTests();
