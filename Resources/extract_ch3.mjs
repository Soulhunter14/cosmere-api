import { getDocument } from './pdfextract/node_modules/pdfjs-dist/legacy/build/pdf.mjs';
import { writeFileSync } from 'fs';
import { resolve } from 'path';

const pdfPath = resolve('ART0001_MdAdlT_ESP_high.pdf');
const outputFile = 'pdfextract/ch3_chars.txt';

async function extractPages(startPage, endPage) {
  const loadingTask = getDocument({
    url: 'file:///' + pdfPath.split('\\').join('/'),
    verbosity: 0
  });
  const pdfDoc = await loadingTask.promise;
  console.log('Total pages:', pdfDoc.numPages);

  let result = 'Total pages: ' + pdfDoc.numPages + '\n\n';

  for (let i = startPage; i <= Math.min(endPage, pdfDoc.numPages); i++) {
    const page = await pdfDoc.getPage(i);
    const textContent = await page.getTextContent();
    const text = textContent.items.map(item => item.str).join(' ');
    result += '=== PAGE ' + i + ' ===\n' + text + '\n\n';
  }

  writeFileSync(outputFile, result);
  console.log('Done! Written to', outputFile);
}

extractPages(47, 68).catch(console.error);
