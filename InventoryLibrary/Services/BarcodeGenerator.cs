using BarcodeStandard;
using InventoryLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Reflection.Emit;

public class BarcodeGenerator
{
    string fileName = "";

    public async Task<string> GenerateBarcodeNumber(MyDbContext _context)
    {
        var item = await _context.InventoryItems
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return "11111111";
        }
        else
        {
            int number = int.Parse(item.Barcode) + 1;
            return number.ToString("D8"); // Ensure the number is 8 digits long
        }
    }

    public void GenerateBarcode(string barcodeText, string outputPath)
    {
        // Ensure outputPath is not null or empty
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

        var directory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory part of output path cannot be null or empty.", nameof(outputPath));

        Directory.CreateDirectory(directory);

        Barcode b = new Barcode();
        b.IncludeLabel = true;
        var img = b.Encode(
            BarcodeStandard.Type.Code128B,
            barcodeText,
            SkiaSharp.SKColor.Parse("#000000"),
            SkiaSharp.SKColor.Parse("#FFFFFF"),
            400,
            200
        );

        using (var data = img.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 90))
        using (var stream = System.IO.File.OpenWrite(outputPath))
        {
            data.SaveTo(stream);
            fileName = outputPath;
        }
    }

    public void deleteBarcodeImage()
    {
        try
        {
            //w przyszłości, po prostu usunąć wszystkie pliki
            string filePath = fileName;
            if (System.IO.File.Exists(filePath))
            {
                if(IsLinux())
                {
                    // On Linux, we might need to use a different path separator or handle permissions
                    filePath = filePath.Replace('\\', '/');
                }
                System.IO.File.Delete(filePath);
            }
            else
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting barcode image: {ex.Message}");
        }
    }
    public static bool IsLinux()
    {
        return OperatingSystem.IsLinux();
    }
}
