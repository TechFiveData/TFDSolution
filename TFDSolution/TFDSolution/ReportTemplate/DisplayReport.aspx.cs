using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using TFDSolution.Common;
using CrystalDecisions.ReportAppServer;
using TFDSolution.Transport.Master;
using CrystalDecisions.Web;
using TFDSolution.Business.Interface;
using TFDSolution.Business;
//using CrystalDecisions.ReportAppServer.ReportDefModel;


namespace TFDSolution.ReportTemplate
{
    public partial class DisplayReport : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                IMasterBusiness masterBusines = new MasterBusiness();
                string reportId = Request.QueryString["Id"];
                if (string.IsNullOrEmpty(reportId))
                {
                    Response.Write("<script>alert('Missing report file name in query string.');</script>");
                    return;
                }
                else
                {
                    int ReportId = Convert.ToInt32(reportId);
                    ReportMast report = masterBusines.getFormReport(ReportId);
                    /*  if (Session["ReportDocument"] != null)
                      {
                          CrystalReportViewer1.ReportSource = Session["ReportDocument"];
                      }*/
                    LoadReport(report.ReportName);
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ReportDocument"] != null)
            {
                CrystalReportViewer1.ReportSource = Session["ReportDocument"];
            }
        }

        private void LoadReport(string reportFile)
        {
            try
            {
                ReportDocument reportDocument = ReportHelper.LoadAndConfigureReport(reportFile);
                Session["ReportDocument"] = reportDocument;
                CrystalReportViewer1.ReportSource = reportDocument;
                CrystalReportViewer1.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Error loading report: " + ex.Message.Replace("'", "\\'") + "');</script>");
            }
        }

        //protected void Page_Unload(object sender, EventArgs e)
        //{
        //    if (Session["ReportDocument"] != null)
        //    {
        //        ReportDocument report = Session["ReportDocument"] as ReportDocument;
        //        if (report != null)
        //        {
        //            report.Close();
        //            report.Dispose();
        //        }
        //        Session["ReportDocument"] = null;
        //    }
        //}
        /*
        
        private void LoadReportFromPrompt()
        {
            try
            {
                ReportDocument reportDocument = new ReportDocument();
                string reportPath = Server.MapPath("~/ReportTemplate/Sales Order MIS.rpt");
                reportDocument.Load(reportPath);

                // Set DB login - must be done before parameters are filled
                reportDocument.SetDatabaseLogon("sa", "Sa@12345", "TFD", "TFData");

                // Store report document in session
                Session["ReportDocument"] = reportDocument;
                CrystalReportViewer1.ReportSource = reportDocument;
            }
            catch (Exception ex)
            {
                Response.Write("ERROR: " + ex.Message);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ReportDocument"] != null)
            {
                CrystalReportViewer1.ReportSource = Session["ReportDocument"];
            }
        }*/
    }
}