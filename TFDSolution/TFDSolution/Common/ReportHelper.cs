using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DocumentFormat.OpenXml.Office2010.Excel;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Common
{
    public class ReportHelper
    {
        private static SqlConnectionStringBuilder GetReportConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CrystalReportConnection"].ConnectionString;

            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            return builder;
        }
        // Helper Method to Apply Connection
        private static void ApplyConnection(ReportDocument reportDocument, SqlConnectionStringBuilder builder)
        {
            foreach (CrystalDecisions.CrystalReports.Engine.Table table in reportDocument.Database.Tables)
            {
                TableLogOnInfo logOnInfo = table.LogOnInfo;
                logOnInfo.ConnectionInfo.ServerName = builder.DataSource;
                logOnInfo.ConnectionInfo.DatabaseName = builder.InitialCatalog;
                logOnInfo.ConnectionInfo.UserID = builder.UserID;
                logOnInfo.ConnectionInfo.Password = builder.Password;
                table.ApplyLogOnInfo(logOnInfo);

                // Very Important: SetLocation to blank to avoid error
                table.Location = table.Name;
            }
        }

        public static ReportDocument LoadAndConfigureReport(string reportFileName)
        {
            ReportDocument reportDocument = new ReportDocument();
            try
            {
                // Ensure the exportFileName ends with .pdf
                if (!reportFileName.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase))
                {
                    reportFileName += ".rpt";
                }

                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportFileName);
                reportDocument.Load(fullPath);

                SqlConnectionStringBuilder builder = GetReportConnection();
                // Apply Connection to Main Report Tables
                ApplyConnection(reportDocument, builder);
                // Apply Connection to Sub Reports (if any)
                foreach (Section section in reportDocument.ReportDefinition.Sections)
                {
                    foreach (ReportObject reportObject in section.ReportObjects)
                    {
                        if (reportObject.Kind == ReportObjectKind.SubreportObject)
                        {
                            SubreportObject subReportObject = (SubreportObject)reportObject;
                            ReportDocument subReportDocument = subReportObject.OpenSubreport(subReportObject.SubreportName);
                            ApplyConnection(subReportDocument, builder);
                        }
                    }
                }
            }
            catch
            {
                reportDocument.Dispose();
                throw; // rethrow to handle it in calling page
            }
            return reportDocument;
        }

        public static ResponseModel ExportSystemReport(ReportMast report, string exportFileName, List<SpecificationField> reportParameters, bool isOpenImmediatly = true)
        {
            ResponseModel response = new ResponseModel();
            ReportDocument reportDocument = new ReportDocument();
            string exportPath = string.Empty;
            string reportPathWithExtention = report.ReportName;
            try
            {
                // Ensure the exportFileName ends with .pdf
                if (!exportFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    exportFileName += ".pdf";
                }
                exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportReport");
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                exportPath = Path.Combine(exportPath, exportFileName);
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
                reportPathWithExtention = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportPathWithExtention);

                reportDocument.Load(reportPathWithExtention);
                List<object> parameters = new List<object>();

                // Get all parameter fields
                foreach (ParameterFieldDefinition param in reportDocument.DataDefinition.ParameterFields)
                {
                    string paramName = param.Name;
                    Console.WriteLine("Parameter Name : " + paramName);
                }
                // Dynamic RecordSelectionFormula
                string formula = string.Empty;
                if (reportParameters != null)
                {
                    List<string> conditions = new List<string>();
                    foreach (var param in reportParameters)
                    {
                        if (!string.IsNullOrEmpty(param.FieldName) && param.FieldName == "RequestUserId")
                        {
                            // Assuming all parameters are string type, adjust if needed
                            conditions.Add("{" + report.SQLTableName + "." + param.FieldName + "} = '" + param.FieldValue.Replace("'", "''") + "'");
                        }
                    }
                    formula = string.Join(" AND ", conditions);
                }
                //reportDocument.RecordSelectionFormula = "{" + report.SQLTableName + ".RequestUserId} = " + Id;
                reportDocument.RecordSelectionFormula = formula;
                SqlConnectionStringBuilder builder = GetReportConnection();
                // Apply Connection to Main Report Tables
                ApplyConnection(reportDocument, builder);
                // Apply Connection to Sub Reports (if any)
                foreach (Section section in reportDocument.ReportDefinition.Sections)
                {
                    foreach (ReportObject reportObject in section.ReportObjects)
                    {
                        if (reportObject.Kind == ReportObjectKind.SubreportObject)
                        {
                            SubreportObject subReportObject = (SubreportObject)reportObject;
                            ReportDocument subReportDocument = subReportObject.OpenSubreport(subReportObject.SubreportName);
                            ApplyConnection(subReportDocument, builder);
                        }
                    }
                }
                // Export to PDF
                reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, exportPath);
                //if (isOpenImmediatly)
                //{
                //    // Open the exported PDF
                //    System.Diagnostics.Process.Start(exportPath);
                //}
                response.Response = exportFileName;
                response.IsSuccess = true;
                //response.Response = "Report is opening..";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Error Occured, " + ex.Message;
                CommonBusiness.LogEx(ex);
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
            return response;
        }
        public static ResponseModel ExportReport(int Id, ReportMast report, string exportFileName, bool isOpenImmediatly = true)
        {
            ResponseModel response = new ResponseModel();
            ReportDocument reportDocument = new ReportDocument();
            string exportPath = string.Empty;
            string reportPathWithExtention = report.ReportName;
            try
            {
                // Ensure the exportFileName ends with .pdf
                if (!exportFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    exportFileName += ".pdf";
                }
                exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportReport");
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                exportPath = Path.Combine(exportPath, exportFileName);
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
                reportPathWithExtention = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportPathWithExtention);

                reportDocument.Load(reportPathWithExtention);
                reportDocument.RecordSelectionFormula = "{" + report.SQLTableName + ".Id} = " + Id;
                SqlConnectionStringBuilder builder = GetReportConnection();
                // Apply Connection to Main Report Tables
                ApplyConnection(reportDocument, builder);
                // Apply Connection to Sub Reports (if any)
                foreach (Section section in reportDocument.ReportDefinition.Sections)
                {
                    foreach (ReportObject reportObject in section.ReportObjects)
                    {
                        if (reportObject.Kind == ReportObjectKind.SubreportObject)
                        {
                            SubreportObject subReportObject = (SubreportObject)reportObject;
                            ReportDocument subReportDocument = subReportObject.OpenSubreport(subReportObject.SubreportName);
                            ApplyConnection(subReportDocument, builder);
                        }
                    }
                }
                // Export to PDF
                reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, exportPath);
                //if (isOpenImmediatly)
                //{
                //    // Open the exported PDF
                //    System.Diagnostics.Process.Start(exportPath);
                //}
                response.Response = exportFileName;
                response.IsSuccess = true;
                //response.Response = "Report is opening..";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Error Occured, " + ex.Message;
                CommonBusiness.LogEx(ex);
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
            return response;
        }
    }
}