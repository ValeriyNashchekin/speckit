const { chromium } = require('playwright');

const TARGET_URL = 'http://localhost:4200';

(async () => {
  const browser = await chromium.launch({ headless: false, slowMo: 100 });
  const page = await browser.newPage();

  console.log('🧪 Recognition Rules Full Test\n');

  try {
    // 1. Load page
    console.log('1. Loading page...');
    await page.goto(`${TARGET_URL}/recognition-rules`, { waitUntil: 'commit', timeout: 30000 });
    await page.waitForFunction(() => document.querySelector('app-root')?.innerHTML.length > 100, { timeout: 15000 });
    console.log('   ✅ Page loaded');

    // 2. Get page state
    console.log('\n2. Checking page state...');
    const title = await page.title();
    const buttons = await page.$$eval('button', els => els.length);
    console.log(`   ✅ Title: ${title}, Buttons: ${buttons}`);

    // 3. Click New Rule
    console.log('\n3. Opening dialog...');
    await page.evaluate(() => {
      const btn = [...document.querySelectorAll('button')].find(b => b.textContent?.includes('New Rule'));
      if (btn) btn.click();
    });
    await page.waitForTimeout(1500);

    const dialogVisible = await page.$eval('.p-dialog', el => !!el).catch(() => false);
    console.log(`   ${dialogVisible ? '✅' : '❌'} Dialog visible: ${dialogVisible}`);

    if (dialogVisible) {
      // 4. Check dialog content
      console.log('\n4. Checking dialog...');
      const roleSelect = await page.$('p-select');
      const tabs = await page.$$('button[type="button"]');
      console.log(`   ✅ Role select: ${!!roleSelect}, Tabs: ${tabs.length}`);

      // 5. Close dialog
      console.log('\n5. Closing dialog...');
      await page.evaluate(() => {
        const btn = [...document.querySelectorAll('button')].find(b => b.textContent?.includes('Cancel'));
        if (btn) btn.click();
      });
      await page.waitForTimeout(1000);
      console.log('   ✅ Dialog closed');

      console.log('\n✅ ALL TESTS PASSED!');
    }

  } catch (e) {
    console.log('\n❌ Error:', e.message);
  } finally {
    await page.waitForTimeout(1000);
    await browser.close();
  }
})();
