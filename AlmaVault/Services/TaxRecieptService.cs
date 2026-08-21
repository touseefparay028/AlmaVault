using AlmaVault.Models.Domains;
using AlmaVault.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlmaVault.Services
{
    public class TaxReceiptService : ITaxReceiptService
    {
        private readonly IWebHostEnvironment _env;

        public TaxReceiptService(IWebHostEnvironment env)
        {
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GeneratePdfReceiptAsync(AlumniContribution contribution, string donorName, string donorEmail)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "receipts");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"Receipt_{contribution.TransactionReference}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("TechCity Educational Foundation")
                               .Style(TextStyle.Default.FontSize(18).Bold().FontColor(Colors.Blue.Darken3));

                            col.Item().Text("Institutional Advancement & Alumni Network")
                               .Style(TextStyle.Default.FontSize(9).FontColor(Colors.Grey.Darken1));

                            col.Item().Text("Tax Exempt Org ID: TAX-8839201-X")
                               .Style(TextStyle.Default.FontSize(9).FontColor(Colors.Grey.Darken1));
                        });

                        row.ConstantItem(120).Column(col =>
                        {
                            col.Item().AlignRight().Text("OFFICIAL RECEIPT")
                               .Style(TextStyle.Default.FontSize(12).Bold().FontColor(Colors.Black));

                            col.Item().AlignRight().Text($"Ref: {contribution.TransactionReference}")
                               .Style(TextStyle.Default.FontSize(9));

                            col.Item().AlignRight().Text($"Date: {contribution.ContributedAt:MMM dd, yyyy}")
                               .Style(TextStyle.Default.FontSize(9));
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingVertical(10);

                        col.Item().Text("Received From:").Bold();
                        col.Item().Text($"{donorName} ({donorEmail})");
                        col.Item().PaddingVertical(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Contribution Designation").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Amount").Bold();
                            });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(contribution.Campaign?.Title ?? "General Academic Endowment Fund");

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .AlignRight().Text($"${contribution.Amount:N2}");
                        });

                        col.Item().PaddingVertical(15);
                        col.Item().Text("Tax Exemption Notice:").Bold().FontSize(10);

                        col.Item().Text("No goods or services were provided in exchange for this contribution. TechCity is a registered non-profit educational institution. Retain this official receipt for your tax filings.")
                            .Style(TextStyle.Default.FontSize(9).Italic().FontColor(Colors.Grey.Darken2));

                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("___________________________").FontSize(10);
                                c.Item().Text("Authorized Finance Signatory").FontSize(9).Bold();
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult($"/receipts/{fileName}");
        }
    }
}