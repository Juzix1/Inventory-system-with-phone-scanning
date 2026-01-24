using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using BarcodeStandard;
using System.ComponentModel;
using System.Reflection.Metadata;

namespace InventoryLibrary.Services
{
    public class BarcodePdfService : IBarcodePdfService
    {
        private readonly BarcodeGenerator _barcodeGenerator;

        public BarcodePdfService()
        {
            _barcodeGenerator = new BarcodeGenerator();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateBarcodePdfAsync(List<InventoryItem> items, string companyName)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(content => ComposeContent(content, items, companyName));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                return document.GeneratePdf();
            });
        }

        private void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Kody kreskowe inwentaryzacyjne")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private void ComposeContent(IContainer container, List<InventoryItem> items, string companyName)
        {
            container.Column(column =>
            {
                // Grupowanie po 2 kody na wiersz (A4 może pomieścić 2 kody obok siebie)
                var itemsPerRow = 2;
                var rows = items.Select((item, index) => new { item, index })
                                .GroupBy(x => x.index / itemsPerRow)
                                .Select(g => g.Select(x => x.item).ToList())
                                .ToList();

                foreach (var rowItems in rows)
                {
                    column.Item().Row(row =>
                    {
                        foreach (var item in rowItems)
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                               .Padding(10).Element(c => ComposeBarcodeCard(c, item, companyName));
                        }

                        // Wypełnij puste miejsce jeśli nieparzysty
                        if (rowItems.Count < itemsPerRow)
                        {
                            for (int i = rowItems.Count; i < itemsPerRow; i++)
                            {
                                row.RelativeItem();
                            }
                        }
                    });

                    column.Item().PaddingBottom(15); // Odstęp między wierszami
                }
            });
        }

        private void ComposeBarcodeCard(IContainer container, InventoryItem item, string companyName)
        {
            container.Column(column =>
            {
                // Informacje o przedmiocie
                column.Item().PaddingBottom(5).Column(info =>
                {
                    info.Item().Text($"ID: {item.Id}").FontSize(9).Bold();
                    info.Item().Text(item.itemName).FontSize(11).Bold();
                    info.Item().Text($"Typ: {item.ItemType?.TypeName ?? "Brak"}").FontSize(8);
                    
                    if (item.Location != null)
                    {
                        info.Item().Text($"Lokalizacja: {item.Location.RoomName}").FontSize(8);
                    }
                    else if (item.personInCharge != null)
                    {
                        info.Item().Text($"Osoba: {item.personInCharge.Name}").FontSize(8);
                    }
                });

                // Kod kreskowy
                column.Item().PaddingVertical(5).Element(c =>
                {
                    var barcodeText = _barcodeGenerator.GenerateBarcodeNumber(item.Id, item.ItemTypeId);
                    var barcodeImage = GenerateBarcodeImage(barcodeText, companyName);
                    
                    if (barcodeImage != null)
                    {
                        c.Image(barcodeImage, ImageScaling.FitWidth);
                    }
                });

                // Data ostatniej inwentaryzacji
                column.Item().PaddingTop(5).Text($"Ostatnia inw.: {item.lastInventoryDate:dd.MM.yyyy}")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
            });
        }

        private byte[] GenerateBarcodeImage(string barcodeText, string companyName)
        {
            try
            {
                var barcode = new Barcode
                {
                    IncludeLabel = true,
                    AlternateLabel = !string.IsNullOrWhiteSpace(companyName) ? companyName : null
                };

                var skFore = SKColor.Parse("#000000");
                var skBack = SKColor.Parse("#FFFFFF");
                var fore = new SKColorF(skFore.Red / 255f, skFore.Green / 255f, skFore.Blue / 255f, skFore.Alpha / 255f);
                var back = new SKColorF(skBack.Red / 255f, skBack.Green / 255f, skBack.Blue / 255f, skBack.Alpha / 255f);

                var img = barcode.Encode(
                    BarcodeStandard.Type.Code128B,
                    barcodeText,
                    fore,
                    back,
                    350,
                    150
                );

                using var data = img.Encode(SKEncodedImageFormat.Png, 90);
                using var memoryStream = new MemoryStream();
                data.SaveTo(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating barcode: {ex.Message}");
                return null;
            }
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        }
    }
}