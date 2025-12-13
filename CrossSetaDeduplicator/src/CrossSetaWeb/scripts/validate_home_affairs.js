const https = require('https');

const BASE_URL = 'https://cross-seta-web-17655.fly.dev';

// Test Data
const TEST_CASES = [
    { id: '0002080806082', expectedName: 'Sichumile', expectedSurname: 'Makaula' },
    { id: '0004140927080', expectedName: 'Aphile', expectedSurname: 'Kweyama' },
    { id: '0004180737084', expectedName: 'Gugulethu', expectedSurname: 'Sibiya' },
    { id: '9999999999999', expectedFound: false } // Non-existent
];

function makeRequest(path, method = 'GET') {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: BASE_URL.replace('https://', ''),
            path: path,
            method: method,
            headers: {
                'Content-Type': 'application/json'
            }
        };

        const startTime = Date.now();

        const req = https.request(options, (res) => {
            let data = '';

            res.on('data', (chunk) => {
                data += chunk;
            });

            res.on('end', () => {
                const endTime = Date.now();
                const duration = endTime - startTime;
                try {
                    resolve({ 
                        status: res.statusCode, 
                        data: data ? JSON.parse(data) : {}, 
                        duration 
                    });
                } catch (e) {
                    resolve({ status: res.statusCode, data, duration, error: e });
                }
            });
        });

        req.on('error', (e) => {
            reject(e);
        });

        req.end();
    });
}

async function runValidation() {
    console.log('🚀 Starting Validation Suite...');
    
    // 1. Trigger Import
    console.log('\n📦 Triggering Home Affairs Database Import...');
    try {
        const importRes = await makeRequest('/api/import/trigger', 'POST');
        if (importRes.status === 200 && importRes.data.details && importRes.data.details.success) {
            console.log(`✅ Import Successful! Processed ${importRes.data.details.recordsProcessed} records.`);
        } else {
            console.error('❌ Import Failed:', importRes.data);
            process.exit(1);
        }
    } catch (e) {
        console.error('❌ Import Request Failed:', e);
        process.exit(1);
    }

    // 2. Verify Records
    console.log('\n🔍 Verifying Records...');
    let totalDuration = 0;
    let successCount = 0;

    for (const test of TEST_CASES) {
        try {
            const res = await makeRequest(`/api/verification/home-affairs/${test.id}`);
            
            totalDuration += res.duration;
            const isSuccess = test.expectedFound === false 
                ? (res.data.found === false)
                : (res.data.found === true && 
                   res.data.data.firstName === test.expectedName && 
                   res.data.data.surname === test.expectedSurname);

            if (isSuccess) {
                console.log(`✅ [${test.id}] Passed (${res.duration}ms)`);
                successCount++;
            } else {
                console.error(`❌ [${test.id}] Failed!`);
                console.error('   Expected:', test);
                console.error('   Received:', res.data);
            }
            
            // Sub-100ms check (soft check, log warning)
            if (res.duration > 100) {
                console.warn(`⚠️  [${test.id}] Slow Response: ${res.duration}ms`);
            }

        } catch (e) {
            console.error(`❌ [${test.id}] Exception:`, e);
        }
    }

    // 3. Summary
    console.log('\n📊 Summary');
    console.log(`Total Tests: ${TEST_CASES.length}`);
    console.log(`Passed: ${successCount}`);
    console.log(`Failed: ${TEST_CASES.length - successCount}`);
    console.log(`Avg Response Time: ${(totalDuration / TEST_CASES.length).toFixed(2)}ms`);
    
    if (successCount === TEST_CASES.length) {
        console.log('\n✨ All Validations Passed!');
        process.exit(0);
    } else {
        console.log('\n💥 Some validations failed.');
        process.exit(1);
    }
}

runValidation();
