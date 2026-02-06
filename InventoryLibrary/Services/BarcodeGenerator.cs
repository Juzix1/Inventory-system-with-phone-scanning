using BarcodeStandard;
using InventoryLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Reflection.Emit;
using SkiaSharp;

public class BarcodeGenerator
{
    string fileName = "";

    public string GenerateBarcodeNumber(int id, int itemTypeId)
    {
        return id.ToString("D6");
    }
    

    public void GenerateBarcode(string barcodeText, string LabelText,string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

        if (string.IsNullOrWhiteSpace(barcodeText))
            throw new ArgumentException("Barcode text cannot be null or empty.", nameof(barcodeText));

        var directory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory part of output path cannot be null or empty.", nameof(outputPath));

        Directory.CreateDirectory(directory);
        Barcode b = new Barcode();
        b.IncludeLabel = true;
        if(!string.IsNullOrWhiteSpace(LabelText))
            b.AlternateLabel = LabelText;
        var skFore = SKColor.Parse("#000000");
        var skBack = SKColor.Parse("#FFFFFF");
        var img = b.Encode(
            BarcodeStandard.Type.Code128B,
            barcodeText,
            skFore,
            skBack,
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
            string filePath = fileName;
            if (System.IO.File.Exists(filePath))
            {
                if(IsLinux())
                {
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
