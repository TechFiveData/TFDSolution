using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.IO;
using System.Web;
using TFDSolution.Business;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Common
{

    public class PdfExporter
    {
        public static void ExportDataSetToPdf(DataSet ds, string folderPath, string fileName, ReportFilterRequest model)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (DataTable table in ds.Tables)
            {
                ExportDataTableToPdf(table, Path.Combine(folderPath, fileName), model);
                break; // only first table (same as your CSV logic)
            }
        }

        public static void ExportDataTableToPdf(DataTable table, string filePath, ReportFilterRequest model)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Document doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // 🔹 Fonts
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

                // 🔹 Header Section (Same as CSV top lines)
                Paragraph company = new Paragraph("Company Name: " + model.CompanyName, titleFont);
                company.Alignment = Element.ALIGN_CENTER;
                doc.Add(company);

                Paragraph report = new Paragraph("Report Title: " + model.ReportName, titleFont);
                report.Alignment = Element.ALIGN_CENTER;
                doc.Add(report);

                if (model.FieldData != null && model.FieldData.Count > 0)
                {
                    foreach (var field in model.FieldData)
                    {
                        Paragraph p = new Paragraph($"{field.FieldName} : {field.FieldText}", normalFont);
                        p.Alignment = Element.ALIGN_CENTER;
                        doc.Add(p);
                    }
                }

                doc.Add(new Paragraph("\n"));

                // 🔹 Table
                PdfPTable pdfTable = new PdfPTable(table.Columns.Count);
                pdfTable.WidthPercentage = 100;
                pdfTable.HeaderRows = 1; // repeat header on each page
                PageSize.A4.Rotate();
                // 🔹 Header Row
                foreach (DataColumn col in table.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, titleFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(135, 206, 235);
                    pdfTable.AddCell(cell);
                }

                // 🔹 Data Rows
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        string value = "";

                        if (row[i] != DBNull.Value && row[i] != null)
                        {
                            value = row[i].ToString();

                            if (value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                value = "";
                        }

                        pdfTable.AddCell(new Phrase(value, normalFont));
                    }
                }

                doc.Add(pdfTable);

                // 🔹 Footer
                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph($"Total Records: {table.Rows.Count}", titleFont));

                doc.Close();
            }
        }
    
        public static void ExportPageDataToPdf(DataTable table, string filePath, string fileName, FormDataModel model)
        {
            try
            {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                string fullPath = Path.Combine(filePath, fileName);

                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    Document doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // 🔹 Fonts
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

                    // ================= HEADER SECTION =================

                    Paragraph p1 = new Paragraph("Company Name: " + model.CompanyName, headerFont);
                    p1.Alignment = Element.ALIGN_CENTER;
                    doc.Add(p1);

                    Paragraph p2 = new Paragraph(model.FormTitle, headerFont);
                    p2.Alignment = Element.ALIGN_CENTER;
                    doc.Add(p2);

                    if (!string.IsNullOrEmpty(model.TabName))
                    {
                        Paragraph p3 = new Paragraph("Detail : " + model.TabName, headerFont);
                        p3.Alignment = Element.ALIGN_CENTER;
                        doc.Add(p3);
                    }

                    Paragraph p4 = new Paragraph("Generated On : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"), normalFont);
                    p4.Alignment = Element.ALIGN_CENTER;
                    doc.Add(p4);

                    doc.Add(new Paragraph("\n"));

                    // ================= TABLE =================

                    int totalColumns = table.Columns.Count + 1; // +1 for serial no
                    PdfPTable pdfTable = new PdfPTable(totalColumns);
                    pdfTable.WidthPercentage = 100;

                    // 🔹 Column Width adjust (optional)
                    float[] widths = new float[totalColumns];
                    widths[0] = 2; // serial column small
                    for (int i = 1; i < totalColumns; i++)
                        widths[i] = 5;

                    pdfTable.SetWidths(widths);

                    // ================= HEADER ROW =================

                    BaseColor skyBlue = new BaseColor(135, 206, 235);

                    PdfPCell cell;

                    // Serial No Header
                    cell = new PdfPCell(new Phrase("#", headerFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = skyBlue;
                    cell.Padding = 5;
                    pdfTable.AddCell(cell);

                    foreach (DataColumn col in table.Columns)
                    {
                        cell = new PdfPCell(new Phrase(col.ColumnName, headerFont));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.BackgroundColor = skyBlue;
                        cell.Padding = 5;
                        pdfTable.AddCell(cell);
                    }

                    pdfTable.HeaderRows = 1;

                    // ================= DATA ROWS =================

                    int iRow = 1;

                    foreach (DataRow row in table.Rows)
                    {
                        // Serial No
                        pdfTable.AddCell(new PdfPCell(new Phrase(iRow.ToString(), normalFont))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });

                        for (int iCol = 0; iCol < table.Columns.Count; iCol++)
                        {
                            string value = "";

                            if (row[iCol] != DBNull.Value && row[iCol] != null)
                            {
                                value = row[iCol].ToString();

                                if (table.Columns[iCol].ColumnName.ToLower() == "status")
                                {
                                    int statusId;
                                    if (int.TryParse(value, out statusId))
                                        value = GetStatus(statusId);
                                }

                                if (value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                                    value = "";
                            }

                            pdfTable.AddCell(new PdfPCell(new Phrase(value, normalFont))
                            {
                                HorizontalAlignment = Element.ALIGN_LEFT,
                                Padding = 4
                            });
                        }

                        iRow++;
                    }

                    doc.Add(pdfTable);

                    // ================= FOOTER =================

                    doc.Add(new Paragraph("\n"));
                    Paragraph footer = new Paragraph("Total Records : " + (iRow - 1), headerFont);
                    footer.Alignment = Element.ALIGN_LEFT;
                    doc.Add(footer);

                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        private static string GetStatus(int statusId)
        {
            switch (statusId)
            {
                case 1:
                    return "Not Approved";
                case 2:
                    return "Approved";
                case 3:
                    return "Cancelled";
                case 4:
                    return "Pending";
                case 5:
                    return "Partial";
                case 6:
                    return "Forwarded";
                case 7:
                    return "Partial Approved";
                default:
                    return "Unknown Status";
            }
        }
    }
}