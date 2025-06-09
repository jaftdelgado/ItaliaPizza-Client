using ItaliaPizzaClient.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ItaliaPizzaClient.Utilities
{
    public static class ReportUtils
    {
        private const double TotalWidthCm = 25.94;
        private const int NumberOfColumns = 7;
        private const string HeaderStyleName = "HeaderStyle";
        private const string CellStyleName = "CellStyle";
        private static decimal totalValue = 0;

        [Obsolete]
        public static void ExportarReporteInventario(List<Supply> items, string filePath)
        {
            var doc = CreateDocument();
            var section = ConfigureDocumentSection(doc);

            AddDocumentHeader(section);
            AddDateInformation(section);
            AddInventoryTable(section, items);

            GeneratePdfDocument(doc, filePath);
            OpenPdfDocument(filePath);
        }

        private static Document CreateDocument()
        {
            var doc = new Document();
            ConfigureBaseStyles(doc);
            return doc;
        }

        private static void ConfigureBaseStyles(Document doc)
        {
            var normalStyle = doc.Styles["Normal"];
            normalStyle.Font.Name = "Arial";
            normalStyle.Font.Size = 12;

            var headerStyle = doc.Styles.AddStyle(HeaderStyleName, "Normal");
            headerStyle.Font.Bold = true;
            headerStyle.Font.Size = 11;
            headerStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            var cellStyle = doc.Styles.AddStyle(CellStyleName, "Normal");
            cellStyle.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            cellStyle.ParagraphFormat.SpaceBefore = 0;
            cellStyle.ParagraphFormat.SpaceAfter = 0;
            cellStyle.ParagraphFormat.LineSpacing = Unit.FromPoint(12);
        }

        private static Section ConfigureDocumentSection(Document doc)
        {
            var section = doc.AddSection();
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.Orientation = Orientation.Portrait;
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            return section;
        }

        private static void AddDocumentHeader(Section section)
        {
            var tableHeader = section.AddTable();
            tableHeader.Borders.Visible = false;

            tableHeader.AddColumn(Unit.FromCentimeter(3));  
            tableHeader.AddColumn(Unit.FromCentimeter(15.59));  

            var headerRow = tableHeader.AddRow();

            AddLogoToHeader(headerRow.Cells[0]);

            var infoCell = headerRow.Cells[1];
            var title = infoCell.AddParagraph("Reporte de Inventario");
            title.Format.Font.Size = 18;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceBefore = Unit.FromPoint(10);
            title.Format.SpaceAfter = Unit.FromPoint(5);

            var companyName = infoCell.AddParagraph("ITALIA PIZZA MEXICO");
            companyName.Format.Font.Size = 14;
            companyName.Format.Font.Bold = true;
            companyName.Format.Alignment = ParagraphAlignment.Center;

            AddCenteredParagraph(infoCell, "AV. XALAPA #15");
            AddCenteredParagraph(infoCell, "Col. Obrera Campesina, C.P. 91040, Xalapa, Veracruz, Mexico");
            AddCenteredParagraph(infoCell, "Tel. 2281-22-33-44");

            infoCell.AddParagraph().Format.SpaceAfter = Unit.FromPoint(3);

            var fiscalInfo = infoCell.AddParagraph();
            fiscalInfo.Format.Alignment = ParagraphAlignment.Center;
            fiscalInfo.Format.Font.Size = 10;
            fiscalInfo.AddText("Italia Pizza, S.A. de C.V. en su carácter de asociada en el Contrato de Asociación en Participación denominado ");
            fiscalInfo.AddLineBreak();
            fiscalInfo.AddText("ITALIA PIZZA CONTRATO 1 A EN P");
            fiscalInfo.AddLineBreak();
            fiscalInfo.AddText("R.F.C. ZMC 960801 538");
            fiscalInfo.AddLineBreak();
            fiscalInfo.AddText("Régimen fiscal: REGIMEN GENERAL DE LEY PERSONAS MORALES.");


            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.2);
            AddSeparatorLine(section);

            var descripcion = section.AddParagraph();
            descripcion.Format.Alignment = ParagraphAlignment.Justify;
            descripcion.Format.Font.Size = 10;
            descripcion.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            descripcion.AddText("Reporte detallado de inventario que muestra: ");
            descripcion.AddFormattedText("existencia actual", TextFormat.Bold);
            descripcion.AddText(", ");
            descripcion.AddFormattedText("precio unitario", TextFormat.Bold);
            descripcion.AddText(" y ");
            descripcion.AddFormattedText("valor total del stock", TextFormat.Bold);
            descripcion.AddText(" para cada producto. Los valores totales se calculan como: [Cantidad] × [Precio Unitario].");
        }

        private static void AddCenteredParagraph(Cell cell, string text)
        {
            var paragraph = cell.AddParagraph(text);
            paragraph.Format.Alignment = ParagraphAlignment.Center;
        }

        private static void AddLogoToHeader(Cell cell)
        {
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "italiapizza-icon2Build.png");
            var logo = cell.AddImage(imagePath);
            logo.LockAspectRatio = true;
            logo.Width = Unit.FromCentimeter(2.5);
        }

        private static void AddTitleToHeader(Cell cell)
        {
            var title = cell.AddParagraph("Reporte de Inventario");
            title.Format.Font.Size = 18;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
        }

        private static void AddSeparatorLine(Section section)
        {
            var linea = section.AddParagraph();
            linea.Format.Borders.Bottom.Width = 1;
            linea.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        private static void AddDateInformation(Section section)
        {
            var fecha = section.AddParagraph("Fecha de generación: " + DateTime.Now.ToString("dd/MM/yyyy"));
            fecha.Format.Font.Size = 10;
            fecha.Format.Alignment = ParagraphAlignment.Right;
            fecha.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private static void AddInventoryTable(Section section, List<Supply> items)
        {
            var table = CreateTableWithColumns(section);
            AddTableHeaders(table);
            AddTableData(table, items);
            AddInventoryTotal(section, items);
        }

        private static Table CreateTableWithColumns(Section section)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Rows.LeftIndent = 0;
            table.TopPadding = 3;
            table.BottomPadding = 3;

            double availableWidth = Unit.FromCentimeter(18.59).Centimeter;
            double columnWidth = availableWidth / NumberOfColumns;

            for (int i = 0; i < NumberOfColumns; i++)
            {
                table.AddColumn(Unit.FromCentimeter(columnWidth));
            }

            return table;
        }

        private static void AddTableHeaders(Table table)
        {
            var headers = new[] { "Nombre", "Categoría", "Existencia", "Proveedor", "Unidad", "Precio Unitario", "Total" };
            var tableHeaderRow = table.AddRow();

            tableHeaderRow.Shading.Color = Colors.LightGray;
            tableHeaderRow.Format.Font.Bold = true;
            tableHeaderRow.HeadingFormat = true;
            tableHeaderRow.Format.Alignment = ParagraphAlignment.Center;
            tableHeaderRow.VerticalAlignment = VerticalAlignment.Center;

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = tableHeaderRow.Cells[i];
                cell.AddParagraph(headers[i]);
                cell.Format.Alignment = ParagraphAlignment.Center;
                cell.VerticalAlignment = VerticalAlignment.Center;
            }
        }

        private static void AddTableData(Table table, List<Supply> items)
        {
            bool alternate = false;
            foreach (var item in items)
            {
                var row = table.AddRow();
                if (alternate) row.Shading.Color = Colors.WhiteSmoke;
                alternate = !alternate;
                totalValue += item.Price * (item.Stock ?? 0);

                AddSupplyDataToRow(row, item);
                ConfigureRowCellAlignment(row);
            }
        }

        private static void AddSupplyDataToRow(Row row, Supply item)
        {
        
            row.Cells[0].AddParagraph(item.Name);
            row.Cells[1].AddParagraph(item.CategoryName);
            row.Cells[2].AddParagraph(item.Stock?.ToString() ?? "0");
            row.Cells[3].AddParagraph(item.SupplierName ?? "No asignado");
            row.Cells[4].AddParagraph(MeasureUnit.GetDefaultMeasureUnits()
                .Find(mu => mu.Id == item.MeasureUnit)?.Abbreviation ?? "u");
            row.Cells[5].AddParagraph(item.Price.ToString("C"));
            row.Cells[6].AddParagraph((item.Price * (item.Stock ?? 0)).ToString("C"));
        }

        private static void ConfigureRowCellAlignment(Row row)
        {
            for (int i = 0; i < NumberOfColumns; i++)
            {
                row.Cells[i].Format.Alignment = i >= 2 ? ParagraphAlignment.Center : ParagraphAlignment.Left;
                row.Cells[i].VerticalAlignment = VerticalAlignment.Center;
            }
        }

        private static void AddInventoryTotal(Section section, List<Supply> items)
        {
            section.AddParagraph().Format.SpaceBefore = Unit.FromCentimeter(0.5);
            var totalParrafo = section.AddParagraph();
            totalParrafo.Format.Alignment = ParagraphAlignment.Right;
            totalParrafo.AddFormattedText("Valor total del inventario: ", TextFormat.Bold);
            totalParrafo.AddFormattedText(totalValue.ToString("C"), TextFormat.Bold);
            totalParrafo.Format.Font.Size = 12;
            totalParrafo.Format.SpaceAfter = Unit.FromCentimeter(1);
            totalParrafo.Format.Font.Bold = true;
        }

        [Obsolete]
        private static void GeneratePdfDocument(Document doc, string filePath)
        {
            var renderer = new MigraDoc.Rendering.PdfDocumentRenderer(true);
            renderer.Document = doc;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }

        private static void OpenPdfDocument(string filePath)
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
    }
}