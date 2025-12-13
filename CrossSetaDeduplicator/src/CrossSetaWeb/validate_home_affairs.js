const axios = require('axios');
const { parse } = require('csv-parse/sync');
const fs = require('fs');

const API_BASE_URL = 'https://cross-seta-web-17655.fly.dev';
const CSV_URL = 'https://docs.google.com/spreadsheets/d/1eQjxSsuOuXU20xG0gGgmR0Agn7WvudJd/export?format=csv&gid=572729852';

async function runTests() {
    console.log('🚀 Starting Comprehensive Home Affairs DB Validation...');
    console.log(`target: ${API_BASE_URL}`);
    
    // 1. Fetch Source Data
    console.log('\n📥 Fetching Source Data from Google Sheet...');
    let records = [];
    try {
        const response = await axios.get(CSV_URL);
        records = parse(response.data, {
            columns: true,
            skip_empty_lines: true
        });
        console.log(`✅ Successfully loaded ${records.length} records from source.`);
    } catch (error) {
        console.error('❌ Failed to fetch source data:', error.message);
        return;
    }

    // 2. Data Integrity & Performance Tests
    console.log('\n🧪 Executing Data Integrity & Performance Tests...');
    
    // Select a random sample of 20 records + some edge cases
    const sampleSize = 20;
    const samples = records.sort(() => 0.5 - Math.random()).slice(0, sampleSize);
    
    let passed = 0;
    let failed = 0;
    let totalTime = 0;
    
    console.log('   Progress: ', end='');
    
    for (const record of samples) {
        const id = record['Identity Number'].trim();
        const expectedName = record['First Name / s'].trim(); // Note: CSV header might be tricky
        const expectedSurname = record['Surname'].trim();
        
        // Skip invalid IDs in source if any (though we want to test valid ones mostly)
        if (id.length !== 13) continue;

        const start = Date.now();
        try {
            const res = await axios.get(`${API_BASE_URL}/api/Verification/home-affairs/${id}`);
            const duration = Date.now() - start;
            totalTime += duration;

            const data = res.data;
            
            // Validation Logic
            let isMatch = true;
            if (!data.found) {
                isMatch = false;
                console.log(`\n   ID ${id} not found in API.`);
            } else if (!data.data) {
                isMatch = false;
                console.log(`\n   ID ${id} returned no data object.`);
            } else {
                if (data.data.nationalID !== id) isMatch = false;
                // Simple fuzzy match or contains check for names as source might have extra spaces/formatting
                if (!data.data.firstName.toLowerCase().includes(expectedName.split(' ')[0].toLowerCase())) isMatch = false; 
                if (data.data.surname.toLowerCase() !== expectedSurname.toLowerCase()) isMatch = false;
            }

            if (isMatch) {
                process.stdout.write('✅');
                passed++;
            } else {
                process.stdout.write('❌');
                console.log(`\n   Mismatch for ID ${id}: Expected ${expectedName} ${expectedSurname}, Got ${data.data.firstName} ${data.data.surname}`);
                failed++;
            }
        } catch (error) {
            process.stdout.write('⚠️');
            console.log(`\n   API Error for ID ${id}: ${error.message}`);
            failed++;
        }
    }
    console.log('\n');

    // 3. Error Handling Tests
    console.log('\n🛡️ Testing Error Handling & Edge Cases...');
    const edgeCases = [
        { id: '0000000000000', desc: 'Invalid ID (All Zeros)', expectedStatus: 200, expectedFound: false }, // API returns 200 with found=false usually
        { id: 'ABC1234567890', desc: 'Non-Numeric ID', expectedStatus: 200, expectedFound: false },
        { id: '', desc: 'Empty ID', expectedStatus: 400, expectedFound: null }, // Should be 400 Bad Request
        { id: '9999999999999', desc: 'Non-Existent ID', expectedStatus: 200, expectedFound: false }
    ];

    for (const case_ of edgeCases) {
        try {
            const res = await axios.get(`${API_BASE_URL}/api/Verification/home-affairs/${case_.id}`, { validateStatus: () => true });
            
            if (res.status === case_.expectedStatus) {
                if (case_.expectedFound !== null) {
                    if (res.data.found === case_.expectedFound) {
                        console.log(`   ✅ ${case_.desc}: Passed (Status ${res.status}, Found=${res.data.found})`);
                    } else {
                         console.log(`   ❌ ${case_.desc}: Failed (Expected Found=${case_.expectedFound}, Got ${res.data.found})`);
                    }
                } else {
                    console.log(`   ✅ ${case_.desc}: Passed (Status ${res.status})`);
                }
            } else {
                console.log(`   ❌ ${case_.desc}: Failed (Expected Status ${case_.expectedStatus}, Got ${res.status})`);
            }
        } catch (err) {
            console.log(`   ⚠️ ${case_.desc}: Error - ${err.message}`);
        }
    }

    // 4. Summary
    const avgTime = (totalTime / sampleSize).toFixed(2);
    console.log('\n📊 TEST SUMMARY');
    console.log('================');
    console.log(`Total Samples: ${sampleSize}`);
    console.log(`Passed:        ${passed}`);
    console.log(`Failed:        ${failed}`);
    console.log(`Avg Latency:   ${avgTime} ms`);
    
    if (avgTime < 100) {
        console.log('⚡ Performance: EXCELLENT (<100ms)');
    } else if (avgTime < 500) {
         console.log('⚡ Performance: GOOD (<500ms)');
    } else {
         console.log('⚠️ Performance: NEEDS IMPROVEMENT (>500ms)');
    }

    if (failed === 0) {
        console.log('\n✅ DATABASE INTEGRATION VERIFIED SUCCESSFUL');
    } else {
        console.log('\n❌ DATABASE INTEGRATION FAILED');
    }
}

runTests();
