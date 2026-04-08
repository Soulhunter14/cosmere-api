using PDFtoImage;

// Args: <pdfPath> <outputDir> <page1> [page2] [page3] ...
// Pages are 1-indexed (book page numbers)
if (args.Length < 3)
{
    Console.WriteLine("Usage: pdf_image_extractor <pdfPath> <outputDir> <page1> [page2] ...");
    return 1;
}

var pdfPath = args[0];
var outputDir = args[1];
var pages = args.Skip(2).Select(int.Parse).ToArray();

Directory.CreateDirectory(outputDir);
Console.WriteLine($"PDF: {pdfPath}");
Console.WriteLine($"Output: {outputDir}");
Console.WriteLine($"Pages: {string.Join(", ", pages)}");

// Read PDF into memory once so we can reuse it per page
var pdfBytes = File.ReadAllBytes(pdfPath);

foreach (var page in pages)
{
    var outputFile = Path.Combine(outputDir, $"map_p{page}.png");
    Console.Write($"  Rendering page {page}... ");

    using var pdfStream = new MemoryStream(pdfBytes);
    using var outputStream = File.Create(outputFile);
    // PDFtoImage is 0-indexed, book pages are 1-indexed. 150 DPI for web.
    Conversion.SavePng(outputStream, pdfStream, page: page - 1, options: new RenderOptions(Dpi: 96));

    Console.WriteLine($"saved → {Path.GetFileName(outputFile)}");
}

Console.WriteLine("Done.");
return 0;
