using ClosedXML.Excel;
using CrystalDecisions.ReportAppServer.DataDefModel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TFDSolution.Business;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Common
{
    public class CsvExporter
    {
        public static DataTable ReadCsvToDataTable(string filePath)
        {
            DataTable dt = new DataTable();
            using (var reader = new StreamReader(filePath))
            {
                bool isHeader = true;
                string[] headers = null;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (isHeader)
                    {
                        headers = values;
                        foreach (string header in headers)
                            dt.Columns.Add(header.Trim());
                        isHeader = false;
                    }
                    else
                    {
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }

        public static DataTable ReadExcelToDataTable(string filePath)
        {
            DataTable dt = new DataTable();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();

                bool isHeader = true;

                foreach (var row in rows)
                {
                    if (isHeader)
                    {
                        // 🔹 Read headers
                        foreach (var cell in row.CellsUsed())
                        {
                            string columnName = cell.GetString();

                            // Prevent duplicate column names
                            if (dt.Columns.Contains(columnName))
                                columnName += "_1";

                            dt.Columns.Add(columnName);
                        }
                        isHeader = false;
                    }
                    else
                    {
                        DataRow dr = dt.NewRow();

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            dr[i] = row.Cell(i + 1).GetValue<string>();
                        }

                        dt.Rows.Add(dr);
                    }
                }
            }

            return dt;
        }

        public static void ExportDataSetToCsv(System.Data.DataSet ds, string folderPath, string fileName, ReportFilterRequest model)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            bool isdone = false;
            foreach (DataTable table in ds.Tables)
            {
                if (isdone == false)
                {
                    isdone = true;
                    string filePath = Path.Combine(folderPath, $"{fileName}");
                    ExportDataTableToCsv(table, filePath, model);
                }
            }
        }

        public static void ExportDataTableToCsv(DataTable table, string filePath, ReportFilterRequest model)
        {
            var sb = new StringBuilder();
            // === Add custom top lines ===
            sb.AppendLine("Company Name: " + model.CompanyName + "");
            sb.AppendLine("Report Title: " + model.ReportName + "");
            if (model.FieldData != null && model.FieldData.Count > 0)
            {
                for (int irow = 0; irow < model.FieldData.Count; irow++)
                {
                    sb.Append(model.FieldData[irow].FieldName + " : ");
                    sb.Append(model.FieldData[irow].FieldText);
                    sb.Append(",");
                }
            }
            sb.AppendLine(); // Empty line for spacing
            sb.AppendLine(); // Empty line for spacing
            // Add header
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sb.Append(table.Columns[i].ColumnName);
                if (i < table.Columns.Count - 1)
                    sb.Append(",");
            }
            sb.AppendLine();
            // Add rows
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string value = string.Empty;
                    if (row[i] != null && row[i] != DBNull.Value)
                    {
                        value = row[i].ToString().Trim();

                        if (value.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                            (DateTime.TryParse(value, out DateTime dt) &&
                             (dt == new DateTime(1900, 10, 1) || dt == DateTime.MinValue)))
                        {
                            value = string.Empty;
                        }
                    }
                    value = value.Replace("\"", "\"\"");
                    sb.Append($"\"{value}\"");

                    if (i < table.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine($"Total Records: {Convert.ToString(table.Rows.Count)}");
            // Write to file
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportPageDataToCsv(DataTable table, string filePath, string fileName, FormDataModel model)
        {
            filePath = Path.Combine(filePath, $"{fileName}");
            var sb = new StringBuilder();
            // === Add custom top lines ===
            sb.AppendLine("Company Name: " + model.CompanyName + "");
            sb.AppendLine("Form : " + model.FormTitle + "");
            if (model.TabName != null && model.TabName != "")
            {
                sb.AppendLine("Detail : " + model.FormTitle + "");
            }
            sb.AppendLine(); // Empty line for spacing
            sb.AppendLine(); // Empty line for spacing
            // Add header
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sb.Append(table.Columns[i].ColumnName);
                if (i < table.Columns.Count - 1)
                    sb.Append(",");
            }
            sb.AppendLine();

            // Add rows
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string value = string.Empty;
                    if (row[i] != null && row[i] != DBNull.Value)
                    {
                        value = row[i].ToString().Trim();

                        if (value.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                            (DateTime.TryParse(value, out DateTime dt) &&
                             (dt == new DateTime(1900, 10, 1) || dt == DateTime.MinValue)))
                        {
                            value = string.Empty;
                        }
                    }
                    value = value.Replace("\"", "\"\"");
                    sb.Append($"\"{value}\"");

                    if (i < table.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine($"Total Records: {Convert.ToString(table.Rows.Count)}");
            // Write to file
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportTemplateToCsv(List<FormField> fields, string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var sb = new StringBuilder();
            string filePath = Path.Combine(folderPath, $"{fileName}");
            if (fields != null && fields.Count > 0)
            {
                foreach (var field in fields)
                {
                    sb.Append(field.FieldCaption);
                    sb.Append(",");
                }
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
        }

        public static void ExportLedgerToCsv(LedgerReportSessionCache table, string filePath, string fileName)
        {
            LedgerReportRequest model = table.Request;
            filePath = Path.Combine(filePath, $"{fileName}.csv");
            var sb = new StringBuilder();
            // === Add custom top lines ===
            sb.AppendLine("Company Name: " + model.CompanyName + "");
            sb.AppendLine("Account Ledger Report");
            if (!string.IsNullOrEmpty(model.AccountName) && model.AccountId > 0)
            {
                sb.AppendLine("Account Name: " + model.AccountName + "");
            }
            sb.AppendLine("Period : " + model.FromDate.ToString("dd/MMM/yyyy") + " To " + model.ToDate.ToString("dd/MMM/yyyy"));
            sb.AppendLine("Generated On : " + DateTime.Now.ToString("dd/MMM/yyyy HH:mm tt"));
            sb.AppendLine(); // Empty line for spacing

            // Header
            sb.AppendLine("#,Doc Date,Doc No,Doc Type,Particular,Debit,Credit, Balance,Remarks");

            // Rows
            CultureInfo india = new CultureInfo("en-IN");
            foreach (var item in table.Data)
            {
                decimal balance = item.Balance;
                string labelType = balance >= 0 ? "Cr" : "Dr";
                decimal absBalance = Math.Abs(balance);
                string balanceText = $"₹ {absBalance.ToString("N2", india)} {labelType}";
                if (item.AccountName == "Grand Total" || item.AccountName == "Closing Balance"
                     || item.AccountName == "Opening Balance")
                {
                    sb.AppendLine(string.Join(",", new string[]
                    {
                    "","","","",
                    Escape(item.AccountName),
                    Escape($"₹ {item.Debit_Amount.ToString("N2", india)}"),
                    Escape($"₹ {item.Credit_Amount.ToString("N2", india)}"),
                    Escape(balanceText),
                    ""
                    }));
                }
                else
                {
                    sb.AppendLine(string.Join(",", new string[]
                    {
                    item.RowNo.ToString(),
                    item.DocDate?.ToString("yyyy-MM-dd") ?? "",
                    Escape(item.DocNo),
                    Escape(item.FormTitle),
                    Escape(item.AccountName),
                    Escape($"₹ {item.Debit_Amount.ToString("N2", india)}"),
                    Escape($"₹ {item.Credit_Amount.ToString("N2", india)}"),
                    Escape(balanceText),
                    Escape(item.Remarks)
                    }));
                }
            }
            sb.AppendLine();
            sb.AppendLine($"Total Records: {Convert.ToString(table.TotalRecords)}");
            // Write to file
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        #region Excel

        public static void ExportTemplateToExcel(List<FormField> fields, string folderPath, string fileName, string tabName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var sb = new StringBuilder();
            string filePath = Path.Combine(folderPath, $"{fileName}");
            if (fields != null && fields.Count > 0)
            {
                try
                {
                    int columns = columns = fields.Count + 1;
                    using (var wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(tabName);
                        int currentRow = 1; // Start at row 1
                                            // 🔹 Report Header                        
                                            // 🔹 Report Header (Merged & Centered)
                        var headerRange = ws.Range(currentRow, 1, currentRow, columns); // A1:J1
                        // 🔹 Now add ledger table headers starting from the next row
                        int headerRow = currentRow;
                        int colCount = 1;
                        foreach (var field in fields)
                        {
                            ws.Cell(headerRow, colCount).Value = field.FieldCaption;
                            colCount++;
                        }

                        // Header styling
                        headerRange = ws.Range(headerRow, 1, headerRow, columns);
                        headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                        ws.SheetView.FreezeRows(headerRow);

                        // Auto-fit columns
                        ws.Columns().AdjustToContents();

                        // 🔹 Save file
                        //string folderPath = @"C:\Reports\";
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                        //filePath = Path.Combine(filePath, $"{fileName}");
                        wb.SaveAs(filePath);
                    }
                }
                catch (Exception ex)
                {
                    CommonBusiness.LogEx(ex);
                }
            }
        }

        public static void ExportPageDataToExcel(DataTable table, string filePath, string fileName, FormDataModel model)
        {
            try
            {
                int columns = 0;
                if (table != null && table.Rows.Count > 0)
                {
                    columns = table.Columns.Count + 1;
                }

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(model.FormTitle);
                    int currentRow = 1; // Start at row 1
                                        // 🔹 Report Header
                    #region Company Name
                    // 🔹 Report Header (Merged & Centered)
                    var headerRange = ws.Range(currentRow, 1, currentRow, (columns > 5 ? 5 : columns)); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Company Name: " + model.CompanyName;
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Report Name
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, (columns > 5 ? 5 : columns)); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = model.FormTitle;
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Detail
                    if (model.TabName != null && model.TabName != "")
                    {
                        // 🔹 Report Header (Merged & Centered)
                        headerRange = ws.Range(currentRow, 1, currentRow, (columns > 5 ? 5 : columns)); // A1:J1
                        headerRange.Merge();
                        headerRange.Value = $"Detail : " + model.TabName;
                        // Styling
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        // Optional styling
                        headerRange.Style.Font.FontSize = 12;
                        currentRow++;
                    }
                    #endregion

                    #region Date Generated
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, (columns > 5 ? 5 : columns)); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Generated On : " + DateTime.Now.ToString("dd/mm/yyyy HH:mm tt");
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    // 🔹 Now add ledger table headers starting from the next row
                    int headerRow = currentRow;
                    int colCount = 1;
                    ws.Cell(headerRow, colCount).Value = "#";
                    colCount++;
                    foreach (DataColumn column in table.Columns)
                    {
                        ws.Cell(headerRow, colCount).Value = column.ColumnName;
                        colCount++;
                    }

                    // Header styling
                    headerRange = ws.Range(headerRow, 1, headerRow, columns);
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                    // 🔹 Fill ledger data starting from row after header
                    int dataRow = headerRow + 1, Irow = 1;
                    foreach (DataRow row in table.Rows)
                    {
                        ws.Cell(dataRow, 1).Value = Convert.ToString(Irow);
                        Irow = Irow + 1;
                        for (int iCol = 0; iCol < table.Columns.Count; iCol++)
                        {
                            if (Convert.ToString(table.Columns[iCol].ColumnName).ToLower() == "status")
                            {
                                ws.Cell(dataRow, iCol + 2).Value = GetStatus(Convert.ToInt32(Convert.ToString(row[iCol])));
                            }
                            else
                            {
                                ws.Cell(dataRow, iCol + 2).Value = Convert.ToString(row[iCol]);
                            }
                        }
                        dataRow++;
                    }

                    #region Footer
                    //// 🔹 Report Header (Merged & Centered)
                    //headerRange = ws.Range(currentRow, 1, currentRow, (columns > 5 ? 5 : columns)); // A1:J1
                    //headerRange.Merge();
                    //headerRange.Value = "Total Records : " + Convert.ToString(Irow);
                    //// Styling
                    //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //// Optional styling
                    //headerRange.Style.Font.FontSize = 12;
                    //currentRow++;
                    #endregion

                    // 🔹 Format columns
                    //ws.Column(2).Style.DateFormat.Format = "dd-MM-yyyy";
                    //ws.Columns(6, 8).Style.NumberFormat.Format = "₹ #,##0.00";
                    //ws.Columns(6, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //ws.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    // Freeze header (ledger table)
                    ws.SheetView.FreezeRows(headerRow);

                    // Auto-fit columns
                    ws.Columns().AdjustToContents();

                    // 🔹 Save file
                    //string folderPath = @"C:\Reports\";
                    if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                    filePath = Path.Combine(filePath, $"{fileName}");
                    wb.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        #endregion
        public static void ExportLedgerExcel(LedgerReportSessionCache table, string filePath, string fileName)
        {
            try
            {
                LedgerReportRequest model = table.Request;
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Ledger");
                    int currentRow = 1; // Start at row 1
                                        // 🔹 Report Header
                    #region Company Name
                    // 🔹 Report Header (Merged & Centered)
                    var headerRange = ws.Range(currentRow, 1, currentRow, 9); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Company Name: " + model.CompanyName;
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Report Name
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 9); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Account Ledger Report";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Account Name
                    if (!string.IsNullOrEmpty(model.AccountName) && model.AccountId > 0)
                    {

                        // 🔹 Report Header (Merged & Centered)
                        headerRange = ws.Range(currentRow, 1, currentRow, 9); // A1:J1
                        headerRange.Merge();
                        headerRange.Value = "Account Name: " + model.AccountName;
                        // Styling
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        // Optional styling
                        headerRange.Style.Font.FontSize = 12;
                        currentRow++;
                    }
                    #endregion

                    #region Period
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 9); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = $"Period : {model.FromDate:dd/MMM/yyyy} To {model.ToDate:dd/MMM/yyyy}";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Date Generated
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 9); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Generated On : " + DateTime.Now.ToString("dd/mm/yyyy HH:mm tt");
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    // 🔹 Now add ledger table headers starting from the next row
                    int headerRow = currentRow;
                    ws.Cell(headerRow, 1).Value = "#";
                    ws.Cell(headerRow, 2).Value = "Doc Date";
                    ws.Cell(headerRow, 3).Value = "Doc No";
                    ws.Cell(headerRow, 4).Value = "Doc Type";
                    ws.Cell(headerRow, 5).Value = "Account Name";
                    ws.Cell(headerRow, 6).Value = "Debit";
                    ws.Cell(headerRow, 7).Value = "Credit";
                    ws.Cell(headerRow, 8).Value = "Balance";
                    ws.Cell(headerRow, 9).Value = "Remarks";

                    // Header styling
                    headerRange = ws.Range(headerRow, 1, headerRow, 9);
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                    // 🔹 Fill ledger data starting from row after header
                    int dataRow = headerRow + 1;
                    foreach (var item in table.Data)
                    {
                        if (item.AccountName == "Closing Balance" || item.AccountName == "Opening Balance")
                        {
                            int closingRow = dataRow; // row after last ledger entry
                            // Merge A:E
                            var closingCell = ws.Range(closingRow, 1, closingRow, 4);
                            closingCell.Merge();
                            closingCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            closingCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            closingCell.Style.Font.Bold = true;
                            closingCell.Style.Font.FontColor = XLColor.Black;
                            closingCell.Style.Fill.BackgroundColor = XLColor.LightYellow;

                            closingCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            closingCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            closingCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            closingCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                            ws.Cell(dataRow, 5).Value = item.AccountName;
                            ws.Cell(dataRow, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                            ws.Cell(dataRow, 6).Value = item.Debit_Amount;
                            ws.Cell(dataRow, 6).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 6).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 6).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                            ws.Cell(dataRow, 7).Value = item.Credit_Amount;
                            ws.Cell(dataRow, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 7).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            if (item.Debit_Amount > 0)
                            {
                                ws.Cell(dataRow, 6).Style.Fill.BackgroundColor = XLColor.Red;
                            }
                            if (item.Credit_Amount > 0)
                            {
                                ws.Cell(dataRow, 7).Style.Fill.BackgroundColor = XLColor.Green;
                            }
                            // Balance with Cr/Dr
                            string balanceText = Math.Abs(item.Balance).ToString("₹ #,##0.00") + " " + (item.Balance >= 0 ? "Cr" : "Dr");
                            var balanceCell = ws.Cell(dataRow, 8);
                            balanceCell.Value = balanceText;
                            balanceCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            balanceCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            balanceCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            balanceCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            // 🔹 Set background and font color based on Cr/Dr
                            if (item.Balance >= 0) // Cr
                            {
                                balanceCell.Style.Font.FontColor = XLColor.DarkGreen;
                            }
                            else // Dr
                            {
                                balanceCell.Style.Font.FontColor = XLColor.DarkRed;
                            }

                            ws.Cell(dataRow, 9).Value = item.Remarks;
                            ws.Cell(dataRow, 9).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 9).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 9).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            ws.Cell(dataRow, 9).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                            ws.Range(closingRow, 1, closingRow, 9).Style.Fill.BackgroundColor = XLColor.LightYellow;
                            ws.Range(closingRow, 1, closingRow, 9).Style.Font.Bold = true;
                        }
                        else
                        {
                            if (item.AccountName == "Grand Total")
                            {
                                ws.Cell(dataRow, 1).Value = "";
                                ws.Cell(dataRow, 2).Value = "";
                                ws.Cell(dataRow, 3).Value = "";
                                ws.Cell(dataRow, 4).Value = "";
                                ws.Cell(dataRow, 5).Value = item.AccountName;
                                ws.Cell(dataRow, 5).Style.Font.Bold = true;
                            }
                            else
                            {
                                ws.Cell(dataRow, 1).Value = item.RowNo;
                                ws.Cell(dataRow, 2).Value = item.DocDate;
                                ws.Cell(dataRow, 3).Value = item.DocNo;
                                ws.Cell(dataRow, 4).Value = item.FormTitle;
                                ws.Cell(dataRow, 5).Value = item.AccountName;
                            }


                            ws.Cell(dataRow, 6).Value = item.Debit_Amount;
                            ws.Cell(dataRow, 7).Value = item.Credit_Amount;
                            if (item.Debit_Amount > 0)
                            {
                                ws.Cell(dataRow, 6).Style.Font.FontColor = XLColor.Red;
                            }
                            if (item.Credit_Amount > 0)
                            {
                                ws.Cell(dataRow, 7).Style.Font.FontColor = XLColor.Green;
                            }
                            // Balance with Cr/Dr
                            string balanceText = Math.Abs(item.Balance).ToString("₹ #,##0.00") + " " + (item.Balance >= 0 ? "Cr" : "Dr");
                            var balanceCell = ws.Cell(dataRow, 8);
                            balanceCell.Value = balanceText;
                            // 🔹 Set background and font color based on Cr/Dr
                            if (item.Balance >= 0) // Cr
                            {
                                balanceCell.Style.Font.FontColor = XLColor.Green;
                            }
                            else // Dr
                            {
                                balanceCell.Style.Font.FontColor = XLColor.Red;
                            }
                            ws.Cell(dataRow, 9).Value = item.Remarks;
                            ws.Cell(dataRow, 9).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        }

                        dataRow++;
                    }
                    // 🔹 Format columns
                    ws.Column(2).Style.DateFormat.Format = "dd-MM-yyyy";
                    ws.Columns(6, 8).Style.NumberFormat.Format = "₹ #,##0.00";
                    ws.Columns(6, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    // Freeze header (ledger table)
                    ws.SheetView.FreezeRows(headerRow);

                    // Auto-fit columns
                    ws.Columns().AdjustToContents();

                    // 🔹 Save file
                    //string folderPath = @"C:\Reports\";
                    if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                    filePath = Path.Combine(filePath, $"{fileName}.xlsx");
                    wb.SaveAs(filePath);
                    Console.WriteLine("Excel saved at: " + filePath);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }

        public static void ExportTrailBalanceExcel(TrailBalanceReportSessionCache table, string filePath, string fileName)
        {
            try
            {
                LedgerReportRequest model = table.Request;
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("TrialBalance");
                    int currentRow = 1; // Start at row 1
                                        // 🔹 Report Header
                    #region Company Name
                    // 🔹 Report Header (Merged & Centered)
                    var headerRange = ws.Range(currentRow, 1, currentRow, 8); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Company Name: " + model.CompanyName;
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Report Name
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 8); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Trial Balance Report";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Period
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 8); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = $"Period : {model.FromDate:dd/MMM/yyyy} To {model.ToDate:dd/MMM/yyyy}";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Date Generated
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 8); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Generated On : " + DateTime.Now.ToString("dd/mm/yyyy HH:mm tt");
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    // 🔹 Now add ledger table headers starting from the next row
                    int headerRow = currentRow;
                    ws.Cell(headerRow, 1).Value = "Particular";
                    ws.Cell(headerRow, 2).Value = "Opening Debit";
                    ws.Cell(headerRow, 3).Value = "Opening Credit";
                    ws.Cell(headerRow, 4).Value = "Debit";
                    ws.Cell(headerRow, 5).Value = "Credit";
                    ws.Cell(headerRow, 6).Value = "Closing Debit";
                    ws.Cell(headerRow, 7).Value = "Closing Credit";
                    ws.Cell(headerRow, 8).Value = "Ledger Type";
                    // Header styling
                    headerRange = ws.Range(headerRow, 1, headerRow, 8);
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                    // 🔹 Fill ledger data starting from row after header
                    int dataRow = headerRow + 1;
                    foreach (var item in table.Data)
                    {
                        //ws.Cell(dataRow, 1).Value = item.Level;
                        ws.Cell(dataRow, 1).Value = item.Particular;
                        
                        ws.Cell(dataRow, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 2).Value = item.Op_Debit; //Opening Debit
                        ws.Cell(dataRow, 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 3).Value = item.Op_Credit; //Opening Credit
                        ws.Cell(dataRow, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 4).Value = item.Tr_Debit; //Transaction Debit
                        ws.Cell(dataRow, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 5).Value = item.Tr_Credit;//Transaction Credit
                        ws.Cell(dataRow, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 6).Value = item.Cl_Debit; //Closing Debit
                        ws.Cell(dataRow, 6).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 6).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 6).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 7).Value = item.Cl_Credit; //Closing Credit
                        ws.Cell(dataRow, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 7).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 8).Value = (item.RowType == "LEDGER" ? "L": "G"); //Closing Credit
                        ws.Cell(dataRow, 8).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 8).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 8).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 8).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        if (item.Op_Debit > 0)
                        {
                            ws.Cell(dataRow, 2).Style.Font.FontColor = XLColor.Red;
                        }
                        if (item.Op_Credit > 0)
                        {
                            ws.Cell(dataRow, 3).Style.Font.FontColor = XLColor.Green;
                        }
                        if (item.Tr_Debit > 0)
                        {
                            ws.Cell(dataRow, 4).Style.Font.FontColor = XLColor.Red;
                        }
                        if (item.Tr_Credit > 0)
                        {
                            ws.Cell(dataRow, 5).Style.Font.FontColor = XLColor.Green;
                        }
                        if (item.Cl_Debit > 0)
                        {
                            ws.Cell(dataRow, 6).Style.Font.FontColor = XLColor.Red;
                        }
                        if (item.Cl_Credit > 0)
                        {
                            ws.Cell(dataRow, 7).Style.Font.FontColor = XLColor.Green;
                        }

                        dataRow++;
                    }
                    // 🔹 Format columns
                    ws.Columns(2, 7).Style.NumberFormat.Format = "₹ #,##0.00";                 
                    // Freeze header (ledger table)
                    ws.SheetView.FreezeRows(headerRow);

                    // Auto-fit columns
                    ws.Columns().AdjustToContents();

                    // 🔹 Save file
                    //string folderPath = @"C:\Reports\";
                    if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                    filePath = Path.Combine(filePath, $"{fileName}.xlsx");
                    wb.SaveAs(filePath);
                    Console.WriteLine("Excel saved at: " + filePath);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        public static void ExportProfitLossExcel(ProfitLossReportSessionCache table, string filePath, string fileName)
        {
            try
            {
                LedgerReportRequest model = table.Request;
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("ProfitAndLoss");
                    int currentRow = 1; // Start at row 1
                                        // 🔹 Report Header
                    #region Company Name
                    // 🔹 Report Header (Merged & Centered)
                    var headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Company Name: " + model.CompanyName;
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Report Name
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Profit & Loss Report";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Period
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = $"Period : {model.FromDate:dd/MMM/yyyy} To {model.ToDate:dd/MMM/yyyy}";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Date Generated
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Generated On : " + DateTime.Now.ToString("dd/mm/yyyy HH:mm tt");
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    // 🔹 Now add ledger table headers starting from the next row
                    int headerRow = currentRow;
                    ws.Cell(headerRow, 1).Value = "Particular";
                    ws.Cell(headerRow, 2).Value = "Account Name";
                    ws.Cell(headerRow, 3).Value = "Debit";
                    ws.Cell(headerRow, 4).Value = "Credit";
                    ws.Cell(headerRow, 5).Value = "Amount";

                    // Header styling
                    headerRange = ws.Range(headerRow, 1, headerRow, 5);
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                    // 🔹 Fill ledger data starting from row after header
                    int dataRow = headerRow + 1;
                    foreach (var item in table.Data)
                    {
                        //ws.Cell(dataRow, 1).Value = item.Level;
                        ws.Cell(dataRow, 1).Value = item.Particular;

                        ws.Cell(dataRow, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 2).Value = item.AccountName;

                        ws.Cell(dataRow, 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 3).Value = item.Debit; //Opening Debit
                        ws.Cell(dataRow, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 4).Value = item.Credit; //Opening Credit
                        ws.Cell(dataRow, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 5).Value = item.Amount; //Transaction Debit
                        ws.Cell(dataRow, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                       

                        if (item.Debit > 0)
                        {
                            ws.Cell(dataRow, 3).Style.Font.FontColor = XLColor.Red;
                        }
                        if (item.Credit > 0)
                        {
                            ws.Cell(dataRow, 4).Style.Font.FontColor = XLColor.Green;
                        }
                        if (item.Amount > 0)
                        {
                            ws.Cell(dataRow, 5).Style.Font.FontColor = XLColor.Red;
                        }
                      

                        dataRow++;
                    }
                    // 🔹 Format columns
                    ws.Columns(3, 5).Style.NumberFormat.Format = "₹ #,##0.00";
                    // Freeze header (ledger table)
                    ws.SheetView.FreezeRows(headerRow);

                    // Auto-fit columns
                    ws.Columns().AdjustToContents();

                    // 🔹 Save file
                    //string folderPath = @"C:\Reports\";
                    if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                    filePath = Path.Combine(filePath, $"{fileName}.xlsx");
                    wb.SaveAs(filePath);
                    Console.WriteLine("Excel saved at: " + filePath);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        public static void ExportBalanceSheetExcel(BalanceSheetReportSessionCache table, string filePath, string fileName)
        {
            try
            {
                LedgerReportRequest model = table.Request;
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("ProfitAndLoss");
                    int currentRow = 1; // Start at row 1
                                        // 🔹 Report Header
                    #region Company Name
                    // 🔹 Report Header (Merged & Centered)
                    var headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Company Name: " + model.CompanyName;
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Report Name
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Balance Sheet Report";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Period
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = $"Period : {model.FromDate:dd/MMM/yyyy} To {model.ToDate:dd/MMM/yyyy}";
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    #region Date Generated
                    // 🔹 Report Header (Merged & Centered)
                    headerRange = ws.Range(currentRow, 1, currentRow, 5); // A1:J1
                    headerRange.Merge();
                    headerRange.Value = "Generated On : " + DateTime.Now.ToString("dd/mm/yyyy HH:mm tt");
                    // Styling
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // Optional styling
                    headerRange.Style.Font.FontSize = 12;
                    currentRow++;
                    #endregion

                    // 🔹 Now add ledger table headers starting from the next row
                    int headerRow = currentRow;
                    ws.Cell(headerRow, 1).Value = "Particular";
                    ws.Cell(headerRow, 2).Value = "Account Name";
                    ws.Cell(headerRow, 3).Value = "Debit";
                    ws.Cell(headerRow, 4).Value = "Credit";
                    ws.Cell(headerRow, 5).Value = "Balance";

                    // Header styling
                    headerRange = ws.Range(headerRow, 1, headerRow, 5);
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                    // 🔹 Fill ledger data starting from row after header
                    int dataRow = headerRow + 1;
                    foreach (var item in table.Data)
                    {
                        //ws.Cell(dataRow, 1).Value = item.Level;
                        ws.Cell(dataRow, 1).Value = item.Particular;

                        ws.Cell(dataRow, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 2).Value = item.AccountName;

                        ws.Cell(dataRow, 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 2).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 3).Value = item.Debit; //Opening Debit
                        ws.Cell(dataRow, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 4).Value = item.Credit; //Opening Credit
                        ws.Cell(dataRow, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 4).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell(dataRow, 5).Value = item.Balance; //Transaction Debit
                        ws.Cell(dataRow, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell(dataRow, 5).Style.Border.RightBorder = XLBorderStyleValues.Thin;



                        if (item.Debit > 0)
                        {
                            ws.Cell(dataRow, 3).Style.Font.FontColor = XLColor.Red;
                        }
                        if (item.Credit > 0)
                        {
                            ws.Cell(dataRow, 4).Style.Font.FontColor = XLColor.Green;
                        }
                        if (item.Balance > 0)
                        {
                            ws.Cell(dataRow, 5).Style.Font.FontColor = XLColor.Red;
                        }


                        dataRow++;
                    }
                    // 🔹 Format columns
                    ws.Columns(3, 5).Style.NumberFormat.Format = "₹ #,##0.00";
                    // Freeze header (ledger table)
                    ws.SheetView.FreezeRows(headerRow);

                    // Auto-fit columns
                    ws.Columns().AdjustToContents();

                    // 🔹 Save file
                    //string folderPath = @"C:\Reports\";
                    if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                    filePath = Path.Combine(filePath, $"{fileName}.xlsx");
                    wb.SaveAs(filePath);
                    Console.WriteLine("Excel saved at: " + filePath);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return $"\"{value.Replace("\"", "\"\"")}\"";
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