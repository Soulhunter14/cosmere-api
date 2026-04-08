using UglyToad.PdfPig;
using System.Text;

var pdfPath = args[0];
var outputPath = args[1];
var fromPage = args.Length > 2 ? int.Parse(args[2]) : 1;
var toPage = args.Length > 3 ? int.Parse(args[3]) : int.MaxValue;

var sb = new StringBuilder();

using var document = PdfDocument.Open(pdfPath, new ParsingOptions { UseLenientParsing = true });

toPage = Math.Min(toPage, document.NumberOfPages);
Console.WriteLine($"Total pages: {document.NumberOfPages}, extracting {fromPage}–{toPage}");

for (int i = fromPage; i <= toPage; i++)
{
    var page = document.GetPage(i);
    sb.AppendLine($"\n--- Página {i} ---");
    var words = page.GetWords();
    var lines = words
        .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
        .OrderByDescending(g => g.Key)
        .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));
    foreach (var line in lines)
        sb.AppendLine(line);
    Console.Write($"\r  Processed page {i}/{toPage}");
}

Console.WriteLine();
File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
Console.WriteLine($"Saved to: {outputPath}");
