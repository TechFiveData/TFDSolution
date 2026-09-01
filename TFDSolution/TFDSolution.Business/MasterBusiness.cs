using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Xml.Linq;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;
using static System.Collections.Specialized.BitVector32;

namespace TFDSolution.Business
{
    public class MasterBusiness : IMasterBusiness
    {
        public MasterBusiness() { }
        public List<FormMast> getUserForms(System.Guid UserId, int RoleId = 0)
        {
            List<FormMast> form = new List<FormMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    FormMast formMast = getFormData("");
                    if (formMast != null)
                    {
                        form = formMast.ChildForm;
                    }
                    if (form != null)
                    {
                        var AllChildForms = getChildFormListWithPermission(RoleId, "");//Get All Child Forms
                        foreach (var item in form)
                        {
                            var childForms = AllChildForms.Where(x => x.ParentFormId == item.FormId).OrderBy(x => x.FormTitle).ToList();
                            item.ChildForm = childForms;
                            // If all child forms have NoAccess == true or PArent form don't have any child form , set parent NoAccess = true
                            if (!childForms.Any())
                            {
                                item.NoAccess = true;
                            }
                            else if (childForms.Any() && childForms.All(x => x.NoAccess))
                            {
                                item.NoAccess = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return form;
        }
        public FormMast getFormData(string formId)
        {
            FormMast formMast = new FormMast();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    if (!string.IsNullOrEmpty(formId))
                    {
                        if (Guid.TryParse(formId, out Guid guid))
                        {
                            formMast = getForms(guid).FirstOrDefault();
                        }
                        //SqlParameter ParamId = new SqlParameter("@FormId", formId);
                        //formMast = context.Database.SqlQuery<FormMast>("EXEC m_getFormData @FormId", ParamId).FirstOrDefault();
                        //m_getFormData_Result _data = context.m_getFormData(formId).FirstOrDefault();
                        if (formMast != null)
                        {
                            m_FormMast parentForm = context.m_FormMast.Where(x => x.FormId == formMast.ParentFormId).FirstOrDefault();
                            if (parentForm != null)
                            {
                                formMast.ParentFormName = parentForm.FormName;
                                formMast.ParentFormTitle = parentForm.FormTitle;
                            }
                        }
                        if (formMast == null)
                        {
                            formMast = new FormMast();
                            formMast.FormTitle = "Form Settings";
                        }
                    }
                    List<FormField> formFields = new List<FormField>();
                    if (!string.IsNullOrEmpty(formId))
                    {
                        List<m_getFormFields_Result> fields = context.m_getFormFields(formId, "", "").ToList();
                        if (fields != null && fields.Count > 0)
                        {
                            foreach (var item in fields)
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldId = item.FieldId.ToString(),
                                    FormId = item.FormId.ToString(),
                                    FormTabId = item.FormTabId.ToString(),
                                    FieldTypeId = item.FieldTypeId.ToString(),
                                    IsActive = item.IsActive,
                                    FieldName = item.FieldName,
                                    FieldCaption = item.FieldCaption,
                                    FieldLength = item.FieldLength,
                                    FormSectionId = item.FormSectionId.ToString(),
                                    IsRequired = item.IsRequired,
                                    CreatedOn = item.CreatedOn,
                                    UpdatedOn = item.UpdatedOn,
                                    ParameterId = item.ParameterId,
                                    FieldType = item.FieldType,
                                    TabName = item.TabName,
                                    SectionName = item.SectionName
                                });
                            }
                        }
                    }
                    formMast.FormFields = formFields;
                    formMast.ChildForm = getFormList(formId);
                    formMast.FormFieldTypes = GetFormFieldTypeList();
                    formMast.FormSection = GetFormSections();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formMast;
        }

        public List<FormMast> getFormList(string formId)
        {
            List<FormMast> formMast = new List<FormMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@FormId", formId);
                    formMast = context.Database.SqlQuery<FormMast>("EXEC m_getForms @FormId", param1).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formMast;
        }
        public List<FormSystemModel> getFormSystemList(string formId)
        {
            List<FormSystemModel> formMast = new List<FormSystemModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@FormId", formId);
                    formMast = context.Database.SqlQuery<FormSystemModel>("EXEC m_getFormsSystem @FormId", param1).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formMast;
        }
        public List<FormMast> getChildFormListWithPermission(int RoleID, string formId)
        {
            List<FormMast> formMast = new List<FormMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@RoleId", RoleID);
                    SqlParameter param2 = new SqlParameter("@ParentFormId", formId);
                    formMast = context.Database.SqlQuery<FormMast>("EXEC proc_GetUserFormWithPermissions @RoleId,@ParentFormId", param1, param2).ToList();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formMast;
        }

        #region Form Tab

        public FormTab GetFormTab(string tabId)
        {
            FormTab formTab = new FormTab();
            try
            {
                Guid _tabId = Guid.Parse(tabId);
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@TabId", _tabId);
                    formTab = context.Database.SqlQuery<FormTab>("EXEC m_getFormTaByFormTabId @TabId", param1).FirstOrDefault();
                    if (formTab == null)
                    {
                        formTab = new FormTab();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formTab;
        }

        public ResponseModel DeleteTab(string tabId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                Guid _tabId = Guid.Parse(tabId);
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@TabId", tabId);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_DeleteTab @TabId", param1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception Occurred: " + ex.Message;
            }
            return response;
        }

        public ResponseModel SaveTab(FormTab mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    bool isNew = false;
                    m_FormTab m_tab = new m_FormTab();
                    Guid formId = Guid.Parse(mast.FormId);
                    Guid secId = Guid.Parse(mast.SectionId);
                    string _tablname = Convert.ToString(mast.TabName).Trim();
                    if (string.IsNullOrEmpty(mast.FormTabId))
                    {
                        m_FormTab exist = context.m_FormTab.Where(x => x.TabName == _tablname && x.FormId == formId && x.FormSectionId == secId).FirstOrDefault();
                        if (exist != null)
                        {
                            response.Action = "Validation";
                            response.Response = "The " + mast.TabName + " tab is already exists.";
                            response.IsSuccess = false;
                            return response;
                        }
                        m_tab.FormTabId = Guid.NewGuid();
                        isNew = true;
                    }
                    else
                    {
                        Guid tabid = CommonBusiness.ConvertGUID(mast.FormTabId);
                        m_tab = context.m_FormTab.Where(x => x.FormTabId == tabid).FirstOrDefault();
                    }
                    if (m_tab != null)
                    {
                        m_tab.TabTitle = mast.TabTitle;
                        m_tab.IsActive = mast.IsActive;
                        if (mast.SortOrder != null && mast.SortOrder > 0)
                        {
                            m_tab.SortOrder = mast.SortOrder;
                        }
                        else
                        {
                            int maxSortOrder = context.m_FormTab
                              .Where(f => f.FormId == formId && f.FormSectionId == secId)
                              .Select(f => (int?)f.SortOrder)
                              .Max() ?? 0; // If no records, default to 0

                            m_tab.SortOrder = maxSortOrder + 1;
                            // **Sort Order Calculation**
                        }
                        if (isNew)
                        {
                            m_tab.TabName = _tablname;
                            m_tab.TabSQLTableName = mast.TabSQLTableName;
                            m_tab.FormId = formId;
                            m_tab.FormSectionId = secId;
                            m_tab.CreatedOn = DateTime.Now;
                            //context.m_FormTab.Add(m_tab);
                        }
                        else
                        {
                            m_tab.UpdatedOn = DateTime.Now;
                        }

                        var param1 = new SqlParameter("@FormId", m_tab.FormId);
                        var param2 = new SqlParameter("@FormSectionId", m_tab.FormSectionId);
                        var param3 = new SqlParameter("@FormTabId", m_tab.FormTabId);
                        var param4 = new SqlParameter("@TabName", m_tab.TabName);
                        var param5 = new SqlParameter("@TabTitle", m_tab.TabTitle);
                        var param6 = new SqlParameter("@NextTabDataScript", (mast.NextTabDataScript != null ? Convert.ToString(mast.NextTabDataScript) : ""));
                        //var param7 = new SqlParameter("@EnableInlineForm", SqlDbType.Bit, mast.EnableInlineForm);
                        //var param7 = new SqlParameter("@EnableInlineForm", SqlDbType.Bit) { Value = mast.EnableInlineForm };
                        response = context.Database.SqlQuery<ResponseModel>("EXEC proc_AddTab @FormId, @FormSectionId, @FormTabId, @TabName, @TabTitle,@NextTabDataScript", param1, param2, param3, param4, param5, param6).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Record not save successfully.";
                response.IsSuccess = false;
            }
            return response;
        }
        #endregion
        public List<FormTab> geSectionTabs(string formId, string sectionId)
        {
            List<FormTab> tabs = new List<FormTab>();
            Guid _formid = Guid.Parse(formId);
            Guid _sectionid = Guid.Parse(sectionId);
            using (var context = new TFDSolutionEntities())
            {
                List<m_FormTab> fields = context.m_FormTab.Where(x => x.FormId == _formid && x.FormSectionId == _sectionid)
                    .OrderBy(x => x.CreatedOn) // Order by SortOrder in ascending order
                    .ToList();
                if (fields != null && fields.Count > 0)
                {
                    foreach (var item in fields.OrderBy(x => x.SortOrder))
                    {
                        tabs.Add(new FormTab()
                        {
                            FormTabId = item.FormTabId.ToString().ToUpper(),
                            IsActive = item.IsActive,
                            TabName = item.TabName,
                            TabTitle = item.TabTitle,
                            TabSQLTableName = item.TabSQLTableName,
                            SortOrder = (item.SortOrder.HasValue ? item.SortOrder.Value : 0),
                        });
                    }
                }
            }
            return tabs;
        }
        public List<FormField> getFormFieldList(string formId, string SectionId, string TabId)
        {
            List<FormField> formFields = new List<FormField>();
            using (var context = new TFDSolutionEntities())
            {
                var param1 = new SqlParameter("@FormId", formId);
                var param2 = new SqlParameter("@SectionId", SectionId);
                var param3 = new SqlParameter("@TabId", TabId);
                formFields = context.Database.SqlQuery<FormField>("EXEC m_getFormFields @FormId, @SectionId, @TabId", param1, param2, param3).ToList();
            }
            return formFields;
        }

        public List<GridField> getFieldDependency(SubmitFormModel model)
        {
            List<GridField> response = new List<GridField>();
            List<FieldDependencySQL> formFields = new List<FieldDependencySQL>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@tabId", model.TabId);
                    var param2 = new SqlParameter("@FromFieldName", model.FromFieldName);
                    var param3 = new SqlParameter("@FieldId", (model.FieldId != null ? Convert.ToString(model.FieldId) : ""));
                    formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC proc_getDependencyFields @tabId, @FromFieldName, @FieldId", param1, param2, param3).ToList();
                    if (formFields != null && formFields.Count > 0)
                    {
                        foreach (var item in formFields)
                        {
                            string sqlQuery = item.SQLQuery;
                            if (sqlQuery.Contains("{CompanyId}"))
                                sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + Convert.ToString(model.CompanyId) + "'");
                            if (sqlQuery.Contains("{CreatedBy}"))
                                sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + Convert.ToString(model.UserId) + "'");
                            if (sqlQuery.Contains("@Head_") == false && sqlQuery.Contains("{ParentId}"))
                                sqlQuery = sqlQuery.Replace("{ParentId}", "'" + Convert.ToString(model.ParentId) + "'");
                            if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                            {
                                foreach (var field in model.FieldData)
                                {
                                    if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                        sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                                }
                            }
                            if (model != null && model.HeaderFieldData != null && model.HeaderFieldData.Count > 0)
                            {
                                foreach (var field in model.HeaderFieldData)
                                {
                                    string pattern = $@"@Head_{Regex.Escape(field.FieldName)}\s*=\s*\{{ParentId\}}";

                                    sqlQuery = Regex.Replace(
                                        sqlQuery,
                                        pattern,
                                        $"@Head_{field.FieldName} = '{Convert.ToString(field.FieldValue)}'"
                                    );
                                }
                            }
                            if (response != null && response.Count > 0)
                            {
                                foreach (var item1 in response)
                                {
                                    if (item1.FieldValue != null && item1.FieldValue.Count > 0)
                                    {
                                        foreach (var itemval in item1.FieldValue)
                                        {
                                            if (sqlQuery.Contains("{" + item1.FieldName + "}"))
                                                sqlQuery = sqlQuery.Replace("{" + item1.FieldName + "}", "'" + Convert.ToString(itemval.FieldValue) + "'");
                                        }
                                    }
                                }
                            }
                            if (sqlQuery.Contains("{") == false)
                            {
                                DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                                //var data = context.Database.SqlQuery<object>(sqlQuery).ToList();
                                if (dst != null && dst.Tables.Count > 0)
                                {
                                    if (dst.Tables[0].Rows.Count > 0)
                                    {
                                        List<PageFieldData> fieldDatas = new List<PageFieldData>();
                                        foreach (DataRow Ditem in dst.Tables[0].Rows)
                                        {
                                            foreach (DataColumn col in Ditem.Table.Columns)
                                            {
                                                string columnName = col.ColumnName;
                                                string value = Convert.ToString(Ditem[columnName]);
                                                fieldDatas.Add(new PageFieldData()
                                                {
                                                    FieldName = col.ColumnName,
                                                    FieldValue = value
                                                });
                                            }
                                        }
                                        response.Add(new GridField()
                                        {
                                            FieldName = item.FieldName,
                                            FieldValue = fieldDatas
                                        });
                                    }
                                    else if (dst.Tables[0].Columns.Count > 0)
                                    {
                                        List<PageFieldData> fieldDatas = new List<PageFieldData>();
                                        foreach (DataColumn col in dst.Tables[0].Columns)
                                        {
                                            fieldDatas.Add(new PageFieldData()
                                            {
                                                FieldName = col.ColumnName,
                                                FieldValue = ""
                                            });
                                        }
                                        response.Add(new GridField()
                                        {
                                            FieldName = item.FieldName,
                                            FieldValue = fieldDatas
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        public List<GridField> getHeaderFieldDependency(SubmitFormModel model)
        {
            List<GridField> response = new List<GridField>();
            List<FieldDependencySQL> formFields = new List<FieldDependencySQL>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", model.FormId);
                    var param2 = new SqlParameter("@CompanyId", model.CompanyId);
                    var param3 = new SqlParameter("@FromFieldName", model.FromFieldName);
                    var param4 = new SqlParameter("@FieldId", (model.FieldId == null ? "" : Convert.ToString(model.FieldId)));
                    formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC proc_getHeaderDependencyFields @FormId, @CompanyId, @FromFieldName, @FieldId",
                        param1, param2, param3, param4).ToList();
                    if (formFields != null && formFields.Count > 0)
                    {
                        foreach (var item in formFields)
                        {
                            if (!string.IsNullOrEmpty(item.SQLQuery))
                            {
                                string sqlQuery = item.SQLQuery;
                                if (sqlQuery.Contains("{CompanyId}"))
                                    sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + Convert.ToString(model.CompanyId) + "'");
                                if (sqlQuery.Contains("{CreatedBy}"))
                                    sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + Convert.ToString(model.UserId) + "'");
                                if (sqlQuery.Contains("{Id}"))
                                    sqlQuery = sqlQuery.Replace("{Id}", "'" + Convert.ToString(model.Id) + "'");
                                if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                                {
                                    foreach (var field in model.FieldData)
                                    {
                                        if (field.FieldValue != string.Empty && field.FieldValue != null && Convert.ToString(field.FieldValue).Length > 0)
                                        {
                                            if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                                sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                                        }
                                        else if (field.IsRequired == false)
                                        {
                                            if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                                sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "''");
                                        }
                                    }
                                }
                                if (sqlQuery.Contains("{") == false)
                                {
                                    DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                                    //var data = context.Database.SqlQuery<object>(sqlQuery).ToList();
                                    if (dst != null && dst.Tables.Count > 0)
                                    {
                                        if (dst.Tables[0].Rows.Count > 0)
                                        {
                                            List<PageFieldData> fieldDatas = new List<PageFieldData>();
                                            foreach (DataRow Ditem in dst.Tables[0].Rows)
                                            {
                                                foreach (DataColumn col in Ditem.Table.Columns)
                                                {
                                                    string columnName = col.ColumnName;
                                                    string value = Convert.ToString(Ditem[columnName]);
                                                    fieldDatas.Add(new PageFieldData()
                                                    {
                                                        FieldName = col.ColumnName,
                                                        FieldValue = value
                                                    });
                                                }
                                            }
                                            response.Add(new GridField()
                                            {
                                                FieldName = item.FieldName,
                                                FieldValue = fieldDatas
                                            });
                                        }
                                        else if (dst.Tables[0].Columns.Count > 0)
                                        {
                                            List<PageFieldData> fieldDatas = new List<PageFieldData>();
                                            foreach (DataColumn col in dst.Tables[0].Columns)
                                            {
                                                fieldDatas.Add(new PageFieldData()
                                                {
                                                    FieldName = col.ColumnName,
                                                    FieldValue = ""
                                                });
                                            }
                                            response.Add(new GridField()
                                            {
                                                FieldName = item.FieldName,
                                                FieldValue = fieldDatas
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        public List<FormField> getFormFieldList(string pageName)
        {
            List<FormField> formFields = new List<FormField>();
            using (var context = new TFDSolutionEntities())
            {
                var param4 = new SqlParameter("@PageName", pageName);
                formFields = context.Database.SqlQuery<FormField>("EXEC proc_GetDataTableFields @PageName", param4).ToList();
            }
            return formFields;
        }

        public List<FormMast> getForms(Guid? formId)
        {
            List<FormMast> formMast = new List<FormMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var clientIdParameter = new SqlParameter("@FormId", formId.HasValue ? (object)formId.Value : DBNull.Value);

                    //var clientIdParameter = new SqlParameter("@FormId", System.Data.SqlDbType.UniqueIdentifier).Value = formId;
                    formMast = context.Database.SqlQuery<FormMast>("exec m_getFormData @FormId", clientIdParameter).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formMast;
        }
        public List<clsSelection> getSelection(string selectionType, string CompanyId, string whareClause = "", string orderBy = "", string entryType = "")
        {
            List<clsSelection> selection = new List<clsSelection>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    //selection = context.m_getSelection(selectionType, whareClause, orderBy, entryType).Select(x => new clsSelection()
                    //{
                    //    Text = x.OptionText,
                    //    Value = x.OptionValue.ToString()
                    //}).ToList();
                    var param1 = new SqlParameter("@SelectionType", selectionType);
                    var param2 = new SqlParameter("@WhereClause", (object)whareClause ?? DBNull.Value);
                    var param3 = new SqlParameter("@OrderBy", (object)orderBy ?? DBNull.Value);
                    var param4 = new SqlParameter("@EntryType", (object)entryType ?? DBNull.Value);
                    var param5 = new SqlParameter("@CompanyId", (object)CompanyId ?? DBNull.Value);

                    selection = context.Database
                        .SqlQuery<clsSelection>(
                            "EXEC m_getFormSelection @SelectionType,@WhereClause,@OrderBy,@EntryType,@CompanyId",
                            param1, param2, param3, param4, param5
                        ).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return selection;
        }

        public ResponseModel DeleteFormSetting(DeleteFormSetting modal)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@PrimaryId", modal.PrimaryId);
                    var param2 = new SqlParameter("@SourceTable", modal.Source);

                    // Use SqlQuery<string>() since we expect a list of table names (strings)
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteTabField @PrimaryId, @SourceTable", param1, param2).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        public FormField GetFormField(string fieldId)
        {
            FormField formField = new FormField();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    Guid _fieldId = Guid.Parse(fieldId);
                    var param1 = new SqlParameter("@FieldId", fieldId);
                    formField = context.Database.SqlQuery<FormField>("EXEC proc_getFormField @FieldId", param1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formField;
        }

        #region Company
        public ResponseModel AddCompany(CompanyMast mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    proc_AddCompany_Result result = context.proc_AddCompany(
                        mast.CompanyCode, mast.CompanyName, mast.UserId.ToString(), mast.Email, mast.Phone, mast.Mobile).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.Response = result.Response;
                        response.IsSuccess = result.IsSuccess;
                        response.PrimaryId = result.PrimaryId;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        public CompanyData GetCompanyData()
        {
            CompanyData company = new CompanyData();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    List<CompanyMast> companies = new List<CompanyMast>();
                    companies = context.Database.SqlQuery<CompanyMast>("EXEC proc_getCompanyList").ToList();
                    if (companies != null && companies.Count > 0)
                    {
                        foreach (var item in companies)
                        {
                            var finan = context.m_CompanyFinancials.Where(x => x.CompanyId == item.CompanyId).ToList();
                            if (finan != null && finan.Count > 0)
                            {
                                item.Financials = finan.Select(x => new CompanyFinancial()
                                {
                                    Alias = x.Alias,
                                    EndDate = x.EndDate,
                                    IsActive = x.IsActive,
                                    CompanyId = x.CompanyId,
                                    StartDate = x.StartDate,
                                    FiancialYearId = x.FiancialYearId,
                                }).ToList();
                            }
                        }
                    }
                    company.Company = companies;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return company;
        }
        public List<CompanyMast> GetCompanies()
        {
            List<CompanyMast> companies = new List<CompanyMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    companies = context.Database.SqlQuery<CompanyMast>("EXEC proc_getCompanyList").ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return companies;
        }

        public List<CompanyFinancial> GetCompanyFinancial(string CompanyId)
        {
            List<CompanyFinancial> financials = new List<CompanyFinancial>();
            try
            {
                Guid cId = Guid.Parse(CompanyId);
                using (var context = new TFDSolutionEntities())
                {
                    financials = context.m_CompanyFinancials.Where(x => x.CompanyId == cId).Select(x => new CompanyFinancial()
                    {
                        Alias = x.Alias,
                        EndDate = x.EndDate,
                        IsActive = x.IsActive,
                        CompanyId = x.CompanyId,
                        StartDate = x.StartDate,
                        FiancialYearId = x.FiancialYearId,
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return financials;
        }

        public ResponseModel SaveCompanyFinancial(CompanyFinancial mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    m_CompanyFinancials m_Item = context.m_CompanyFinancials.Where(x => x.FiancialYearId == mast.FiancialYearId).FirstOrDefault();
                    if (m_Item == null)
                    {
                        m_Item = new m_CompanyFinancials();
                        m_Item.StartDate = mast.StartDate;
                        m_Item.EndDate = mast.EndDate;
                        m_Item.Alias = mast.Alias;
                        m_Item.CreatedBy = mast.UserId;
                        m_Item.CreatedOn = DateTime.Now;
                        m_Item.IsActive = mast.IsActive;
                        m_Item.CompanyId = mast.CompanyId;
                        context.m_CompanyFinancials.Add(m_Item);
                        context.SaveChanges();
                    }
                    else
                    {
                        m_Item.IsActive = mast.IsActive;
                        m_Item.Alias = mast.Alias;
                        m_Item.UpdatedBy = mast.UserId;
                        m_Item.UpdatedOn = DateTime.Now;
                        context.SaveChanges();
                    }
                    response.Action = "Save";
                    response.IsSuccess = true;
                    response.Response = "Record save successfully.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.getFormUserApprovalSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Action = "Error";
                response.Response = "Record not save successfully.";
                response.IsSuccess = false;
            }
            return response;
        }

        public CompanyMast GetCompany(string guid)
        {
            CompanyMast company = new CompanyMast();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@UId", guid);
                    company = context.Database.SqlQuery<CompanyMast>("EXEC proc_getCompany @UId", param1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return company;
        }

        public ResponseModel SaveCompany(CompanyMast mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    if (mast.CompanyId != null)
                    {
                        m_CompanyMast m_Company = context.m_CompanyMast.Where(x => x.CompanyId == mast.CompanyId).FirstOrDefault();
                        if (m_Company != null)
                        {
                            // Prepare parameters for the stored procedure
                            var parameters = new List<SqlParameter> {
                                new SqlParameter("@CompanyId", m_Company.CompanyId),
                                new SqlParameter("@CINNo", mast.CINNo ?? (object)DBNull.Value),
                                new SqlParameter("@ECCNo", mast.ECCNo ?? (object)DBNull.Value),
                                new SqlParameter("@PANNo", mast.PANNo ?? (object)DBNull.Value),
                                new SqlParameter("@Phone", mast.Phone ?? (object)DBNull.Value),
                                new SqlParameter("@FactoryPincode", mast.FactoryPincode ?? (object)DBNull.Value),
                                new SqlParameter("@RegisterPincode", mast.RegisterPincode ?? (object)DBNull.Value),
                                new SqlParameter("@CompanyName", mast.CompanyName ?? (object)DBNull.Value),
                                new SqlParameter("@UpdatedBy", Convert.ToString(mast.UserId)),
                                new SqlParameter("@Fax", mast.Fax ?? (object)DBNull.Value),
                                new SqlParameter("@Email", mast.Email ?? (object)DBNull.Value),
                                new SqlParameter("@FactoryAddress1", mast.FactoryAddress1 ?? (object)DBNull.Value),
                                new SqlParameter("@FactoryAddress2", mast.FactoryAddress2 ?? (object)DBNull.Value),
                                new SqlParameter("@FactoryCityId", SqlDbType.Int) { Value = mast.FactoryCityId ?? (object)DBNull.Value },
                                new SqlParameter("@FactoryCountryId", SqlDbType.Int) { Value = mast.FactoryCountryId ?? (object)DBNull.Value },
                                new SqlParameter("@FactoryStateId", SqlDbType.Int) { Value = mast.FactoryStateId ?? (object)DBNull.Value },
                                new SqlParameter("@GSTNo", mast.GSTNo ?? (object)DBNull.Value),
                                new SqlParameter("@WebStite", mast.WebStite ?? (object)DBNull.Value),
                                new SqlParameter("@Mobile", mast.Mobile ?? (object)DBNull.Value),
                                new SqlParameter("@RegisterAddress1", mast.RegisterAddress1 ?? (object)DBNull.Value),
                                new SqlParameter("@RegisterAddress2", mast.RegisterAddress2 ?? (object)DBNull.Value),
                                new SqlParameter("@RegisterCityId", SqlDbType.Int) { Value = mast.RegiterCityId ??(object) DBNull.Value },
                                new SqlParameter("@RegisterStateId", SqlDbType.Int) { Value = mast.RegiterStateId ??(object) DBNull.Value },
                                new SqlParameter("@RegisterCountryId", SqlDbType.Int) { Value = mast.RegisterCountryId ??(object) DBNull.Value },
                                new SqlParameter("@IECCode", mast.IECCode ?? (object)DBNull.Value),
                                new SqlParameter("@DefaultCurrencyId", SqlDbType.Int) { Value = mast.DefaultCurrencyId ?? (object)DBNull.Value },
                                new SqlParameter("@FromEmail", mast.FromEmail ?? (object)DBNull.Value),
                                new SqlParameter("@Password", mast.FromPassword ?? (object)DBNull.Value),
                                new SqlParameter("@DisplayName", mast.DisplayName ?? (object)DBNull.Value),
                                new SqlParameter("@SmtpServer", mast.SmtpServer ?? (object)DBNull.Value),
                                new SqlParameter("@SmtpPort", SqlDbType.Int) {Value = mast.SmtpPort ?? (object)DBNull.Value},
                                new SqlParameter("@EnableSSL", mast.EnableSSL),
                                new SqlParameter("@RegistrationNo", mast.RegistrationNo ?? (object)DBNull.Value),
                                new SqlParameter("@RegistrationDate", mast.RegistrationDate ?? (object)DBNull.Value),
                                new SqlParameter("@MSMENo", mast.MSMENo ?? (object)DBNull.Value),
                                new SqlParameter("@TANNo", mast.TANNo ?? (object)DBNull.Value),
                                new SqlParameter("@PFNo", mast.PFNo?? (object)DBNull.Value),
                                new SqlParameter("@ESICNo", mast.ESICNo ?? (object)DBNull.Value),
                                new SqlParameter("@PrefessionTaxNo", mast.PrefessionTaxNo ?? (object)DBNull.Value),
                                new SqlParameter("@LWFPrefessionNo", mast.LWFPrefessionNo ?? (object)DBNull.Value)};
                            response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveCompany @CompanyId,@CINNo,@ECCNo,@PANNo,@Phone,@FactoryPincode,@RegisterPincode," +
                                "@CompanyName,@UpdatedBy,@Fax,@Email,@FactoryAddress1,@FactoryAddress2,@FactoryCityId,@FactoryCountryId,@FactoryStateId,@GSTNo,@WebStite,@Mobile," +
                                "@RegisterAddress1,@RegisterAddress2,@RegisterCityId,@RegisterStateId,@RegisterCountryId," +
                                "@IECCode,@DefaultCurrencyId,@FromEmail,@Password,@DisplayName,@SmtpServer,@SmtpPort,@EnableSSL," +
                                "@RegistrationNo,@RegistrationDate,@MSMENo,@TANNo,@PFNo,@ESICNo,@PrefessionTaxNo,@LWFPrefessionNo"
                                , parameters.ToArray()).FirstOrDefault();
                            response.Action = "Save";
                            response.PrimaryId = mast.CompanyId;
                            response.IsSuccess = true;
                            response.Response = "Record save successfully.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Record not save successfully.";
                response.IsSuccess = false;
            }
            return response;
        }
        public ResponseModel DeleteCompany(Guid guid)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var company = context.m_CompanyMast.FirstOrDefault(c => c.CompanyId == guid);
                    if (company == null)
                    {
                        response.Action = "Delete";
                        response.IsSuccess = false;
                        response.Response = "Record not found.";
                        return response;
                    }
                    context.m_CompanyMast.Remove(company);
                    context.SaveChanges();
                    response.Action = "Delete";
                    response.PrimaryId = company.CompanyId;
                    response.IsSuccess = true;
                    response.Response = "Record deleted successfully.";

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Error deleting record!";
                response.IsSuccess = false;
            }
            return response;
        }
        #endregion

        #region Item

        public List<ItemMast> GetItemList()
        {
            List<ItemMast> itemsList = new List<ItemMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    itemsList = context.Database.SqlQuery<ItemMast>("EXEC proc_getItemList").ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return itemsList;
        }
        public ItemMast GetItem(string guid)
        {
            ItemMast Item = new ItemMast();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var oId = new SqlParameter("@UId", guid);
                    //context.proc_getItem`
                    Item = context.Database.SqlQuery<ItemMast>("EXEC proc_getItem @UId", @oId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Item;
        }

        public ResponseModel SaveItem(ItemMast mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {

            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Record not save successfully.";
                response.IsSuccess = false;
            }
            return response;
        }
        public ResponseModel DeleteItem(Guid guid)
        {
            ResponseModel response = new ResponseModel();
            try
            {

            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Error deleting record!";
                response.IsSuccess = false;
            }
            return response;
        }
        #endregion Item

        #region Party
        public List<PartyMst> GetPartyList()
        {
            List<PartyMst> partyList = new List<PartyMst>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    partyList = context.proc_getPartyList().Select(x => new PartyMst()
                    {
                        PartyId = x.PartyId,
                        PartyCode = x.PartyCode,
                        PartyType = x.PartyType,
                        PartyName = x.PartyName,
                        AlternateName = x.AlternateName,
                        PartyGroup = x.PartyGroup,
                        Currency = x.Currency,
                        Telephone1 = x.Telephone1,
                        Telephone2 = x.Telephone2,
                        Mobile = x.Mobile,
                        Email = x.Email,
                        WebSite = x.WebSite,
                        Fax = x.Fax,
                        ContactPerson = x.ContactPerson,
                        Industry = x.Industry,
                        PartyStatus = Convert.ToBoolean(x.PartyStatus),
                        SalesPerson = x.SalesPerson,
                        Remarks = x.Remarks,

                        BillToName = x.BillToName,
                        BillToAddress1 = x.BillToAddress1,
                        BillToAddress2 = x.BillToAddress2,
                        BillToCity = x.BillToCity,
                        BillToState = x.BillToState,
                        BillToCountry = x.BillToCountry,
                        BillToPincode = x.BillToPincode,
                        BillToContactNo = x.BillToContactNo,
                        BillToContactPerson = x.BillToContactPerson,
                        BillToEmail = x.BillToEmail,

                        ShipToName = x.ShipToName,
                        ShipToAddress1 = x.ShipToAddress1,
                        ShipToAddress2 = x.ShipToAddress2,
                        ShipToCity = x.ShipToCity,
                        ShipToState = x.ShipToState,
                        ShipToCountry = x.ShipToCountry,
                        ShipToPincode = x.ShipToPincode,
                        ShipToContactNo = x.ShipToContactNo,
                        ShipToContactPerson = x.ShipToContactPerson,
                        ShipToEmail = x.ShipToEmail,

                        GSTNo = x.GSTNo,
                        GSTType = x.GSTType,
                        PANNo = x.PANNo,
                        CreatedBy = x.CreatedBy,
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedOn = x.UpdatedOn
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return partyList;
        }
        public PartyMst GetParty(string guid)
        {
            PartyMst Party = new PartyMst();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    Party = context.proc_getParty(guid).Select(x => new PartyMst()
                    {
                        PartyId = x.PartyId,
                        PartyCode = x.PartyCode,
                        PartyType = x.PartyType,
                        PartyName = x.PartyName,
                        AlternateName = x.AlternateName,
                        PartyGroup = x.PartyGroup,
                        Currency = x.Currency,
                        Telephone1 = x.Telephone1,
                        Telephone2 = x.Telephone2,
                        Mobile = x.Mobile,
                        Email = x.Email,
                        WebSite = x.WebSite,
                        Fax = x.Fax,
                        ContactPerson = x.ContactPerson,
                        Industry = x.Industry,
                        PartyStatus = Convert.ToBoolean(x.PartyStatus),
                        SalesPerson = x.SalesPerson,
                        Remarks = x.Remarks,

                        BillToName = x.BillToName,
                        BillToAddress1 = x.BillToAddress1,
                        BillToAddress2 = x.BillToAddress2,
                        BillToCity = x.BillToCity,
                        BillToState = x.BillToState,
                        BillToCountry = x.BillToCountry,
                        BillToPincode = x.BillToPincode,
                        BillToContactNo = x.BillToContactNo,
                        BillToContactPerson = x.BillToContactPerson,
                        BillToEmail = x.BillToEmail,

                        ShipToName = x.ShipToName,
                        ShipToAddress1 = x.ShipToAddress1,
                        ShipToAddress2 = x.ShipToAddress2,
                        ShipToCity = x.ShipToCity,
                        ShipToState = x.ShipToState,
                        ShipToCountry = x.ShipToCountry,
                        ShipToPincode = x.ShipToPincode,
                        ShipToContactNo = x.ShipToContactNo,
                        ShipToContactPerson = x.ShipToContactPerson,
                        ShipToEmail = x.ShipToEmail,

                        GSTNo = x.GSTNo,
                        GSTType = x.GSTType,
                        PANNo = x.PANNo,
                        CreatedBy = x.CreatedBy,
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedOn = x.UpdatedOn

                    }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Party;
        }

        public ResponseModel SaveParty(PartyMst mast)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Execute the stored procedure m_SaveParty
                    int result = context.Database.ExecuteSqlCommand(
                        "EXEC m_SaveParty " +
                        "@PartyId, @PartyCode, @PartyType, @PartyName, @AlternateName, @PartyGroup, @Currency, " +
                        "@Telephone1, @Telephone2, @Mobile, @Email, @WebSite, @Fax, @ContactPerson, @Industry, " +
                        "@PartyStatus, @SalesPerson, @Remarks, " +
                        "@BillToName, @BillToAddress1, @BillToAddress2, @BillToCity, @BillToState, @BillToCountry, @BillToPincode, " +
                        "@BillToContactNo, @BillToContactPerson, @BillToEmail, " +
                        "@ShipToName, @ShipToAddress1, @ShipToAddress2, @ShipToCity, @ShipToState, @ShipToCountry, @ShipToPincode, " +
                        "@ShipToContactNo, @ShipToContactPerson, @ShipToEmail, " +
                        "@GSTNo, @GSTType, @PANNo, @UserId",

                        new SqlParameter("@PartyId", mast.PartyId == Guid.Empty ? (object)DBNull.Value : mast.PartyId),
                        new SqlParameter("@PartyCode", mast.PartyCode),
                        new SqlParameter("@PartyType", mast.PartyType),
                        new SqlParameter("@PartyName", mast.PartyName),
                        new SqlParameter("@AlternateName", mast.AlternateName),
                        new SqlParameter("@PartyGroup", mast.PartyGroup),
                        new SqlParameter("@Currency", mast.Currency),
                        new SqlParameter("@Telephone1", mast.Telephone1),
                        new SqlParameter("@Telephone2", mast.Telephone2),
                        new SqlParameter("@Mobile", mast.Mobile),
                        new SqlParameter("@Email", mast.Email),
                        new SqlParameter("@WebSite", mast.WebSite),
                        new SqlParameter("@Fax", mast.Fax),
                        new SqlParameter("@ContactPerson", mast.ContactPerson),
                        new SqlParameter("@Industry", mast.Industry),
                        new SqlParameter("@PartyStatus", mast.PartyStatus),
                        new SqlParameter("@SalesPerson", mast.SalesPerson),
                        new SqlParameter("@Remarks", mast.Remarks),

                        new SqlParameter("@BillToName", mast.BillToName),
                        new SqlParameter("@BillToAddress1", mast.BillToAddress1),
                        new SqlParameter("@BillToAddress2", mast.BillToAddress2),
                        new SqlParameter("@BillToCity", mast.BillToCity),
                        new SqlParameter("@BillToState", mast.BillToState),
                        new SqlParameter("@BillToCountry", mast.BillToCountry),
                        new SqlParameter("@BillToPincode", mast.BillToPincode),
                        new SqlParameter("@BillToContactNo", mast.BillToContactNo),
                        new SqlParameter("@BillToContactPerson", mast.BillToContactPerson),
                        new SqlParameter("@BillToEmail", mast.BillToEmail),

                        new SqlParameter("@ShipToName", mast.ShipToName),
                        new SqlParameter("@ShipToAddress1", mast.ShipToAddress1),
                        new SqlParameter("@ShipToAddress2", mast.ShipToAddress2),
                        new SqlParameter("@ShipToCity", mast.ShipToCity),
                        new SqlParameter("@ShipToState", mast.ShipToState),
                        new SqlParameter("@ShipToCountry", mast.ShipToCountry),
                        new SqlParameter("@ShipToPincode", mast.ShipToPincode),
                        new SqlParameter("@ShipToContactNo", mast.ShipToContactNo),
                        new SqlParameter("@ShipToContactPerson", mast.ShipToContactPerson),
                        new SqlParameter("@ShipToEmail", mast.ShipToEmail),

                        new SqlParameter("@GSTNo", mast.GSTNo),
                        new SqlParameter("@GSTType", mast.GSTType),
                        new SqlParameter("@PANNo", mast.PANNo),

                        new SqlParameter("@UserId", mast.UserId)

                    );

                    response.Action = "Save";
                    response.PrimaryId = mast.PartyId;
                    response.IsSuccess = result > 0;
                    response.Response = result > 0 ? "Record saved successfully." : "No record was inserted.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Record not saved successfully. Error: " + ex.Message;
                response.IsSuccess = false;
            }

            return response;
        }

        public ResponseModel DeleteParty(Guid guid)
        {
            ResponseModel response = new ResponseModel();
            return response;
        }
        #endregion Party


        #region Form Master

        public ResponseModel SaveForm(FormMast mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formIdParam = new SqlParameter("@FormId", (object)mast.FormId ?? DBNull.Value);
                    var formNameParam = new SqlParameter("@FormName", mast.FormName);
                    var formDescParam = new SqlParameter("@FormDescription", (object)mast.FormDescription ?? DBNull.Value);
                    var formTitleParam = new SqlParameter("@FormTitle", (object)mast.FormTitle ?? DBNull.Value);
                    var isActiveParam = new SqlParameter("@IsActive", mast.IsActive);
                    var pFormIdParam = new SqlParameter("@PFormId", (object)mast.PFormId ?? DBNull.Value);
                    var sqlTableParam = new SqlParameter("@SQLTableName", (object)mast.SQLTableName ?? DBNull.Value);
                    var companyIdParam = new SqlParameter("@CompanyId", mast.CompanyId);
                    var prefixParam = new SqlParameter("@Prefix", (object)mast.Prefix ?? DBNull.Value);
                    var aliasParam = new SqlParameter("@Alias", (object)mast.FormAlias ?? DBNull.Value);
                    var sqlTemplateIdParam = new SqlParameter("@SqlTemplateId", (object)mast.SqlTemplateId ?? DBNull.Value);

                    // Capture stored procedure result set
                    var result = context.Database.SqlQuery<ResponseModel>(
                        "EXEC proc_SaveFormMast @FormId, @FormName, @FormDescription, @FormTitle, @IsActive, @PFormId, @SQLTableName, @CompanyId, @Prefix, @Alias, @SqlTemplateId",
                        formIdParam, formNameParam, formDescParam, formTitleParam, isActiveParam, pFormIdParam, sqlTableParam, companyIdParam, prefixParam, aliasParam, sqlTemplateIdParam
                    ).FirstOrDefault();

                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.PrimaryId = result.PrimaryId;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Record not saved successfully. Error: " + ex.Message;
            }
            return response;
        }

        public ResponseModel SaveFormCaption(FormDataModel mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formIdParam = new SqlParameter("@FormId", (object)mast.FormId ?? DBNull.Value);
                    var formTitleParam = new SqlParameter("@FormTitle", (object)mast.FormTitle ?? DBNull.Value);
                    var result = context.Database.SqlQuery<ResponseModel>(
                        "EXEC proc_SetFromData @FormId,@FormTitle",
                        formIdParam, formTitleParam).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Record not saved successfully. Error: " + ex.Message;
            }
            return response;
        }

        public List<FormFieldData> GetFormFieldData(string tblName)
        {
            List<FormFieldData> returnData = new List<FormFieldData>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string Sql = "SELECT * FROM " + tblName;
                    //DataSet dataSet = new DataSet();
                    //SqlDataAdapter adapter = new SqlDataAdapter("Select Count(*) From Login where Username='" + usernameTextfield.Text + "' and Password = '" + passwordTextfield.Text + "'", connection);
                    //DataTable dt = new DataTable();
                    //adapter.Fill(dt);
                    returnData = context.Database.SqlQuery<FormFieldData>(Sql).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                returnData = new List<FormFieldData>();
            }
            return returnData;
        }

        public PageTotalData getPageTotal(string formId, int parentId, string userId)
        {
            PageTotalData pageTotal = new PageTotalData();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FormId", Value = (object)formId ?? DBNull.Value });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", DbType = DbType.Int32, Value = parentId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = (object)userId ?? DBNull.Value });
                        command.CommandText = "dbo.proc_getPageTotal";
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = command.ExecuteReader())
                        {
                            // Read SubTotal
                            if (reader.Read())
                            {
                                List<PageTotal> _totals = new List<PageTotal>();
                                Dictionary<string, object> obj = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    _totals.Add(new PageTotal() { Name = reader.GetName(i), Amount = Convert.ToDecimal(reader.GetValue(i)) });
                                }
                                pageTotal.Totals = _totals;
                                // Process subTotal
                            }

                            reader.NextResult();
                            List<PageCharges> list = new List<PageCharges>();
                            // Read OtherCharges
                            while (reader.Read())
                            {
                                var taxChargeName = reader["TaxChargeName"] as string;
                                var efferct = reader["Effect"] as string;
                                var changeValue = Convert.ToInt32(reader["ChangeValue"]);
                                var therChargesDetailId = Convert.ToInt32(reader["OtherChargesDetailId"]);
                                var amount = Convert.ToDecimal(reader.GetValue(3));
                                list.Add(new PageCharges
                                {
                                    Amount = amount,
                                    TaxChargeName = taxChargeName,
                                    ChangeValue = changeValue,
                                    OtherChargesDetailId = therChargesDetailId,
                                    Effect = efferct
                                });
                            }
                            pageTotal.OtherChargeDetail = list;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return pageTotal;
        }

        public ResponseModel SaveOtherChargesItem(ItemOtherChargeData model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.proc_SaveOtherChargesItem";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter { ParameterName = "@formId", Value = model.FormId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = model.ParentId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@OtherChargesDetailId", Value = model.OtherChargesDetailId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@TaxChargeName", Value = model.TaxChargeName });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ChargesAmont", Value = model.Amount });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = model.UserId });
                        command.ExecuteNonQuery();
                        response.IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception Occured: " + ex.Message;
            }
            return response;
        }

        public ResponseModel SavePageCharges(PageOtherCharges model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DataTable itemTable = new DataTable();
                itemTable.Columns.Add("DetailId", typeof(int));
                itemTable.Columns.Add("ParentId", typeof(int));
                itemTable.Columns.Add("FormId", typeof(string));
                itemTable.Columns.Add("OtherChargesDetailId", typeof(int));
                itemTable.Columns.Add("TaxChargeName", typeof(string));
                itemTable.Columns.Add("Percentage", typeof(decimal));
                itemTable.Columns.Add("Amount", typeof(decimal));
                itemTable.Columns.Add("CreatedBy", typeof(Guid)); // This is both CreatedBy/UpdatedBy

                foreach (var item in model.FieldData)
                {
                    itemTable.Rows.Add(model.DetailId, model.ParentId, model.FormId, item.OtherChargesDetailId, item.TaxChargeName, item.Percentage, item.Amount, model.UserId);
                }
                // Set up the connection and command
                using (var context = new TFDSolutionEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.proc_SaveOtherCharges";
                        command.CommandType = CommandType.StoredProcedure;

                        var param = new SqlParameter();
                        param.ParameterName = "@ItemDetails";
                        param.SqlDbType = SqlDbType.Structured;
                        param.SourceColumn = "dbo.SaveOtherCharges";
                        param.Value = itemTable;
                        command.Parameters.Add(param);

                        command.ExecuteNonQuery();
                        response.IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception Occured.";
            }
            return response;
        }

        public ResponseModel PageDataValidation(SubmitFormModel request)
        {
            ResponseModel response = new ResponseModel();
            response.IsSuccess = true;
            List<PageFieldData> pageFields = new List<PageFieldData>();
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("FieldName", typeof(string));
                permissionTable.Columns.Add("FieldValue", typeof(string));
                permissionTable.Columns.Add("DataType", typeof(string));
                if (request.FieldData != null && request.FieldData.Count > 0)
                {
                    foreach (var item in request.FieldData)
                    {
                        permissionTable.Rows.Add(item.FieldName, item.FieldValue, "");
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@TabId", (object)request.TabId ?? DBNull.Value),
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@DetailId", (object)request.Id ?? DBNull.Value),
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@FiancialYearId", request.FiancialYearId),
                        new SqlParameter("@UserId", request.UserId),
                        new SqlParameter("@PageTablename", request.PageTablename),
                        new SqlParameter("@PageType", request.PageType),
                        new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_FieldsData",
                            Value = permissionTable
                        }
                    };
                    pageFields = context.Database.SqlQuery<PageFieldData>("EXEC Proc_PageValidation @FormId,@TabId,@ParentId,@DetailId,@CompanyId,@FiancialYearId,@UserId,@PageTablename,@PageType,@FieldsData",
                        parameters.ToArray()).ToList();
                    if (pageFields != null && pageFields.Count > 0)
                    {
                        response.IsSuccess = false;
                        if (pageFields.Where(X => X.MessageTemplate == "Duplicate").FirstOrDefault() != null)
                        {
                            string fieldNames = string.Join(",", pageFields.Select(f => f.FieldName));
                            response.Response = "Duplicate Data present in " + fieldNames + " field(s)";
                        }
                        else
                        {
                            var tabs = pageFields.Select(f => new { f.TabTitle }).Distinct().ToList();
                            foreach (var item in tabs)
                            {
                                pageFields.Select(f => new { f.TabTitle }).Distinct().ToList();

                                string fieldNames = string.Join(", ", pageFields.Where(x => x.TabTitle == item.TabTitle).Select(f => f.FieldName));
                                response.Response = "Please fill in the " + fieldNames + " fields in the " + item.TabTitle + " tab.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Oops! Something went wrong while validating your data. Please try once more.";
                response.IsSuccess = true;
            }
            return response;
        }
        public ResponseModel PageDynamicDataValidation(SubmitFormModel request)
        {
            ResponseModel response = new ResponseModel();
            response.IsSuccess = true;
            string storedProcedureName = string.Empty;
            if (request.PageType == "Detail")
            {
                storedProcedureName = TFDSolution.Business.CommonBusiness.getFormKeySettingTab(Convert.ToString(request.FormId),
                    Convert.ToString(request.TabId), "DetailValidationScript");
            }
            else
            {
                storedProcedureName = TFDSolution.Business.CommonBusiness.getFormKeySetting(Convert.ToString(request.FormId), "HeaderValidationScript");
            }
            if (!string.IsNullOrEmpty(storedProcedureName))
            {
                try
                {
                    var permissionTable = new DataTable();
                    permissionTable.Columns.Add("FieldName", typeof(string));
                    permissionTable.Columns.Add("FieldValue", typeof(string));
                    permissionTable.Columns.Add("DataType", typeof(string));
                    if (request.FieldData != null && request.FieldData.Count > 0)
                    {
                        foreach (var item in request.FieldData)
                        {
                            permissionTable.Rows.Add(item.FieldName, item.FieldValue, "");
                        }
                        permissionTable.Rows.Add("UserId", request.UserId, "");
                    }
                    using (var context = new TFDSolutionEntities())
                    {
                        int parentId = request.ParentId;
                        int id = request.Id;
                        if (request.PageType != "Detail")
                        {
                            parentId = request.Id;
                            id = 0;
                        }
                        // Prepare parameters for the stored procedure
                        var parameters = new List<SqlParameter>{
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@ParentId", (object)parentId ?? DBNull.Value),
                        new SqlParameter("@DetailId", (object)id ?? DBNull.Value),
                        new SqlParameter("@FiancialYearId", request.FiancialYearId),
                        new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_FieldsData",
                            Value = permissionTable
                        }
                    };
                        response = context.Database.SqlQuery<ResponseModel>("EXEC " + storedProcedureName + " @FormId,@CompanyId,@ParentId,@DetailId,@FiancialYearId,@FieldsData",
                            parameters.ToArray()).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    CommonBusiness.LogEx(ex);
                    response.Action = "Error";
                    response.Response = "Oops! Something went wrong while validating your data. Please try once more.";
                    response.IsSuccess = true;
                }
            }
            return response;
        }

        public ResponseModel SubmitAccountLedger(DynamicSubmitPageModel request)
        {
            ResponseModel response = new ResponseModel();
            response.IsSuccess = true;
            string storedProcedureName = TFDSolution.Business.CommonBusiness.getFormKeySetting(Convert.ToString(request.FormId), "AllowFinanceEffect");
            if (!string.IsNullOrEmpty(storedProcedureName))
            {
                try
                {
                    using (var context = new TFDSolutionEntities())
                    {
                        // Prepare parameters for the stored procedure
                        var parameters = new List<SqlParameter>{
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@FiancialYearId", request.FiancialYearId),
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@CreatedBy", request.UserId)};
                        response = context.Database.SqlQuery<ResponseModel>("EXEC " + storedProcedureName + " @CompanyId,@FiancialYearId,@FormId,@ParentId,@CreatedBy",
                            parameters.ToArray()).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    CommonBusiness.LogEx(ex);
                    response.Action = "Error";
                    response.Response = "Oops! Something went wrong while validating your data. Please try once more.";
                    response.IsSuccess = true;
                }
            }
            return response;
        }
        public ResponseModel SavePageData(SubmitFormModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string fieldName = "";
                string fieldValue = "";
                string slqQuery = string.Empty, URNNo = string.Empty;
                var outputParam = new SqlParameter("@OutputParam", SqlDbType.Int) { Direction = ParameterDirection.Output };
                string responseMessage = "Record has been save successfully.";
                int itemSrNo = 0;
                if (model.FieldData != null && model.FieldData.Count > 0)
                {
                    response = PageDataValidation(model);
                    if (response != null && response.IsSuccess == false)
                    {
                        return response;
                    }
                    response = PageDynamicDataValidation(model);
                    if (response != null && response.IsSuccess == false)
                    {
                        return response;
                    }
                    int multipleCounter = 0;
                    List<SqlQueryModel> SQLQuries = new List<SqlQueryModel>(0);
                    response = new ResponseModel();
                    if (model.Id > 0)
                    {
                        slqQuery = "UPDATE [dbo].[" + model.PageTablename + "] SET ";
                        string updatePart = string.Empty;
                        updatePart = "UpdatedOn=GETDATE(),UpdatedBy='" + model.UserId.ToString() + "'";
                        foreach (var item in model.FieldData)
                        {
                            if (item.FieldName == "ExchangeRate")
                            {
                                item.FieldValue = Convert.ToString(item.FieldValue).Replace("₹", "");
                            }
                            else if (item.FieldName == "URNNo")
                            {
                                URNNo = item.FieldValue;
                            }
                            if (item.FieldName != "ItemSrNo")
                            {
                                if (updatePart.Length == 0)
                                {
                                    updatePart = item.FieldName + " = '" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                }
                                if (item.FieldType == "date")
                                {
                                    if (string.IsNullOrEmpty(item.FieldValue) || item.FieldValue == null)
                                    {
                                        updatePart += ", " + item.FieldName + " = NULL";
                                    }
                                    else
                                    {
                                        updatePart += ", " + item.FieldName + " = '" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                    }
                                }
                                else
                                {
                                    updatePart += ", " + item.FieldName + " = '" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                }
                            }
                        }
                        slqQuery = slqQuery + updatePart + " WHERE Id = " + model.Id;
                    }
                    else
                    {
                        List<PageFieldData> multiDoc = new List<PageFieldData>();
                        if (model.PageType == "Header" && model.AllowMultipleDocument)
                        {
                            var fieldNames = model.FieldData.FirstOrDefault(x => x.FieldName == model.MultipleDocFieldName);
                            var fieldValues = model.FieldData.FirstOrDefault(x => x.FieldName == model.MultipleDocFieldName + "_Id");
                            if (fieldNames != null && fieldValues != null && !string.IsNullOrEmpty(fieldNames.FieldValue) && !string.IsNullOrEmpty(fieldValues.FieldValue))
                            {
                                string[] fieldN = fieldNames.FieldValue.Split(',');
                                string[] fieldV = fieldValues.FieldValue.Split(',');
                                int length = Math.Min(fieldN.Length, fieldV.Length); // prevent mismatch crash
                                multipleCounter = length;
                                for (int i = 0; i < length; i++)
                                {
                                    multiDoc.Add(new PageFieldData()
                                    {
                                        FieldName = model.MultipleDocFieldName,
                                        FieldValue = fieldN[i]
                                    });

                                    multiDoc.Add(new PageFieldData()
                                    {
                                        FieldName = model.MultipleDocFieldName + "_Id",
                                        FieldValue = fieldV[i]
                                    });
                                }
                            }
                        }
                        if (multipleCounter > 1 && model.PageType == "Header")
                        {
                            for (int iRow = 0; iRow < multipleCounter; iRow++)
                            {
                                fieldName = "CompanyId,CreatedOn,CreatedBy,FiancialYearId";
                                fieldValue = "'" + model.CompanyId.ToString() + "',GETDATE(),'" + model.UserId.ToString() + "'," + model.FiancialYearId + "";
                                bool recordNoChanged = false;
                                foreach (var item in model.FieldData)
                                {
                                    if (item.FieldName == "ExchangeRate")
                                    {
                                        item.FieldValue = Convert.ToString(item.FieldValue).Replace("₹", "");
                                    }
                                    if (item.FieldName == "DocNo")
                                    {
                                        recordNoChanged = true;
                                        FormDocumentNo docNo = getDocNumber(Convert.ToString(model.FormId), model.FiancialYearId, Convert.ToString(model.CompanyId), model.DocId);
                                        item.FieldValue = docNo.DocNo;
                                    }
                                    else if (item.FieldName == "URNNo")
                                    {
                                        recordNoChanged = true;
                                        URNNo = CommonBusiness.getTransNextCode(Convert.ToString(model.FormId), Convert.ToString(model.CompanyId));
                                        item.FieldValue = URNNo;
                                    }
                                    if (item.FieldName == "ItemSrNo")
                                    {
                                        itemSrNo = Convert.ToInt32(item.FieldValue);
                                    }
                                    if (recordNoChanged)
                                    {
                                        responseMessage = "Record Number has been changed.";
                                    }
                                    if (fieldName.Length > 0)
                                    {
                                        int rowindex = 0;
                                        if (iRow > 0)
                                        {
                                            rowindex = iRow * 2;
                                        }
                                        if (multiDoc[rowindex].FieldName == item.FieldName)
                                        {
                                            fieldName = fieldName + "," + multiDoc[rowindex].FieldName;
                                            if (item.FieldType == "datetime-local")
                                            {
                                                fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(multiDoc[rowindex].FieldValue) + ":00'";
                                            }
                                            else
                                            {
                                                fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(multiDoc[rowindex].FieldValue) + "'";
                                            }
                                        }
                                        else if (multiDoc[rowindex + 1].FieldName == item.FieldName)
                                        {
                                            fieldName = fieldName + "," + multiDoc[rowindex + 1].FieldName;                                            
                                            if (item.FieldType == "datetime-local")
                                            {
                                                fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(multiDoc[rowindex + 1].FieldValue) + ":00'";
                                            }
                                            else
                                            {
                                                fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(multiDoc[rowindex + 1].FieldValue) + "'";
                                            }
                                        }
                                        else
                                        {

                                            fieldName = fieldName + "," + item.FieldName;
                                            if (item.FieldValue == null)
                                            {
                                                fieldValue = fieldValue + ",NULL";
                                            }
                                            else
                                            {
                                                fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                                if (item.FieldType == "datetime-local")
                                                {
                                                    fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(item.FieldValue) + ":00'";
                                                }
                                                else
                                                {
                                                    fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        fieldName = item.FieldName;
                                        if (item.FieldValue == null)
                                        {
                                            fieldValue = "NULL";
                                        }
                                        else
                                        {
                                            if (item.FieldType == "datetime-local")
                                            {
                                                fieldValue = "N'" + CodeHelper.ReplaceQuote(item.FieldValue) + ":00'";
                                            }
                                            else
                                            {
                                                fieldValue = "N'" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                            }
                                        }
                                    }
                                }
                                slqQuery = "INSERT INTO [dbo].[" + model.PageTablename + "] (" + fieldName + ") VALUES (" + fieldValue + ") ";
                                slqQuery = slqQuery + " SET @OutputParam = SCOPE_IDENTITY(); SELECT @OutputParam AS NewId;";
                                SQLQuries.Add(new SqlQueryModel() { URNNo = URNNo, Query = slqQuery });
                            }
                        }
                        else
                        {
                            fieldName = "CompanyId,CreatedOn,CreatedBy,FiancialYearId";
                            fieldValue = "'" + model.CompanyId.ToString() + "',GETDATE(),'" + model.UserId.ToString() + "'," + model.FiancialYearId + "";
                            if (model.PageType == "Detail")
                            {
                                fieldName = fieldName + ",ParentId,Status";
                                fieldValue = fieldValue + "," + model.ParentId + ",1";
                            }
                            bool recordNoChanged = false;
                            foreach (var item in model.FieldData)
                            {
                                if (item.FieldName == "ExchangeRate" && !string.IsNullOrEmpty(item.FieldValue))
                                {
                                    item.FieldValue = Convert.ToString(item.FieldValue).Replace("₹", "");
                                }
                                if (item.FieldName == "DocNo")
                                {
                                    recordNoChanged = true;
                                    FormDocumentNo docNo = getDocNumber(Convert.ToString(model.FormId), model.FiancialYearId, Convert.ToString(model.CompanyId.Value), model.DocId);
                                    item.FieldValue = docNo.DocNo;
                                }
                                if (item.FieldName == "ItemSrNo")
                                {
                                    itemSrNo = Convert.ToInt32(item.FieldValue);
                                }
                                else if (item.FieldName == "URNNo")
                                {
                                    recordNoChanged = true;
                                    URNNo = CommonBusiness.getTransNextCode(Convert.ToString(model.FormId), Convert.ToString(model.CompanyId));
                                    item.FieldValue = URNNo;
                                }
                                if (recordNoChanged)
                                {
                                    responseMessage = "Record Number has been changed.";
                                }
                                if (fieldName.Length > 0)
                                {
                                    fieldName = fieldName + "," + item.FieldName;
                                    if (item.FieldValue == null)
                                    {
                                        if (item.FieldType == "datetime-local")
                                            fieldValue = fieldValue + ":00" + ",NULL";
                                        else
                                            fieldValue = fieldValue + ",NULL";
                                    }
                                    else
                                    {
                                        if (item.FieldType == "datetime-local")
                                        {
                                            fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(item.FieldValue) + ":00'";
                                        }
                                        else
                                        {
                                            fieldValue = fieldValue + ",N'" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                        }
                                    }
                                }
                                else
                                {
                                    fieldName = item.FieldName;
                                    if (item.FieldValue == null)
                                    {
                                        fieldValue = "NULL";
                                    }
                                    else
                                    {
                                        if (item.FieldType == "datetime-local")
                                        {
                                            fieldValue = "N'" + CodeHelper.ReplaceQuote(item.FieldValue) + ":00'";
                                        }
                                        else
                                        {
                                            fieldValue = "N'" + CodeHelper.ReplaceQuote(item.FieldValue) + "'";
                                        }
                                    }
                                }
                            }
                            slqQuery = "INSERT INTO [dbo].[" + model.PageTablename + "] (" + fieldName + ") VALUES (" + fieldValue + ") ";
                            slqQuery = slqQuery + " SET @OutputParam = SCOPE_IDENTITY(); SELECT @OutputParam AS NewId;";
                            SQLQuries.Add(new SqlQueryModel() { URNNo = URNNo, Query = slqQuery });
                        }
                    }
                    using (var context = new TFDSolutionEntities())
                    {
                        if (model.FieldData.Where(x => x.FieldName == "ExchangeRate").FirstOrDefault() != null)
                        {
                            string fieldAdd = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + model.PageTablename + "' AND COLUMN_NAME = 'ExchangeRate') BEGIN  ALTER TABLE " + model.PageTablename + " ADD ExchangeRate Float NULL END";
                            context.Database.ExecuteSqlCommand(fieldAdd);
                        }
                        if (model.AllowMultipleDocument)
                        {
                            string fieldAdd = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + model.PageTablename + "' AND COLUMN_NAME = 'MultipleDocumentIds') BEGIN  ALTER TABLE " + model.PageTablename + " ADD MultipleDocumentIds NVARCHAR(200) NULL END";
                            context.Database.ExecuteSqlCommand(fieldAdd);
                        }
                        List<int> primaryIdList = new List<int>();
                        if (model.Id <= 0)
                        {
                            if (model.AllowMultipleDocument && SQLQuries.Count > 1)
                            {
                                int mainId = 0;
                                foreach (var qry in SQLQuries)
                                {
                                    int primaryId = context.Database.ExecuteSqlCommand(qry.Query, outputParam);
                                    int id = (int)outputParam.Value;
                                    response.Id = id;
                                    primaryIdList.Add(id);
                                    // After loop
                                    if (mainId == 0)
                                    {
                                        mainId = id;
                                        if (primaryId > 0 && model.Id <= 0 && id > 0 && model.PageType != "Detail")
                                        {
                                            FinalPageHeaderSave(model.FormId, id, Convert.ToString(model.UserId.Value), qry.URNNo);
                                        }
                                    }
                                    else
                                    {
                                        FormDocumentNo docNo = getDocNumber(Convert.ToString(model.FormId), model.FiancialYearId, Convert.ToString(model.CompanyId.Value), model.DocId);
                                        URNNo = CommonBusiness.getTransNextCode(Convert.ToString(model.FormId), Convert.ToString(model.CompanyId));
                                        SavePageWithMultipleDocument(model.PageTablename, model.FormId, mainId, id, Convert.ToString(model.UserId.Value), URNNo, docNo.DocNo);
                                    }
                                }
                            }
                            else
                            {
                                int primaryId = context.Database.ExecuteSqlCommand(slqQuery, outputParam);
                                int id = (int)outputParam.Value;
                                response.Id = id;
                                primaryIdList.Add(id);
                                if (primaryId > 0 && model.Id <= 0 && id > 0)
                                {
                                    if (model.PageType != "Detail")
                                    {
                                        FinalPageHeaderSave(model.FormId, id, Convert.ToString(model.UserId.Value), URNNo);
                                        //context.Database.ExecuteSqlCommand("UPDATE m_Attachments SET URNNo='" + URNNo + "' ,ParentId = " + id + " WHERE UploadedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                        //context.Database.ExecuteSqlCommand("UPDATE t_OtherChargesData SET URNNo='" + URNNo + "' ,ParentId = " + id + " WHERE CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                        //context.Database.ExecuteSqlCommand("UPDATE t_SpecificationsData SET URNNo='" + URNNo + "' ,ParentId = " + id + " WHERE CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                        //context.Database.ExecuteSqlCommand("UPDATE t_DeliveryScheduledsData SET URNNo='" + URNNo + "' ,ParentId = " + id + " WHERE CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                        //context.Database.ExecuteSqlCommand("UPDATE m_ItemStockSerial SET URNNo='" + URNNo + "' ,ParentId = " + id + " WHERE CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                        //context.Database.ExecuteSqlCommand("UPDATE m_BOMProcess SET ParentId = " + id + " WHERE CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "'  AND ParentId IN (0,-1)");
                                        //string sql = "SELECT m_FormTab.TabSQLTableName FROM m_FormTab WITH(NOLOCK) INNER JOIN m_FormMast WITH(NOLOCK) ON m_FormTab.FormId = m_FormMast.FormId INNER JOIN m_FormSection WITH(NOLOCK) ON m_FormSection.FormSectionId = m_FormTab.FormSectionId WHERE m_FormMast.FormId='" + model.FormId + "' AND m_FormTab.TabSQLTableName IS NOT NULL and m_FormMast.ParentFormId is not null AND m_FormSection.SectionName='Detail' AND m_FormMast.SQLTableName='" + model.PageTablename + "'";
                                        //List<string> tab = context.Database.SqlQuery<string>(sql).ToList();
                                        ////List<FormTab> tab = geSectionTabs(model.FormId, model.SectionId);
                                        //if (tab != null && tab.Count > 0)
                                        //{
                                        //    foreach (var item in tab)
                                        //    {
                                        //        if (!string.IsNullOrEmpty(item))
                                        //        {
                                        //            model.Id = id;
                                        //            if (model.Id > 0)
                                        //            {
                                        //                context.Database.ExecuteSqlCommand("Update " + item + " set URNNo = '" + URNNo + "', ParentId = " + model.Id + " where CreatedBy = '" + Convert.ToString(model.UserId) + "' and ParentId in (0,-1)");
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    else
                                    {

                                        var itemSrField = model.FieldData.Where(x => x.FieldName == "ItemSrNo").FirstOrDefault();
                                        if (itemSrField != null)
                                        {
                                            itemSrNo = Convert.ToInt32(itemSrField.FieldValue);
                                            context.Database.ExecuteSqlCommand(
                                                "EXEC m_FinalPageDetailSave @ParentId, @FormId, @ItemSrNo, @UserId, @DetailId, @URNNo",
                                                new SqlParameter("@ParentId", -1),
                                                new SqlParameter("@FormId", model.FormId),
                                                new SqlParameter("@ItemSrNo", itemSrNo),
                                                new SqlParameter("@UserId", model.UserId),
                                                new SqlParameter("@DetailId", id),
                                                new SqlParameter("@URNNo", URNNo)
                                            //,
                                            //new SqlParameter("@TabId", model.TabId),
                                            //new SqlParameter("@Action", model.PageAction)
                                            );
                                            //context.Database.ExecuteSqlCommand("UPDATE m_Attachments SET DetailId = " + id + " WHERE UploadedBy = '" + Convert.ToString(model.UserId) + "' and ItemSrNo =" + itemSrNo + " and FormId = '" + Convert.ToString(model.FormId) + "' AND DetailId IN (0)");
                                            //context.Database.ExecuteSqlCommand("UPDATE t_ReadingData SET URNNo='" + URNNo + "' ,DetailId = " + id + " WHERE ItemSrNo= " + itemSrNo + " and CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                            //context.Database.ExecuteSqlCommand("UPDATE t_BillAdjustment SET URNNo='" + URNNo + "' ,DetailId = " + id + " WHERE ItemSrNo= " + itemSrNo + " and CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                        }
                                    }
                                }
                            }

                            if (model.AllowMultipleDocument)
                            {
                                string primaryIds = string.Join(",", primaryIdList);
                                string UpdateSQL = "UPDATE " + model.PageTablename + " SET MultipleDocumentIds = '" + primaryIds + "' WHERE Id IN (" + primaryIds + ")";
                                context.Database.ExecuteSqlCommand(UpdateSQL);
                            }
                        }
                        else
                        {
                            response.Id = model.Id;
                            context.Database.ExecuteSqlCommand(slqQuery);
                            if (model.PageType != "Detail")
                            {
                                FinalPageHeaderSave(model.FormId, model.Id, Convert.ToString(model.UserId.Value), URNNo);
                                if (model.PageTablename == "m_MastProduct_Master")
                                {
                                    context.Database.ExecuteSqlCommand("EXEC Proc_UpdateItemNameGlobally @FormId = '" + Convert.ToString(model.FormId) + "',@UserId = '" + Convert.ToString(model.UserId) + "', @ParentId = " + model.Id + " ");
                                }
                                string sql = "SELECT m_FormTab.TabSQLTableName FROM m_FormTab WITH(NOLOCK) INNER JOIN m_FormMast WITH(NOLOCK) ON m_FormTab.FormId = m_FormMast.FormId INNER JOIN m_FormSection WITH(NOLOCK) ON m_FormSection.FormSectionId = m_FormTab.FormSectionId WHERE m_FormMast.FormId='" + model.FormId + "' AND m_FormTab.TabSQLTableName IS NOT NULL and m_FormMast.ParentFormId is not null AND m_FormSection.SectionName='Detail' AND m_FormMast.SQLTableName='" + model.PageTablename + "'";
                                List<string> tab = context.Database.SqlQuery<string>(sql).ToList();
                                //List<FormTab> tab = geSectionTabs(model.FormId, model.SectionId);
                                if (tab != null && tab.Count > 0)
                                {
                                    foreach (var item in tab)
                                    {
                                        if (!string.IsNullOrEmpty(item))
                                        {
                                            //context.Database.ExecuteSqlCommand("Update " + item + " set URNNo = '" + URNNo + "', ParentId = " + model.Id + " where ParentId in (0,-1," + model.Id + ")");
                                            context.Database.ExecuteSqlCommand("Update " + item + " set URNNo = '" + URNNo + "', ParentId = " + model.Id + " where CreatedBy = '" + Convert.ToString(model.UserId) + "' and ParentId in (0,-1)");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                context.Database.ExecuteSqlCommand(
                                                "EXEC m_FinalPageDetailSave @ParentId, @FormId, @ItemSrNo, @UserId, @DetailId, @URNNo",
                                                new SqlParameter("@ParentId", -1),
                                                new SqlParameter("@FormId", model.FormId),
                                                new SqlParameter("@ItemSrNo", itemSrNo),
                                                new SqlParameter("@UserId", model.UserId),
                                                new SqlParameter("@DetailId", model.Id),
                                                new SqlParameter("@URNNo", URNNo)
                                            //,new SqlParameter("@TabId", model.TabId),
                                            //new SqlParameter("@Action", model.PageAction)
                                            );
                                //context.Database.ExecuteSqlCommand("UPDATE t_ReadingData SET URNNo='" + URNNo + "' ,DetailId = " + model.Id + " WHERE  ItemSrNo= " + itemSrNo + " and CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                                //context.Database.ExecuteSqlCommand("UPDATE t_BillAdjustment SET URNNo='" + URNNo + "' ,DetailId = " + model.Id + " WHERE ItemSrNo= " + itemSrNo + " and CreatedBy = '" + Convert.ToString(model.UserId) + "' and FormId = '" + Convert.ToString(model.FormId) + "' AND ParentId IN (0,-1)");
                            }
                        }
                        if (model.PageType != "Detail")
                        {
                            CommonBusiness.ItemStock(new ItemStockModel()
                            {
                                CompanyId = (model.CompanyId.HasValue ? model.CompanyId.Value.ToString() : string.Empty),
                                FormId = model.FormId,
                                DetailId = 0,
                                Id = response.Id,
                                Status = 1,
                                Userid = (model.UserId.HasValue ? model.UserId.Value.ToString() : string.Empty)
                            });
                            SubmitAccountLedger(new DynamicSubmitPageModel()
                            {
                                CompanyId = (model.CompanyId.HasValue ? model.CompanyId.Value.ToString() : string.Empty),
                                FormId = model.FormId,
                                FiancialYearId = model.FiancialYearId,
                                ParentId = response.Id,
                                UserId = (model.UserId.HasValue ? model.UserId.Value.ToString() : string.Empty)
                            });
                        }
                        else if (model.PageType == "Detail")
                        {
                            CommonBusiness.ItemStock(new ItemStockModel()
                            {
                                CompanyId = (model.CompanyId.HasValue ? model.CompanyId.Value.ToString() : string.Empty),
                                FormId = model.FormId,
                                DetailId = response.Id,
                                Id = model.ParentId,
                                Status = 1,
                                Userid = (model.UserId.HasValue ? model.UserId.Value.ToString() : string.Empty),
                                TabId = model.TabId,
                                PageAction = model.PageAction
                            });
                        }
                        context.SaveChanges();
                        response.Action = "Save";
                        response.IsSuccess = true;
                        response.Response = responseMessage;
                        response.URNNo = URNNo;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Action = "Error";
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            return response;
        }

        public void SavePageWithMultipleDocument(string PageTablename, string FormId, int ParentId, int NewParentId, string UserId, string URNNo, string DocNo)
        {
            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", FormId);
                    var param2 = new SqlParameter("@ParentId", ParentId);
                    var param3 = new SqlParameter("@NewParentId", NewParentId);
                    var param4 = new SqlParameter("@UserId", UserId);
                    var param5 = new SqlParameter("@URNNo", URNNo);
                    var param6 = new SqlParameter("@DocNo", DocNo);
                    var param7 = new SqlParameter("@PageTablename", PageTablename);
                    context.Database.SqlQuery<string>("EXEC m_SavePageWithMultipleDocument @FormId,@ParentId,@NewParentId,@UserId,@URNNo,@DocNo,@PageTablename", param1, param2, param3, param4, param5, param6, param7).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        public void FinalPageHeaderSave(string FormId, int ParentId, string UserId, string URNNo)
        {
            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", FormId);
                    var param2 = new SqlParameter("@ParentId", ParentId);
                    var param3 = new SqlParameter("@UserId", UserId);
                    var param4 = new SqlParameter("@URNNo", URNNo);
                    context.Database.SqlQuery<string>("EXEC m_FinalPageHeaderSave @FormId,@ParentId,@UserId,@URNNo", param1, param2, param3, param4).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }

        public SubmitFormModel GetFormFieldDataRecord(string tblName, int id)
        {
            SubmitFormModel model = new SubmitFormModel
            {
                PageTablename = tblName,
                Id = id,
                FieldData = new List<PageFieldData>() // Initialize list to avoid null reference issues
            };

            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string Sql = "SELECT * FROM " + tblName + " Where Id=" + id;
                    var fieldData = context.Database.SqlQuery<FormFieldData>(Sql).ToList();
                    if (fieldData != null && fieldData.Count > 0)
                    {
                        foreach (var field in fieldData)
                        {
                            foreach (var prop in field.GetType().GetProperties())
                            {
                                var fieldName = prop.Name; // Property name
                                var fieldValue = prop.GetValue(field)?.ToString() ?? string.Empty; // Property value

                                model.FieldData.Add(new PageFieldData
                                {
                                    FieldName = fieldName,
                                    FieldValue = fieldValue
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return model;
            }
            return model;
        }
        public ResponseModel DeleteTabFieldDataRecord(int id, string TabId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@TABID", TabId);
                    var param2 = new SqlParameter("@PrimaryId", id);

                    // Use SqlQuery<string>() since we expect a list of table names (strings)
                    response = context.Database.SqlQuery<ResponseModel>("EXEC procDeleteTabFieldData @TABID, @PrimaryId", param1, param2).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response = new ResponseModel();
                response.IsSuccess = false;
                response.Action = "Delete";
                response.Response = "Record not deleted";
            }
            return response;
        }

        public ItemSrNoResult GETNextItemSrNo(int parentId, string TabId, string userId, int ProcessId = 0, int DetailParentId = 0)
        {
            ItemSrNoResult response = new ItemSrNoResult();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormTabId", TabId);
                    var param2 = new SqlParameter("@CreatedBy", userId);
                    var param3 = new SqlParameter("@ParentId", parentId);
                    var param4 = new SqlParameter("@ProcessId", ProcessId);
                    var param5 = new SqlParameter("@DetailParentId", DetailParentId);
                    //@DetailParentId
                    // Use SqlQuery<string>() since we expect a list of table names (strings)
                    response = context.Database.SqlQuery<ItemSrNoResult>("EXEC Proc_GetNextItemNumber @FormTabId, @CreatedBy, @ParentId, @ProcessId, @DetailParentId", param1, param2, param3, param4, param5).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response = new ItemSrNoResult();
            }
            return response;
        }
        public ResponseModel DeleteFormFieldDataRecord(int id, string tblName)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string Sql = "DELETE FROM " + tblName + " Where Id=" + id;
                using (var context = new TFDSolutionEntities())
                {
                    //context.Database.ExecuteSqlCommand(Sql);
                    m_FormMast formdat = context.m_FormMast.Where(x => x.SQLTableName == tblName).FirstOrDefault();
                    if (formdat != null)
                    {
                        List<m_FormTab> tabs = context.m_FormTab.Where(x => x.FormId == formdat.FormId && !string.IsNullOrEmpty(x.TabSQLTableName)).ToList();
                        if (tabs != null && tabs.Count > 0)
                        {
                            foreach (var item in tabs)
                            {
                                m_FormSection section = context.m_FormSection.Where(x => x.FormSectionId == item.FormSectionId).FirstOrDefault();
                                if (section.SectionName != "Header")
                                {
                                    context.Database.ExecuteSqlCommand("DELETE FROM " + item.TabSQLTableName + " Where ParentId=" + id);
                                }
                            }
                        }
                        //context.Database.ExecuteSqlCommand("DELETE FROM t_OtherChargesData WHERE ParentId=" + id + " AND FormId='" + Convert.ToString(formdat.FormId) + "'");
                        //context.Database.ExecuteSqlCommand("DELETE FROM m_ItemStock WHERE ParentId=" + id + " AND FormId='" + Convert.ToString(formdat.FormId) + "'");
                        //context.Database.ExecuteSqlCommand("DELETE FROM t_SpecificationsData WHERE ParentId=" + id + " AND FormId='" + Convert.ToString(formdat.FormId) + "'");

                        var parameter1 = new SqlParameter("@Id", id);
                        var parameter2 = new SqlParameter("@SQLTableName", tblName);
                        var parameter3 = new SqlParameter("@FormId", Convert.ToString(formdat.FormId));
                        context.Database.ExecuteSqlCommand("exec proc_DeleteTransaction @Id , @SQLTableName, @FormId", parameter1, parameter2, parameter3);
                    }
                    context.SaveChanges();
                    response.Action = "Delete";
                    response.IsSuccess = true;
                    response.Response = "Record Deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Error deleting record!";
                response.IsSuccess = false;
            }
            return response;
        }
        public List<FormSection> GetFormSections()
        {
            List<FormSection> sections = new List<FormSection>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var section = context.m_FormSection.ToList();
                    if (section != null && section.Count > 0)
                    {
                        foreach (var ft in section)
                        {
                            sections.Add(new FormSection()
                            {
                                FormSectionId = ft.FormSectionId.ToString(),
                                SectionName = ft.SectionName
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                sections = new List<FormSection>();
            }
            return sections; ;
        }
        public List<FormFieldType> GetFormFieldTypeList()
        {
            List<FormFieldType> formFieldsType = new List<FormFieldType>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var fieldTypes = context.m_FormFieldTypes
                        .Where(f => f.IsFieldTypeElement == true) // Fetch only elements
                        .OrderBy(f => f.SortOrder) // Optional: Sort by order
                        .ToList();

                    if (fieldTypes != null && fieldTypes.Count > 0)
                    {
                        foreach (var ft in fieldTypes)
                        {
                            formFieldsType.Add(new FormFieldType()
                            {
                                FieldTypeId = ft.FieldTypeId,
                                FieldType = ft.FieldType,
                                FieldTypeClass = ft.FieldTypeClass,
                                SortOrder = ft.SortOrder,
                                IsFieldTypeElement = ft.IsFieldTypeElement
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return formFieldsType;
            }
            return formFieldsType; ;
        }
        public List<string> GetTablesByPrefix(string sourceType)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    if (sourceType == "Table")
                    {
                        var param1 = new SqlParameter("@Prefix", "");
                        // Use SqlQuery<string>() since we expect a list of table names (strings)
                        var tables = context.Database.SqlQuery<string>("EXEC proc_GetTablesByPrefix @Prefix", param1).ToList();
                        return tables;
                    }
                    else if (sourceType == "StoredProcedure")
                    {
                        // Use SqlQuery<string>() since we expect a list of table names (strings)
                        var tables = context.Database.SqlQuery<string>("EXEC proc_GetAllStoredProcedures").ToList();
                        return tables;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return new List<string>();
        }

        public List<SqlTemplateDto> GetSqlTemplateList()
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var TemplateList = context.Database.SqlQuery<SqlTemplateDto>("EXEC procGetSqlTemplates").ToList();
                    return TemplateList;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return new List<SqlTemplateDto>();
        }
        public List<string> GetSqlTemplateDetail(int TemplateId)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@TemplateId", TemplateId);
                    var TemplateList = context.Database.SqlQuery<string>("EXEC procGetSqlTemplateDetail @TemplateId", param1).ToList();
                    return TemplateList;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return new List<string>();
        }

        public ProcedureSchema GetProcedureRequests(string spName, string pageTableName, string fieldId)
        {
            ProcedureSchema schema = new ProcedureSchema();
            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    var param2 = new SqlParameter("@SPName", spName);
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    var columns = context.Database.SqlQuery<string>("EXEC proc_GetSPResultSchema @SPName", param2).ToList();
                    schema.Columns = columns;

                    var param3 = new SqlParameter("@SPName", spName);
                    var param4 = new SqlParameter("@FieldId", fieldId);
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    schema.Request = context.Database.SqlQuery<ProcedureRequest>("EXEC proc_GetSPRequestSchema @SPName, @FieldId", param3, param4).ToList();

                    var param5 = new SqlParameter("@TableName", pageTableName);
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    var SPcolumns = context.Database.SqlQuery<string>("EXEC proc_GetColumnsByTable @TableName", param5).ToList();
                    schema.SPColumns = SPcolumns;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return schema;
        }

        public ProcedureSchema GetSourceColumns(string sourceType, string tableName, string fieldId, string pageTableName)
        {
            ProcedureSchema schema = new ProcedureSchema();
            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    if (sourceType == "Table")
                    {
                        var param1 = new SqlParameter("@TableName", tableName);
                        // Use SqlQuery<string>() since we expect a list of column names (strings)
                        var columns = context.Database.SqlQuery<string>("EXEC proc_GetColumnsByTable @TableName", param1).ToList();
                        schema.Columns = columns;
                    }
                    else if (sourceType == "StoredProcedure")
                    {
                        var param2 = new SqlParameter("@SPName", tableName);
                        // Use SqlQuery<string>() since we expect a list of column names (strings)
                        var columns = context.Database.SqlQuery<string>("EXEC proc_GetSPResultSchema @SPName", param2).ToList();
                        schema.Columns = columns;

                        var param3 = new SqlParameter("@SPName", tableName);
                        var param4 = new SqlParameter("@FieldId", fieldId);
                        // Use SqlQuery<string>() since we expect a list of column names (strings)
                        var request = context.Database.SqlQuery<ProcedureRequest>("EXEC proc_GetSPRequestSchema @SPName, @FieldId", param3, param4).ToList();
                        schema.Request = request;

                        var param5 = new SqlParameter("@TableName", pageTableName);
                        // Use SqlQuery<string>() since we expect a list of column names (strings)
                        var SPcolumns = context.Database.SqlQuery<string>("EXEC proc_GetColumnsByTable @TableName", param5).ToList();
                        schema.SPColumns = SPcolumns;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return schema;
        }

        public ResponseModel SaveField(FormField mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    Guid _FormId = CommonBusiness.ConvertGUID(mast.FormId);
                    Guid _FormSectionId = CommonBusiness.ConvertGUID(mast.FormSectionId);
                    Guid _FormTabId = CommonBusiness.ConvertGUID(mast.FormTabId);
                    Guid _FieldTypeId = CommonBusiness.ConvertGUID(mast.FieldTypeId);
                    bool isNew = false;
                    m_FormField m_field = new m_FormField();
                    if (string.IsNullOrEmpty(mast.FieldId))
                    {
                        m_FormField exist = context.m_FormField
                            .Where(x => x.FieldName == mast.FieldName && x.FormTabId == _FormTabId
                            && x.FormId == _FormId && x.FormSectionId == _FormSectionId)
                            .FirstOrDefault();
                        if (exist != null)
                        {
                            response.Action = "Validation";
                            response.Response = "The '" + mast.FieldName + "' field is already exists.";
                            response.IsSuccess = false;
                            return response;
                        }
                        m_field.FieldId = Guid.NewGuid();
                        isNew = true;
                    }
                    else
                    {
                        Guid _FieldId = CommonBusiness.ConvertGUID(mast.FieldId);
                        m_field = context.m_FormField.Where(x => x.FieldId == _FieldId).FirstOrDefault();
                    }
                    if (mast.AllowMultiDocument.HasValue ? mast.AllowMultiDocument.Value : false)
                    {
                        m_FormField existMultiDoc = context.m_FormField.Where(x => x.AllowMultiDocument == true && x.FieldId != m_field.FieldId).FirstOrDefault();
                        if (existMultiDoc != null)
                        {
                            response.Action = "Validation";
                            response.Response = "The '" + existMultiDoc.FieldName + "' field already has allow multiple document.";
                            response.IsSuccess = false;
                            return response;
                        }
                    }
                    if (m_field != null)
                    {
                        m_field.FieldCaption = mast.FieldCaption;
                        m_field.IsRequired = mast.IsRequired;
                        m_field.IsActive = mast.IsActive;
                        m_field.IsReadOnly = mast.IsReadOnly;
                        m_field.IsSummary = mast.IsSummary;
                        m_field.IsVisible = mast.IsVisible;
                        m_field.IsUnique = mast.IsUnique;
                        m_field.IsVisibleInList = mast.IsVisibleInList;
                        m_field.IsDependencyField = mast.IsDependencyField;
                        m_field.IsMultiSelection = mast.IsMultiSelection;
                        m_field.AllowMultiDocument = mast.AllowMultiDocument;
                        m_field.FieldSize = mast.FieldSize;
                        m_field.FieldDecimal = mast.FieldDecimal;
                        m_field.FieldPlaceHolder = mast.FieldPlaceHolder;
                        m_field.FieldHelpText = mast.FieldHelpText;
                        m_field.IsDisable = mast.IsDisable;
                        m_field.DDLSourceType = mast.DDLSourceType;
                        m_field.DDLTextField = mast.DDLTextField;
                        m_field.DDLValueField = mast.DDLValueField;
                        m_field.DDLSourceName = mast.DDLSourceName;
                        m_field.FieldFormula = mast.FieldFormula;
                        m_field.FieldLength = mast.FieldLength;
                        m_field.TemplateId = mast.TemplateId;
                        m_field.IsTimeField = mast.IsTimeField;
                        m_field.FieldFormat = mast.FieldFormat;

                        if (mast.SortOrder != null && mast.SortOrder > 0)
                        {
                            m_field.SortOrder = mast.SortOrder;
                        }
                        else
                        {
                            int maxSortOrder = context.m_FormField
                              .Where(f => f.FormId == _FormId && f.FormSectionId == _FormSectionId && f.FormTabId == _FormTabId)
                              .Select(f => (int?)f.SortOrder)
                              .Max() ?? 0; // If no records, default to 0
                            m_field.SortOrder = maxSortOrder + 1;
                            // **Sort Order Calculation**
                        }
                        var sectionData = context.m_FormSection.Where(x => x.FormSectionId == _FormSectionId).FirstOrDefault();
                        if (sectionData != null && Convert.ToString(sectionData.SectionName).ToLower() == "detail")
                        {
                            var tabData = context.m_FormTab.Where(x => x.FormTabId == _FormTabId).FirstOrDefault();
                            if (tabData != null)
                            {
                                if (!string.IsNullOrEmpty(tabData.TabSQLTableName))
                                {
                                    mast.SQLTableName = tabData.TabSQLTableName;
                                }
                                else
                                {
                                    var formData = context.m_FormMast.Where(x => x.FormId == _FormId).FirstOrDefault();
                                    mast.SQLTableName = formData.SQLTableName + "_" + tabData.TabName;
                                }
                            }
                        }
                        if (isNew)
                        {
                            m_field.FieldTypeId = _FieldTypeId;
                            m_field.FormId = _FormId;
                            m_field.FormSectionId = _FormSectionId;
                            m_field.FormTabId = _FormTabId;
                            m_field.FieldName = mast.FieldName;
                            m_field.FieldCaption = mast.FieldCaption;
                            m_field.CreatedOn = DateTime.Now;
                            context.m_FormField.Add(m_field);
                        }
                        else
                        {
                            m_field.UpdatedOn = DateTime.Now;
                        }
                        var sqlTable = new SqlParameter("@SQLTableName", mast.SQLTableName);
                        var fieldName = new SqlParameter("@FieldName", mast.FieldName);
                        var fieldType = new SqlParameter("@FieldType", mast.FieldType);
                        var fieldLength = new SqlParameter("@FieldLength", mast.FieldLength.ToString());
                        var param4 = new SqlParameter("@IsMultiSelection", mast.IsMultiSelection);
                        context.Database.ExecuteSqlCommand("exec proc_AddField @SQLTableName , @FieldName , @FieldType, @FieldLength, @IsMultiSelection", sqlTable, fieldName, fieldType, fieldLength, param4);
                        #region Field Parameters
                        if (mast.Parameters != null && mast.Parameters.Count > 0)
                        {
                            DataTable itemTable = new DataTable();
                            itemTable.Columns.Add("FieldId", typeof(Guid));
                            itemTable.Columns.Add("ParamName", typeof(string));
                            itemTable.Columns.Add("DefaultValue", typeof(string));
                            itemTable.Columns.Add("ParamFieldName", typeof(string));
                            itemTable.Columns.Add("ParameterFromTable", typeof(string));
                            foreach (var item in mast.Parameters)
                            {
                                itemTable.Rows.Add(m_field.FieldId, item.ParameterName, item.ParamDefaultValue, item.ParameterValueFieldName, item.ParameterFromTable);
                            }
                            using (var command = context.Database.Connection.CreateCommand())
                            {
                                if (context.Database.Connection.State == ConnectionState.Closed)
                                    context.Database.Connection.Open();

                                command.CommandText = "dbo.proc_SetFieldParamMapping";
                                command.CommandType = CommandType.StoredProcedure;

                                var param = new SqlParameter();
                                param.ParameterName = "@ParamList";
                                param.SqlDbType = SqlDbType.Structured;
                                param.SourceColumn = "dbo.FieldProcInParamMappingType";
                                param.Value = itemTable;
                                command.Parameters.Add(param);

                                command.ExecuteNonQuery();
                                response.IsSuccess = true;
                            }
                        }
                        else
                        {
                            context.Database.ExecuteSqlCommand("DELETE FROM m_FieldProcInParamMapping WHERE FieldId = '" + Convert.ToString(m_field.FieldId) + "'");
                        }
                        #endregion

                        context.SaveChanges();
                        response.Action = "Save";
                        response.PrimaryId = m_field.FieldId;
                        response.IsSuccess = true;
                        response.Response = "Record save successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.Response = "Record not save successfully.";
                response.IsSuccess = false;
            }
            return response;
        }

        public FormMast_Data GetFormStructure(string formName, string uId, string CreatedBy, int RoleId)
        {
            FormMast_Data formStructure = new FormMast_Data();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    List<SqlParameter> parameters1 = new List<SqlParameter> { new SqlParameter("@FormName", formName) };
                    DataSet ds = DbHelper.ExecuteStoredProcedure(context, "proc_GetFormStructure", parameters1);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        formStructure = FormStructureMapper.MapToFormStructure(ds);
                    }
                    formStructure.FullAccess = false;
                    formStructure.CanAdd = false;
                    formStructure.CanEdit = false;
                    formStructure.CanDelete = false;
                    formStructure.CanPrint = false;
                    formStructure.CanApprove = false;
                    formStructure.NoAccess = true;
                    if (RoleId > 0)
                    {
                        //Bind Permission to Model
                        var permission = GetFormPermissions(Convert.ToString(formStructure.FormId), RoleId);
                        if (permission != null && permission?.RolePermissionID != null)
                        {
                            formStructure.FullAccess = permission.FullAccess;
                            formStructure.CanAdd = permission.CanAdd;
                            formStructure.CanEdit = permission.CanEdit;
                            formStructure.CanDelete = permission.CanDelete;
                            formStructure.CanPrint = permission.CanPrint;
                            formStructure.CanApprove = permission.CanApprove;
                            formStructure.NoAccess = permission.NoAccess;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return formStructure;
        }

        public Permissions GetFormPermissions(string formId, int roleId)
        {
            Permissions permission = new Permissions();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var paramRoleId = new SqlParameter("@RoleId", roleId);
                    var paramFormId = new SqlParameter("@FormId", formId);

                    // Fetch permissions from stored procedure
                    permission = context.Database.SqlQuery<Permissions>(
                       "EXEC proc_GetFormPermissions @RoleId, @FormId", paramRoleId, paramFormId
                   ).FirstOrDefault();


                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return permission;
        }

        public FormDocumentNo getDocNumber(string formId, int FinancialYear, string CompanyId, int DocNoId = 0, int PendingId = 0, string PendingItemSrNos = "")
        {
            FormDocumentNo response = new FormDocumentNo();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FinancialYearId", FinancialYear);
                    var param2 = new SqlParameter("@FormId", formId);
                    var param3 = new SqlParameter("@DocNoId", DocNoId);
                    var param4 = new SqlParameter("@PendingId", PendingId);
                    var param5 = new SqlParameter("@PendingItemSrNos", PendingItemSrNos);
                    var param6 = new SqlParameter("@CompanyId", CompanyId);
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    response = context.Database.SqlQuery<FormDocumentNo>("EXEC proc_getDocNumber @FinancialYearId, @FormId, @DocNoId, @PendingId, @PendingItemSrNos, @CompanyId",
                        param1, param2, param3, param4, param5, param6).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public async Task<FormMast_Data> FillFieldValue(int uid, FormMast_Data Formdata, string UserId)
        {
            try
            {
                if (uid > 0)
                {
                    if (Formdata != null && Formdata.Sections != null && Formdata.Sections.Count > 0)
                    {
                        string pageName = Formdata.FormName;
                        List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
                        string storedProcedureName = "proc_GetTableData";
                        using (var context = new TFDSolutionEntities())
                        {
                            using (var command = context.Database.Connection.CreateCommand())
                            {
                                if (context.Database.Connection.State == ConnectionState.Closed)
                                    context.Database.Connection.Open();

                                command.Parameters.Add(new SqlParameter { ParameterName = "@PageName", Value = pageName });
                                command.Parameters.Add(new SqlParameter { ParameterName = "@RecordId", DbType = DbType.Int32, Value = uid });
                                command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = UserId });
                                //command.Transaction = context.Database.Connection.BeginTransaction();
                                command.CommandText = storedProcedureName;
                                command.CommandType = CommandType.StoredProcedure;
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (await reader.ReadAsync())
                                        {
                                            Dictionary<string, object> obj = new Dictionary<string, object>();
                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                string columnName = Convert.ToString(reader.GetName(i)).ToUpper();
                                                object columnValue = reader.GetValue(i);
                                                obj[columnName] = columnValue;
                                            }
                                            items.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        if (items != null && items.Count > 0)
                        {
                            foreach (var item in Formdata.Sections)
                            {
                                if (item.SectionName == "Header" && item.Tabs.Count > 0)
                                {
                                    foreach (var tb in item.Tabs)
                                    {
                                        if (tb.Fields.Count > 0)
                                        {
                                            foreach (var field in tb.Fields)
                                            {
                                                foreach (var _it in items)
                                                {
                                                    object value;
                                                    string _fieldName = Convert.ToString(field.FieldName).ToUpper();
                                                    if (_it.TryGetValue(_fieldName, out value))
                                                    {
                                                        if (field.FieldType == "CheckBox")
                                                        {
                                                            field.FieldValue = Convert.ToString(value);
                                                        }
                                                        else if (field.FieldType == "DateTimeField" &&
                                                            (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                                        {
                                                            try
                                                            {
                                                                //DateTime date = DateTime.ParseExact(Convert.ToString(value), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                                                //// Convert it to dd/MM/yyyy format
                                                                //string formattedDate = date.ToString("dd/MM/yyyy");

                                                                //string fmt = "dd-MM-yyyy HH:mm:ss";
                                                                //DateTime date1 = CommonBusiness.ParseWithLocalCulture(Convert.ToString(value));
                                                                //string formattedDate = date1.ToString("dd/MM/yyyy");
                                                                field.FieldValue = Convert.ToString(value);
                                                            }
                                                            catch (Exception)
                                                            {
                                                            }
                                                        }
                                                        else if (field.FieldType == "TimeField" &&
                                                            (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                                        {
                                                            DateTime date = Convert.ToDateTime(Convert.ToString(value));
                                                            field.FieldValue = date.ToString("HH:mm");
                                                        }
                                                        else
                                                        {
                                                            field.FieldValue = Convert.ToString(value);
                                                        }
                                                    }
                                                    ///field.FieldValue = _it[field.FieldName];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Formdata;
        }

        public List<List<Dictionary<string, object>>> GetPendingMaster(int fromPendingId, string ItemSrNo)
        {
            //List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<List<Dictionary<string, object>>> items = new List<List<Dictionary<string, object>>>();
            try
            {
                if (fromPendingId > 0)
                {
                    string storedProcedureName = "m_GetPendingMaster_Data";
                    using (var context = new TFDSolutionEntities())
                    {
                        using (var command = context.Database.Connection.CreateCommand())
                        {
                            if (context.Database.Connection.State == ConnectionState.Closed)
                                context.Database.Connection.Open();

                            command.Parameters.Add(new SqlParameter { ParameterName = "@FormPendingId", Value = fromPendingId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ITEMSRNO", Value = ItemSrNo });
                            //command.Transaction = context.Database.Connection.BeginTransaction();
                            command.CommandText = storedProcedureName;
                            command.CommandType = CommandType.StoredProcedure;

                            using (var reader = command.ExecuteReader())
                            {
                                do
                                {
                                    var tableData = new List<Dictionary<string, object>>();

                                    while (reader.Read())
                                    {
                                        var row = new Dictionary<string, object>();

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            row[reader.GetName(i).ToUpper()] = reader.IsDBNull(i)
                                                ? null
                                                : reader.GetValue(i);
                                        }

                                        tableData.Add(row);
                                    }
                                    items.Add(tableData);

                                } while (reader.NextResult());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return items;
        }

        public async Task<FormMast_Data> FillPendingMaster(int fromPendingId, string ItemSrNo, FormMast_Data Formdata)
        {
            try
            {
                if (fromPendingId > 0)
                {
                    if (Formdata != null && Formdata.Sections != null && Formdata.Sections.Count > 0)
                    {
                        string pageName = Formdata.FormName;
                        List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
                        string storedProcedureName = "m_GetPendingMaster";
                        using (var context = new TFDSolutionEntities())
                        {
                            using (var command = context.Database.Connection.CreateCommand())
                            {
                                if (context.Database.Connection.State == ConnectionState.Closed)
                                    context.Database.Connection.Open();

                                command.Parameters.Add(new SqlParameter { ParameterName = "@FormPendingId", Value = fromPendingId });
                                command.Parameters.Add(new SqlParameter { ParameterName = "@ITEMSRNO", Value = ItemSrNo });
                                //command.Transaction = context.Database.Connection.BeginTransaction();
                                command.CommandText = storedProcedureName;
                                command.CommandType = CommandType.StoredProcedure;
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (await reader.ReadAsync())
                                        {
                                            Dictionary<string, object> obj = new Dictionary<string, object>();
                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                string columnName = Convert.ToString(reader.GetName(i)).ToUpper();
                                                object columnValue = reader.GetValue(i);
                                                obj[columnName] = columnValue;
                                            }
                                            items.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        if (items != null && items.Count > 0)
                        {
                            Formdata.Data = items;
                            foreach (var item in Formdata.Sections)
                            {
                                if (item.SectionName == "Header" && item.Tabs.Count > 0)
                                {
                                    foreach (var tb in item.Tabs)
                                    {
                                        if (tb.Fields.Count > 0)
                                        {
                                            foreach (var field in tb.Fields)
                                            {
                                                foreach (var _it in items)
                                                {
                                                    object value;
                                                    string _fieldName = Convert.ToString(field.FieldName).ToUpper();
                                                    if (_it.TryGetValue(_fieldName, out value))
                                                    {
                                                        if (field.FieldType == "CheckBox")
                                                        {
                                                            field.FieldValue = Convert.ToString(value);
                                                        }
                                                        else if (field.FieldType == "DateTimeField" &&
                                                            (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                                        {
                                                            try
                                                            {
                                                                field.FieldValue = Convert.ToString(value);
                                                            }
                                                            catch (Exception)
                                                            {
                                                                field.FieldValue = Convert.ToString(value);
                                                            }
                                                        }
                                                        else if (field.FieldType == "TimeField" &&
                                                            (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                                        {
                                                            DateTime date = Convert.ToDateTime(Convert.ToString(value));
                                                            field.FieldValue = date.ToString("HH:mm");
                                                        }
                                                        else
                                                        {
                                                            field.FieldValue = Convert.ToString(value);
                                                        }
                                                    }
                                                    ///field.FieldValue = _it[field.FieldName];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Formdata;
        }

        public async Task<PageTabModel> FillTabFieldValue(int uid, PageTabModel Tabdata)
        {
            try
            {
                if (Tabdata != null && Tabdata.Design != null && Tabdata.FormFields != null && Tabdata.FormFields.Count > 0)
                {
                    string tabId = Tabdata.Design.FormTabId;
                    List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
                    string storedProcedureName = "proc_GetTabData";
                    using (var context = new TFDSolutionEntities())
                    {
                        using (var command = context.Database.Connection.CreateCommand())
                        {
                            if (context.Database.Connection.State == ConnectionState.Closed)
                                context.Database.Connection.Open();

                            command.Parameters.Add(new SqlParameter { ParameterName = "@TabId", Value = tabId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@RecordId", DbType = DbType.Int32, Value = uid });
                            //command.Transaction = context.Database.Connection.BeginTransaction();
                            command.CommandText = storedProcedureName;
                            command.CommandType = CommandType.StoredProcedure;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        Dictionary<string, object> obj = new Dictionary<string, object>();
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            string columnName = Convert.ToString(reader.GetName(i)).ToUpper();
                                            object columnValue = reader.GetValue(i);
                                            obj[columnName] = columnValue;
                                        }
                                        items.Add(obj);
                                    }
                                }
                            }
                            if (items != null && items.Count > 0)
                            {
                                Tabdata.Data = items;
                                foreach (var field in Tabdata.FormFields)
                                {
                                    foreach (var _it in items)
                                    {
                                        object value;
                                        string _fieldName = Convert.ToString(field.FieldName).ToUpper();
                                        if (_it.TryGetValue(_fieldName, out value))
                                        {
                                            if (field.FieldType == "CheckBox")
                                            {
                                                field.FieldValue = Convert.ToString(value);
                                            }
                                            else if (field.FieldType == "DateTimeField" &&
                                                (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                            {
                                                field.FieldValue = Convert.ToString(value);
                                            }
                                            else if (field.FieldType == "TimeField" &&
                                                (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                            {
                                                DateTime date = Convert.ToDateTime(Convert.ToString(value));
                                                field.FieldValue = date.ToString("HH:mm");
                                            }
                                            //else if (field.FieldType == "Selection"
                                            //    && (value != null && !string.IsNullOrEmpty(Convert.ToString(value))))
                                            //{
                                            //    object _value;
                                            //    if (_it.TryGetValue(_fieldName + "_ID", out _value))
                                            //    {
                                            //        value = _value;
                                            //    }
                                            //    field.FieldValue = Convert.ToString(value);
                                            //}
                                            else
                                            {

                                                field.FieldValue = Convert.ToString(value);
                                            }
                                        }
                                        ///field.FieldValue = _it[field.FieldName];
                                    }
                                }
                                foreach (var _it in items)
                                {
                                    object value;
                                    if (_it.TryGetValue("CHARGESID", out value))
                                    {
                                        Tabdata.FormFields.Add(new FormField
                                        {
                                            FieldName = "ChargesId",
                                            FieldType = "Selection",
                                            FieldCaption = "Other Charges Template",
                                            FieldValue = Convert.ToString(value),
                                            FieldId = Guid.NewGuid().ToString()
                                        });
                                    }
                                    if (_it.TryGetValue("ITEMSRNO", out value))
                                    {
                                        Tabdata.FormFields.Add(new FormField
                                        {
                                            FieldName = "ItemSrNo",
                                            FieldType = "Label",
                                            FieldCaption = "Item Sr#",
                                            FieldValue = Convert.ToString(value),
                                            FieldId = Guid.NewGuid().ToString()
                                        });
                                    }
                                    if (_it.TryGetValue("ROWINDEX", out value))
                                    {
                                        Tabdata.FormFields.Add(new FormField
                                        {
                                            FieldName = "RowIndex",
                                            FieldType = "Label",
                                            FieldCaption = "Sr#",
                                            FieldValue = Convert.ToString(value),
                                            FieldId = Guid.NewGuid().ToString()
                                        });
                                    }
                                    if (_it.TryGetValue("FROM_URNNO", out value))
                                    {
                                        Tabdata.From_URNNO = Convert.ToString(value);
                                    }
                                    if (_it.TryGetValue("FROM_SOURCE", out value))
                                    {
                                        Tabdata.From_Source = Convert.ToString(value);
                                    }
                                    if (_it.TryGetValue("FROM_ITEM_SRNO", out value))
                                    {
                                        Tabdata.From_Item_SRNO = int.TryParse(Convert.ToString(value), out int num) ? num : 0;
                                    }
                                    if (_it.TryGetValue("PREVID", out value))
                                    {
                                        Tabdata.PrevId = int.TryParse(Convert.ToString(value), out int num) ? num : 0;
                                    }
                                    if (_it.TryGetValue("NEXTID", out value))
                                    {
                                        Tabdata.NextId = int.TryParse(Convert.ToString(value), out int num) ? num : 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Tabdata;
        }

        #endregion Form Master

        #region Page Operation
        public List<object> getPageGridData(string pageName, string companyId, int FinacialYearId)
        {
            List<object> _result = new List<object>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameter1 = new SqlParameter { ParameterName = "@PageName", Value = pageName };
                    var parameter2 = new SqlParameter { ParameterName = "@CompanyId", Value = companyId };
                    var parameter3 = new SqlParameter { ParameterName = "@FinancialYearId", Value = FinacialYearId };
                    var errorMsg = new SqlParameter("@ErrorMsg", SqlDbType.VarChar, 200)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    _result = context.Database.SqlQuery<object>("exec proc_GetDataTableList @PageName,@CompanyId,@FinancialYearId, @ErrorMsg out",
                        parameter1, parameter2, parameter3, errorMsg).ToList();

                    string errorMessage = (string)errorMsg.Value;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return _result;
        }
        public IEnumerable<IDictionary<string, object>> GetDynamicData(string pageName, int recordId, string UserId) //Task<IEnumerable<IDictionary<string, object>>>
        {
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            try
            {
                string storedProcedureName = "proc_GetTableData";
                using (var context = new TFDSolutionEntities())
                {
                    //dynamic re = GetTaskExecution(context, pageName);
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageName", Value = pageName });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@RecordId", Value = recordId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = UserId });
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return items;
        }
        public IEnumerable<IDictionary<string, object>> GetDynamicPendingData(int fromPendingId, string ItemSrNo) //Task<IEnumerable<IDictionary<string, object>>>
        {
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            try
            {
                string storedProcedureName = "m_GetPendingMaster";
                using (var context = new TFDSolutionEntities())
                {
                    //dynamic re = GetTaskExecution(context, pageName);
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@FormPendingId", Value = fromPendingId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ITEMSRNO", Value = ItemSrNo });
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return items;
        }
        public async Task<FormData> GetDynamicDataFromSPAsync(string pageName, string companyId, int FinacialYearId) //Task<IEnumerable<IDictionary<string, object>>>
        {
            FormData response = new FormData();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            List<PageGridData> pageGrids = new List<PageGridData>();
            try
            {
                string storedProcedureName = "proc_GetDataTableList";
                using (var context = new TFDSolutionEntities())
                {
                    //dynamic re = GetTaskExecution(context, pageName);
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageName", Value = pageName });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = companyId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FinancialYearId", Value = FinacialYearId });
                        //command.Parameters.Add(new SqlParameter("@ErrorMsg", pageName));
                        SqlParameter pvNewId = new SqlParameter();
                        pvNewId.ParameterName = "@ErrorMsg";
                        pvNewId.DbType = DbType.String;
                        pvNewId.Size = 200;
                        pvNewId.Direction = ParameterDirection.Output;
                        command.Parameters.Add(pvNewId);
                        //command.Transaction = context.Database.Connection.BeginTransaction();
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        DataTable dtSchema = new DataTable("Schema");
                        using (var reader = command.ExecuteReader())
                        {
                            dtSchema = reader.GetSchemaTable();
                            foreach (DataRow schemarow in dtSchema.Rows)
                            {
                                itemCol.Add(schemarow.ItemArray[0].ToString());
                            }
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        if (columnValue.GetType() == typeof(DateTime))
                                        {
                                            DateTime dt = Convert.ToDateTime(columnValue);
                                            bool hasOnlyTime = dt.TimeOfDay != TimeSpan.Zero;
                                            if (hasOnlyTime)
                                            {
                                                TimeSpan timeOnly = dt.TimeOfDay;
                                                columnValue = dt.ToString("HH:mm");
                                            }
                                            else
                                            {
                                                columnValue = (dt == new DateTime(1900, 1, 1)) ? "" : dt.ToString("dd/MM/yyyy");
                                            }
                                        }
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    string columnName = reader["FieldName"] != DBNull.Value ? (string)reader["FieldName"] : string.Empty;
                                    string columnTitle = reader["FieldTitle"] != DBNull.Value ? (string)(reader["FieldTitle"]) : string.Empty;
                                    pageGrids.Add(new PageGridData()
                                    {
                                        ColumnName = columnName,
                                        ColumnTitle = columnTitle,
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occured: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            response.Columns = itemCol;
            response.PageData = pageGrids;
            return response;
        }

        public async Task<FormData> GetPageColumns(string pageName)
        {
            FormData response = new FormData();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            List<PageGridData> pageGrids = new List<PageGridData>();
            try
            {
                string storedProcedureName = "proc_GetDataTableList_PagingColumns";
                using (var context = new TFDSolutionEntities())
                {
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageName", Value = pageName });
                        SqlParameter pvNewId = new SqlParameter();
                        pvNewId.ParameterName = "@ErrorMsg";
                        pvNewId.DbType = DbType.String;
                        pvNewId.Size = 200;
                        pvNewId.Direction = ParameterDirection.Output;
                        command.Parameters.Add(pvNewId);
                        //command.Transaction = context.Database.Connection.BeginTransaction();
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        DataTable dtSchema = new DataTable("Schema");
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName = reader["FieldName"] != DBNull.Value ? (string)reader["FieldName"] : string.Empty;
                                string columnTitle = reader["FieldTitle"] != DBNull.Value ? (string)(reader["FieldTitle"]) : string.Empty;
                                pageGrids.Add(new PageGridData()
                                {
                                    ColumnName = columnName,
                                    ColumnTitle = columnTitle,
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occured: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.PageData = pageGrids;
            return response;
        }

        public async Task<FormData> GetPageDataPaginationAsync(string pageName, string companyId, int FinacialYearId, int start, int length, string searchValue, string userId) //Task<IEnumerable<IDictionary<string, object>>>
        {
            FormData response = new FormData();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            List<PageGridData> pageGrids = new List<PageGridData>();
            try
            {
                string storedProcedureName = "proc_GetDataTableList_Paging";
                using (var context = new TFDSolutionEntities())
                {
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageName", Value = pageName });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = companyId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FinancialYearId", Value = FinacialYearId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageNumber", Value = start });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageSize", Value = length });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@SearchValue", Value = searchValue });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = userId });
                        //command.Parameters.Add(new SqlParameter("@ErrorMsg", pageName));
                        SqlParameter pvNewId = new SqlParameter();
                        pvNewId.ParameterName = "@ErrorMsg";
                        pvNewId.DbType = DbType.String;
                        pvNewId.Size = 200;
                        pvNewId.Direction = ParameterDirection.Output;
                        command.Parameters.Add(pvNewId);
                        //command.Transaction = context.Database.Connection.BeginTransaction();
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        DataTable dtSchema = new DataTable("Schema");
                        using (var reader = command.ExecuteReader())
                        {
                            dtSchema = reader.GetSchemaTable();
                            foreach (DataRow schemarow in dtSchema.Rows)
                            {
                                itemCol.Add(schemarow.ItemArray[0].ToString());
                            }
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        if (columnValue.GetType() == typeof(DateTime))
                                        {
                                            DateTime dt = Convert.ToDateTime(columnValue);

                                            // Case 1: Only time (date part is default 01-01-1900)
                                            if (dt.Date == new DateTime(1900, 1, 1) && dt.TimeOfDay != TimeSpan.Zero)
                                            {
                                                columnValue = dt.ToString("HH:mm");
                                            }
                                            // Case 2: Only date (time part is 00:00:00)
                                            else if (dt.TimeOfDay == TimeSpan.Zero && dt.Date != new DateTime(1900, 1, 1))
                                            {
                                                columnValue = dt.ToString("dd/MM/yyyy");
                                            }
                                            // Case 3: Both date and time
                                            else if (dt.Date != new DateTime(1900, 1, 1) && dt.TimeOfDay != TimeSpan.Zero)
                                            {
                                                columnValue = dt.ToString("dd/MM/yyyy HH:mm tt");
                                            }
                                            else
                                            {
                                                columnValue = ""; // Handle empty/invalid
                                            }
                                        }
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    string columnName = reader["FieldName"] != DBNull.Value ? (string)reader["FieldName"] : string.Empty;
                                    string columnTitle = reader["FieldTitle"] != DBNull.Value ? (string)(reader["FieldTitle"]) : string.Empty;
                                    pageGrids.Add(new PageGridData()
                                    {
                                        ColumnName = columnName,
                                        ColumnTitle = columnTitle,
                                    });
                                }
                            }
                            if (reader.NextResult() && reader.Read())
                            {
                                response.TotalRecords = Convert.ToInt32(reader["TotalCount"]);
                            }
                        }
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occured: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            response.Columns = itemCol;
            response.PageData = pageGrids;
            return response;
        }
        public async Task<FormData> GetPageChildDataAsync(string pageName, string companyId, int FinacialYearId, int start, int length, string searchValue, string userId, int ParentId)
        {
            FormData response = new FormData();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            List<PageGridData> pageGrids = new List<PageGridData>();
            try
            {
                string storedProcedureName = "proc_GetDataChildTableList_Paging";
                using (var context = new TFDSolutionEntities())
                {
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageName", Value = pageName });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = companyId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FinancialYearId", Value = FinacialYearId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageNumber", Value = start });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageSize", Value = length });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@SearchValue", Value = searchValue });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = userId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = ParentId });
                        //command.Parameters.Add(new SqlParameter("@ErrorMsg", pageName));
                        SqlParameter pvNewId = new SqlParameter();
                        pvNewId.ParameterName = "@ErrorMsg";
                        pvNewId.DbType = DbType.String;
                        pvNewId.Size = 200;
                        pvNewId.Direction = ParameterDirection.Output;
                        command.Parameters.Add(pvNewId);
                        //command.Transaction = context.Database.Connection.BeginTransaction();
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        DataTable dtSchema = new DataTable("Schema");
                        using (var reader = command.ExecuteReader())
                        {
                            dtSchema = reader.GetSchemaTable();
                            foreach (DataRow schemarow in dtSchema.Rows)
                            {
                                itemCol.Add(schemarow.ItemArray[0].ToString());
                            }
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        if (columnValue.GetType() == typeof(DateTime))
                                        {
                                            DateTime dt = Convert.ToDateTime(columnValue);
                                            // Case 1: Only time (date part is default 01-01-1900)
                                            if (dt.Date == new DateTime(1900, 1, 1) && dt.TimeOfDay != TimeSpan.Zero)
                                            {
                                                columnValue = dt.ToString("HH:mm");
                                            }
                                            // Case 2: Only date (time part is 00:00:00)
                                            else if (dt.TimeOfDay == TimeSpan.Zero && dt.Date != new DateTime(1900, 1, 1))
                                            {
                                                columnValue = dt.ToString("dd/MM/yyyy");
                                            }
                                            // Case 3: Both date and time
                                            else if (dt.Date != new DateTime(1900, 1, 1) && dt.TimeOfDay != TimeSpan.Zero)
                                            {
                                                columnValue = dt.ToString("dd/MM/yyyy HH:mm tt");
                                            }
                                            else
                                            {
                                                columnValue = ""; // Handle empty/invalid
                                            }
                                        }
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    string columnName = reader["FieldName"] != DBNull.Value ? (string)reader["FieldName"] : string.Empty;
                                    string columnTitle = reader["FieldTitle"] != DBNull.Value ? (string)(reader["FieldTitle"]) : string.Empty;
                                    pageGrids.Add(new PageGridData()
                                    {
                                        ColumnName = columnName,
                                        ColumnTitle = columnTitle,
                                    });
                                }
                            }
                            if (reader.NextResult() && reader.Read())
                            {
                                response.TotalRecords = Convert.ToInt32(reader["TotalCount"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occured: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            response.Columns = itemCol;
            response.PageData = pageGrids;
            return response;
        }

        public DataTable getGridData(string tabId, int ParentId, string userId, int ProcessId = 0)
        {
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            DataTable dtData = new DataTable("Data");
            try
            {
                string storedProcedureName = "proc_GetTabDataTableList";
                using (var context = new TFDSolutionEntities())
                {
                    var parameter1 = new SqlParameter { ParameterName = "@TabId", Value = tabId };
                    var parameter3 = new SqlParameter { ParameterName = "@ParentId", Value = ParentId.ToString() };
                    var parameter4 = new SqlParameter { ParameterName = "@UserId", Value = userId };
                    var parameter5 = new SqlParameter { ParameterName = "@ProcessId", Value = ProcessId };
                    var parameter6 = new SqlParameter { ParameterName = "@DetailParentId", Value = 0 };
                    var parameter2 = new SqlParameter("@ErrorMsg", SqlDbType.VarChar, 200)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    dynamic result = context.Database.SqlQuery<dynamic>("exec " + storedProcedureName + " @TabId, ParentId,@UserId, @ProcessId,@DetailParentId, @ErrorMsg out", parameter1, parameter3, parameter4, parameter5, parameter6, parameter2).ToList();
                    string json = result.ToString(); // suppose `dynamicObject` is your input
                    string errorMessage = (string)parameter2.Value;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        //dynamic re = GetTaskExecution(context, pageName);
                        using (var command = context.Database.Connection.CreateCommand())
                        {
                            if (context.Database.Connection.State == ConnectionState.Closed)
                                context.Database.Connection.Open();

                            command.Parameters.Add(new SqlParameter { ParameterName = "@TabId", Value = tabId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = ParentId.ToString() });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = userId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ProcessId", Value = ProcessId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@DetailParentId", Value = 0 });
                            SqlParameter pvNewId = new SqlParameter();
                            pvNewId.ParameterName = "@ErrorMsg";
                            pvNewId.DbType = DbType.String;
                            pvNewId.Size = 200;
                            pvNewId.Direction = ParameterDirection.Output;
                            command.Parameters.Add(pvNewId);
                            command.CommandText = storedProcedureName;
                            command.CommandType = CommandType.StoredProcedure;

                            DataTable dtSchema = new DataTable("Schema");
                            using (var reader = command.ExecuteReader())
                            {
                                dtSchema = reader.GetSchemaTable();
                                foreach (DataRow schemarow in dtSchema.Rows)
                                {
                                    dtData.Columns.Add(schemarow.ItemArray[0].ToString());
                                }
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        object[] ColArray = new object[reader.FieldCount];
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            if (reader[i] != null) ColArray[i] = reader[i];
                                        }
                                        dtData.LoadDataRow(ColArray, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            //if (dtData != null && dtData.Rows.Count > 0) {
            //   items = GetDataTableDictionaryList(dtData);
            //}
            return dtData;
        }
        public static List<Dictionary<string, object>> GetDataTableDictionaryList(DataTable dt)
        {
            return dt.AsEnumerable().Select(
                row => dt.Columns.Cast<DataColumn>().ToDictionary(
                    column => column.ColumnName,
                    column => row[column]
                )).ToList();
        }
        public async Task<FormTabData> GetDynamicTabData(string tabId, int ParentId, string userId, int ProcessId = 0)
        {
            FormTabData response = new FormTabData();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    //dynamic re = GetTaskExecution(context, pageName); 
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();

                        command.Parameters.Add(new SqlParameter { ParameterName = "@TabId", Value = tabId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = ParentId.ToString() });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = userId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ProcessId", Value = ProcessId });
                        SqlParameter pvNewId = new SqlParameter();
                        pvNewId.ParameterName = "@ErrorMsg";
                        pvNewId.DbType = DbType.String;
                        pvNewId.Size = 200;
                        pvNewId.Direction = ParameterDirection.Output;
                        command.Parameters.Add(pvNewId);
                        command.CommandText = "proc_GetTabGridData";
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        if (columnValue.GetType() == typeof(DateTime))
                                        {
                                            DateTime dt = Convert.ToDateTime(columnValue);
                                            if (dt == new DateTime(1900, 1, 1, 0, 0, 0))
                                            {
                                                columnValue = "";
                                            }
                                            else if (dt.Date == new DateTime(1900, 1, 1))
                                            {
                                                columnValue = dt.ToString("HH:mm");
                                            }
                                            else if (dt.TimeOfDay != TimeSpan.Zero)
                                            {
                                                columnValue = dt.ToString("yyyy/MM/dd HH:mm");
                                            }
                                            else
                                            {
                                                columnValue = dt.ToString("yyyy/MM/dd");
                                            }
                                        }
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occured: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            return response;
        }

        public async Task<FormTabData> GetDynamicTabDataFromSPAsync(string tabId, int ParentId, string userId, int ProcessId = 0, int DetailParentId = 0)
        {
            FormTabData response = new FormTabData();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<PageGridData> pageGrids = new List<PageGridData>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            try
            {
                string storedProcedureName = "proc_GetTabDataTableList";
                using (var context = new TFDSolutionEntities())
                {
                    var parameter1 = new SqlParameter { ParameterName = "@TabId", Value = tabId };
                    var parameter3 = new SqlParameter { ParameterName = "@ParentId", Value = ParentId.ToString() };
                    var parameter4 = new SqlParameter { ParameterName = "@UserId", Value = userId };
                    var parameter5 = new SqlParameter { ParameterName = "@ProcessId", Value = ProcessId };
                    var parameter6 = new SqlParameter { ParameterName = "@DetailParentId", Value = DetailParentId };
                    var parameter2 = new SqlParameter("@ErrorMsg", SqlDbType.VarChar, 200)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    dynamic result = context.Database.SqlQuery<dynamic>("exec " + storedProcedureName + " @TabId, ParentId,@UserId, @ProcessId, @DetailParentId, @ErrorMsg out", parameter1, parameter3, parameter4, parameter5, parameter2, parameter6).ToList();
                    string json = result.ToString(); // suppose `dynamicObject` is your input

                    string errorMessage = (string)parameter2.Value;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        //dynamic re = GetTaskExecution(context, pageName); 
                        using (var command = context.Database.Connection.CreateCommand())
                        {
                            if (context.Database.Connection.State == ConnectionState.Closed)
                                context.Database.Connection.Open();

                            command.Parameters.Add(new SqlParameter { ParameterName = "@TabId", Value = tabId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = ParentId.ToString() });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = userId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ProcessId", Value = ProcessId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@DetailParentId", Value = DetailParentId });
                            //command.Parameters.Add(new SqlParameter("@ErrorMsg", pageName));
                            SqlParameter pvNewId = new SqlParameter();
                            pvNewId.ParameterName = "@ErrorMsg";
                            pvNewId.DbType = DbType.String;
                            pvNewId.Size = 200;
                            pvNewId.Direction = ParameterDirection.Output;
                            command.Parameters.Add(pvNewId);
                            //command.Transaction = context.Database.Connection.BeginTransaction();
                            command.CommandText = storedProcedureName;
                            command.CommandType = CommandType.StoredProcedure;
                            DataTable dtSchema = new DataTable("Schema");
                            using (var reader = command.ExecuteReader())
                            {
                                dtSchema = reader.GetSchemaTable();
                                if (dtSchema != null)
                                {
                                    foreach (DataRow schemarow in dtSchema.Rows)
                                    {
                                        itemCol.Add(schemarow.ItemArray[0].ToString());
                                    }
                                }
                                if (reader.HasRows)
                                {
                                    //while (reader.Read())
                                    //{
                                    //    object[] ColArray = new object[reader.FieldCount];
                                    //    for (int i = 0; i < reader.FieldCount; i++)
                                    //    {
                                    //        if (reader[i] != null) ColArray[i] = reader[i];
                                    //    }
                                    //    dtData.LoadDataRow(ColArray, true);
                                    //}
                                    while (await reader.ReadAsync())
                                    {
                                        Dictionary<string, object> obj = new Dictionary<string, object>();
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            string columnName = reader.GetName(i);
                                            object columnValue = reader.GetValue(i);
                                            if (columnValue.GetType() == typeof(DateTime))
                                            {
                                                DateTime dt = Convert.ToDateTime(columnValue);
                                                bool hasOnlyTime = dt.TimeOfDay != TimeSpan.Zero;
                                                if (hasOnlyTime)
                                                {
                                                    TimeSpan timeOnly = dt.TimeOfDay;
                                                    columnValue = dt.ToString("HH:mm");
                                                }
                                                else
                                                {
                                                    columnValue = (dt == new DateTime(1900, 1, 1)) ? "" : dt.ToString("dd/MM/yyyy");
                                                }
                                            }
                                            obj[columnName] = columnValue;
                                        }
                                        items.Add(obj);
                                    }
                                }
                                if (reader.NextResult())
                                {
                                    while (reader.Read())
                                    {
                                        string columnName = reader["FieldName"] != DBNull.Value ? (string)reader["FieldName"] : string.Empty;
                                        string columnTitle = reader["FieldTitle"] != DBNull.Value ? (string)(reader["FieldTitle"]) : string.Empty;
                                        string FieldType = reader["FieldType"] != DBNull.Value ? (string)(reader["FieldType"]) : string.Empty;
                                        bool isSummary = reader["IsSummary"] != DBNull.Value ? (bool)(reader["IsSummary"]) : false;
                                        string _FieldId = reader["FieldId"] != DBNull.Value ? (string)(reader["FieldId"]) : string.Empty;
                                        int templateId = reader["TemplateId"] != DBNull.Value ? (int)(reader["TemplateId"]) : 0;
                                        int fieldDecimal = reader["FieldDecimal"] != DBNull.Value ? (int)(reader["FieldDecimal"]) : 0;
                                        pageGrids.Add(new PageGridData()
                                        {
                                            ColumnName = columnName,
                                            ColumnTitle = columnTitle,
                                            ColumnType = FieldType,
                                            IsSummary = isSummary,
                                            FieldId = _FieldId,
                                            TemplateId = templateId,
                                            FieldDecimal = fieldDecimal
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            response.PageData = pageGrids;
            response.Columns = itemCol;
            return response;
        }
        public async Task<FormTabData> GetDetailButtonData(ItemDetailButtonRequest request)
        {
            FormTabData response = new FormTabData();
            PageTabModel pageTab = new PageTabModel();
            response.IsSuccess = true;
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<PageGridData> pageGrids = new List<PageGridData>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("FieldName", typeof(string));
                permissionTable.Columns.Add("FieldValue", typeof(string));
                permissionTable.Columns.Add("DataType", typeof(string));
                if (request.FieldData != null && request.FieldData.Count > 0)
                {
                    foreach (var item in request.FieldData)
                    {
                        permissionTable.Rows.Add(item.FieldName, item.FieldValue, "");
                    }
                }
                string storedProcedureName = "m_getTabDetailButtonData";
                using (var context = new TFDSolutionEntities())
                {
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = request.CompanyId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FormId", Value = request.FormId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@TabId", Value = request.UserId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = request.ParentId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@DetailId", Value = request.DetailId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ItemSrNo", Value = request.ItemSrNo });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@TemplateId", Value = request.TemplateId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@ReadingCount", Value = request.ReferenceFieldCounter });
                        command.Parameters.Add(new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_FieldsData",
                            Value = permissionTable
                        });
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        DataTable dtSchema = new DataTable("Schema");
                        using (var reader = command.ExecuteReader())
                        {
                            dtSchema = reader.GetSchemaTable();
                            if (dtSchema != null)
                            {
                                foreach (DataRow schemarow in dtSchema.Rows)
                                {
                                    itemCol.Add(schemarow.ItemArray[0].ToString());
                                }
                            }
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        if (columnValue == DBNull.Value || string.IsNullOrEmpty(columnValue.ToString()))
                                        {
                                            if (request.FieldData != null && request.FieldData.Count > 0)
                                            {
                                                var _data = request.FieldData.Where(x => x.FieldName == columnName).FirstOrDefault();
                                                if (_data != null)
                                                {
                                                    columnValue = _data.FieldValue;
                                                }
                                            }
                                            // value is NULL or empty
                                        }
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    PageGridData grid = new PageGridData()
                                    {
                                        ColumnName = reader["FieldName"] != DBNull.Value ? reader["FieldName"].ToString() : "",
                                        ColumnTitle = reader["FieldCaption"] != DBNull.Value ? reader["FieldCaption"].ToString() : "",
                                        ColumnType = reader["FieldType"] != DBNull.Value ? reader["FieldType"].ToString() : "",
                                        ColumnLength = reader["FieldLength"] != DBNull.Value ? Convert.ToInt32(reader["FieldLength"]) : 0,

                                        FieldId = reader["FieldId"] != DBNull.Value ? reader["FieldId"].ToString() : "",
                                        ItemAdvanceId = reader["ItemAdvanceId"] != DBNull.Value ? Convert.ToInt32(reader["ItemAdvanceId"]) : 0,
                                        FieldTypeId = reader["FieldTypeId"] != DBNull.Value ? reader["FieldTypeId"].ToString() : "",
                                        IsActive = reader["IsActive"] != DBNull.Value ? Convert.ToBoolean(reader["IsActive"]) : false,
                                        IsSummary = reader["IsSummary"] != DBNull.Value ? Convert.ToBoolean(reader["IsSummary"]) : false,
                                        IsUnique = reader["IsUnique"] != DBNull.Value ? Convert.ToBoolean(reader["IsUnique"]) : false,

                                        FieldName = reader["FieldName"] != DBNull.Value ? reader["FieldName"].ToString() : "",
                                        FieldCaption = reader["FieldCaption"] != DBNull.Value ? reader["FieldCaption"].ToString() : "",
                                        FieldFormula = reader["FieldFormula"] != DBNull.Value ? reader["FieldFormula"].ToString() : "",

                                        FieldLength = reader["FieldLength"] != DBNull.Value ? Convert.ToInt32(reader["FieldLength"]) : 0,
                                        FieldDecimal = reader["FieldDecimal"] != DBNull.Value ? Convert.ToInt32(reader["FieldDecimal"]) : 0,
                                        FieldType = reader["FieldType"] != DBNull.Value ? reader["FieldType"].ToString() : "",

                                        DDLTextField = reader["DDLTextField"] != DBNull.Value ? reader["DDLTextField"].ToString() : "",
                                        DDLValueField = reader["DDLValueField"] != DBNull.Value ? reader["DDLValueField"].ToString() : "",
                                        DDLSourceType = reader["DDLSourceType"] != DBNull.Value ? reader["DDLSourceType"].ToString() : "",
                                        DDLSourceName = reader["DDLSourceName"] != DBNull.Value ? reader["DDLSourceName"].ToString() : "",

                                        FieldSize = reader["FieldSize"] != DBNull.Value ? reader["FieldSize"].ToString() : "",
                                        FieldPlaceHolder = reader["FieldPlaceHolder"] != DBNull.Value ? reader["FieldPlaceHolder"].ToString() : "",
                                        FieldHelpText = reader["FieldHelpText"] != DBNull.Value ? reader["FieldHelpText"].ToString() : "",

                                        //FieldValue = reader["FieldValue"] != DBNull.Value ? reader["FieldValue"].ToString() : "",
                                        //FieldRemarks = reader["FieldRemarks"] != DBNull.Value ? reader["FieldRemarks"].ToString() : ""
                                    };
                                    pageGrids.Add(grid);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            response.PageData = pageGrids;
            response.Columns = itemCol;
            return response;
        }

        public ResponseModel SaveDetailButtonData(SubmitItemDetailButton request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    if (request.ButtonFieldData != null && request.ButtonFieldData.Count > 0)
                    {
                        foreach (var item in request.ButtonFieldData)
                        {
                            var permissionTable = new DataTable();
                            permissionTable.Columns.Add("FieldName", typeof(string));
                            permissionTable.Columns.Add("FieldValue", typeof(string));
                            permissionTable.Columns.Add("DataType", typeof(string));
                            string jsonData = JsonConvert.SerializeObject(request.ButtonFieldData);
                            if (item.ItemRowData != null && item.ItemRowData.Count > 0)
                            {
                                foreach (var item1 in item.ItemRowData)
                                {
                                    permissionTable.Rows.Add(item1.FieldName, item1.FieldValue, "");
                                }
                            }
                            var parameters = new List<SqlParameter>
                            {
                                new SqlParameter("@FormId", request.FormId),
                                new SqlParameter("@TabId", string.IsNullOrEmpty(request.TabId) ? (object)DBNull.Value : request.TabId),
                                new SqlParameter("@ParentId", request.ParentId),
                                new SqlParameter("@DetailId", request.DetailId),
                                new SqlParameter("@ItemSrNo", request.ItemSrNo),
                                new SqlParameter("@CompanyId", request.CompanyId),
                                new SqlParameter("@FiancialYearId", request.FiancialYearId),
                                new SqlParameter("@UserId", request.UserId),
                                new SqlParameter("@TemplateId", request.TemplateId),
                                new SqlParameter("@FieldsData", SqlDbType.Structured)
                                {
                                    TypeName = "dbo.Udt_FieldsData",
                                    Value = permissionTable
                                }
                            };
                            response = context.Database.SqlQuery<ResponseModel>("EXEC m_Proc_ButtonDetail_Upsert @FormId,@TabId,@ParentId,@DetailId,@ItemSrNo,@CompanyId,@FiancialYearId,@UserId,@TemplateId,@FieldsData",
                                parameters.ToArray()).FirstOrDefault();
                            if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                            {
                                response.Response = "Record has been saved successfully.";
                                response.IsSuccess = true;
                            }
                            else
                            {
                                response.Response = " Error Occured: " + response.Response;
                                response.IsSuccess = false;
                                CommonBusiness.SaveAppLog(new AppLog()
                                {
                                    CompanyId = Convert.ToString(request.CompanyId.Value),
                                    Logger = "MasterBusiness.SaveDetailButtonData",
                                    LogLevel = "INFO",
                                    Message = response.Response,
                                    UserId = Convert.ToString(request.UserId.Value)
                                });
                            }
                        }
                    }
                    else
                    {
                        response.Response = "Record not save successfully. Please try again..";
                        response.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            return response;
        }

        public ResponseModel SavePageReadingData(SubmitItemDetailButton request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("SrNo", typeof(int));
                permissionTable.Columns.Add("RequireDim", typeof(string));
                permissionTable.Columns.Add("LowerLimit", typeof(string));
                permissionTable.Columns.Add("UpperLimit", typeof(string));
                permissionTable.Columns.Add("Parameter", typeof(string));
                permissionTable.Columns.Add("Reading", typeof(string));
                if (request.FieldData != null && request.FieldData.Count > 0)
                {
                    foreach (var item in request.FieldData)
                    {
                        permissionTable.Rows.Add(item.SrNo, item.RequireDim, item.LowerLimit, item.UpperLimit, item.Parameter, item.Reading);
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@TabId", (object)request.TabId ?? DBNull.Value),
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@DetailId", (object)request.DetailId ?? DBNull.Value),
                        new SqlParameter("@ItemSrNo", (object)request.ItemSrNo ?? DBNull.Value),
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@FiancialYearId", request.FiancialYearId),
                        new SqlParameter("@UserId", request.UserId),
                        new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.ReadingTableType",
                            Value = permissionTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC m_SaveTabDetailButtonData @FormId,@TabId,@ParentId,@DetailId, @ItemSrNo,@CompanyId,@FiancialYearId,@UserId,@FieldsData",
                        parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            return response;
        }

        #endregion

        #region Document Numbering

        public ResponseModel SaveDocument(DocumentModel docModal, string CompanyId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var DocId = new SqlParameter("@DocId", (object)docModal.DocNoSettingId ?? DBNull.Value);
                    var DocName = new SqlParameter("@DocName", docModal.DocName);
                    var DocAlias = new SqlParameter("@DocAlias", docModal.DocAlias);
                    var StartNo = new SqlParameter("@StartNo", docModal.StartNumber);
                    var FormId = new SqlParameter("@FormId", docModal.FormId);
                    var IsDefault = new SqlParameter("@IsDefault", docModal.IsDefault);
                    var companyId = new SqlParameter("@CompanyId", CompanyId);
                    // Capture stored procedure result set
                    var result = context.Database.SqlQuery<ResponseModel>(
                        "EXEC proc_SaveFormDocument @DocId, @DocName, @DocAlias, @StartNo, @FormId, @IsDefault, @CompanyId",
                        DocId, DocName, DocAlias, StartNo, FormId, IsDefault, companyId).FirstOrDefault();

                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        public List<DocumentModel> getDocFormList(string formId, string companyId)
        {
            List<DocumentModel> docList = new List<DocumentModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam = new SqlParameter("@FormId", formId);
                    var formParam2 = new SqlParameter("@CompanyId", companyId);
                    docList = context.Database.SqlQuery<DocumentModel>("EXEC proc_GetFormDocument @FormId, @CompanyId", formParam, formParam2).ToList();
                    return docList;
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return docList;
        }
        public DocumentModel getFormDocument(string formId, string companyId)
        {
            DocumentModel model = new DocumentModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam = new SqlParameter("@FormId", formId);
                    var formParam2 = new SqlParameter("@CompanyId", companyId);
                    model = context.Database.SqlQuery<DocumentModel>("EXEC proc_getFormDoc @FormId @CompanyId", formParam, formParam2).FirstOrDefault();
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return model;
        }
        public DocumentModel GetDocument(int DocuId)
        {
            DocumentModel response = new DocumentModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var DocId = new SqlParameter("@DocId", DocuId);
                    response = context.Database.SqlQuery<DocumentModel>("EXEC proc_GetDocument @DocId", @DocId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel DeleteDocument(int DocuId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var DocId = new SqlParameter("@DocId", DocuId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteFormDocument @DocId", @DocId).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        #endregion

        #region Form Pending
        public ResponseModel SaveFormPending(FormPendingModel pendingModal)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var FormPendingId = new SqlParameter("@FormPendingId", (object)pendingModal.FormPendingId ?? DBNull.Value);
                    var FormId = new SqlParameter("@FormId", pendingModal.FormId);
                    var FormTabName = new SqlParameter("@FormTabName", pendingModal.FormTabName);
                    var SourceName = new SqlParameter("@SourceName", pendingModal.SourceName);
                    var SourceRequest = new SqlParameter("@SourceRequest", pendingModal.SourceRequest ?? (object)DBNull.Value);
                    var SortOrder = new SqlParameter("@SortOrder", pendingModal.SortOrder);
                    var UserId = new SqlParameter("@UserId", pendingModal.UserId);
                    var fromFormId = new SqlParameter("@FromFormId", pendingModal.FromFormId);
                    var fromFormTabId = new SqlParameter("@FromFormTabId", pendingModal.FromFormTabId ?? (object)DBNull.Value);
                    var formTabId = new SqlParameter("@FormTabId", pendingModal.FormTabId ?? (object)DBNull.Value);
                    // Capture stored procedure result set
                    var result = context.Database.SqlQuery<ResponseModel>(
                       "exec proc_SaveFormPending @FormPendingId, @FormId, @FormTabName, @SourceName, @SourceRequest, @SortOrder, @UserId, @FromFormId, @FromFormTabId,@FormTabId",
                       FormPendingId, FormId, FormTabName, SourceName, SourceRequest, SortOrder, UserId, fromFormId, fromFormTabId, formTabId).FirstOrDefault();

                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        public List<FormPendingModel> getFormPendingList(string formId)
        {
            List<FormPendingModel> docList = new List<FormPendingModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam = new SqlParameter("@FormId", formId);
                    docList = context.Database.SqlQuery<FormPendingModel>("EXEC proc_GetFormPendingList @FormId", formParam).ToList();
                    return docList;
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return docList;
        }
        public FormPendingModel GetFormPending(int FormPendingId)
        {
            FormPendingModel response = new FormPendingModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var DocId = new SqlParameter("@FormPendingId", FormPendingId);
                    response = context.Database.SqlQuery<FormPendingModel>("EXEC proc_GetFormPending @FormPendingId", @DocId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel DeleteFormPending(int FormPendingId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formPendingId = new SqlParameter("@FormPendingId", FormPendingId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteFormPending @FormPendingId", @formPendingId).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        #endregion

        #region Report

        public List<ReportMast> getFormReport(string ReportType, string formId)
        {
            List<ReportMast> docList = new List<ReportMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@FormId", formId);
                    var formParam2 = new SqlParameter("@ReportType", @ReportType);
                    docList = context.Database.SqlQuery<ReportMast>("EXEC proc_GetReportListByForm @FormId,@ReportType", formParam1, formParam2).ToList();
                    return docList;
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return docList;
        }
        public List<ReportMast> getFormSystemReport(string formId)
        {
            List<ReportMast> docList = new List<ReportMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@FormId", formId);
                    docList = context.Database.SqlQuery<ReportMast>("EXEC proc_GetReportListByFormSystem_New @FormId", formParam1).ToList();
                    return docList;
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return docList;
        }
        public ReportMast getFormReport(int reportId)
        {
            ReportMast response = new ReportMast();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@ReportId", reportId);
                    response = context.Database.SqlQuery<ReportMast>("EXEC proc_GetFormReport @ReportId", formParam1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return response;
        }
        public ReportMast getFormSystemReport(int reportId)
        {
            ReportMast response = new ReportMast();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@ReportId", reportId);
                    response = context.Database.SqlQuery<ReportMast>("EXEC proc_GetFormSystemReport @ReportId", formParam1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return response;
        }
        public ResponseModel SaveFormReport(ReportMast model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@ReportId", model.ReportId);
                    var formParam2 = new SqlParameter("@ReportName", model.ReportName);
                    var formParam3 = new SqlParameter("@ReportTitle", model.ReportTitle);
                    var formParam4 = new SqlParameter("@ReportPath", model.ReportPath);
                    var formParam5 = new SqlParameter("@ReportSource", model.ReportSource);
                    var formParam6 = new SqlParameter("@FormId", model.FormId);
                    var formParam7 = new SqlParameter("@ReportType", model.ReportType);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC usp_FormReport @ReportId,@ReportName,@ReportTitle, @ReportPath,@ReportSource, @FormId, @ReportType",
                        formParam1, formParam2, formParam3, formParam4, formParam5, formParam6, formParam7).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel SaveFormSystemReport(ReportMast model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@ReportId", model.ReportId);
                    var formParam2 = new SqlParameter("@ReportName", model.ReportName);
                    var formParam3 = new SqlParameter("@ReportTitle", model.ReportTitle);
                    var formParam4 = new SqlParameter("@ReportPath", model.ReportPath);
                    var formParam5 = new SqlParameter("@ReportSource", model.ReportSource);
                    var formParam6 = new SqlParameter("@FormId", model.FormId);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC usp_FormSystemReport @ReportId,@ReportName,@ReportTitle, @ReportPath,@ReportSource, @FormId",
                        formParam1, formParam2, formParam3, formParam4, formParam5, formParam6).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel UpdateRecordStatus(string formId, int status, int ParentId, int Id, string cancelReason, string UserId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@ParentId", ParentId);
                    var formParam2 = new SqlParameter("@Id", Id);
                    var formParam3 = new SqlParameter("@Status", status);
                    var formParam4 = new SqlParameter("@FromId", formId);
                    var formParam5 = new SqlParameter("@CancelReason", cancelReason);
                    var formParam6 = new SqlParameter("@UserId", UserId);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC sp_UpdatePageStatus @ParentId,@Id,@Status, @FromId, @CancelReason, @UserId",
                        formParam1, formParam2, formParam3, formParam4, formParam5, formParam6).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel RunPageStatusScript(string pageStatusScript, string formId, int status, int ParentId, string UserId, string CompanyId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@FormId", formId);
                    var formParam2 = new SqlParameter("@CompanyId", CompanyId);
                    var formParam3 = new SqlParameter("@UserId", UserId);
                    var formParam4 = new SqlParameter("@ParentId", ParentId);
                    var formParam5 = new SqlParameter("@Status", status);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC " + pageStatusScript + " @FormId,@CompanyId,@UserId,@ParentId,@Status",
                        formParam1, formParam2, formParam3, formParam4, formParam5).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public int CheckFormSettingForSendEmail(string formId)
        {
            ResponseModel response = new ResponseModel();
            using (var context = new TFDSolutionEntities())
            {
                var param = new SqlParameter("@FromId", formId);

                var result = context.Database
                    .SqlQuery<int>("EXEC sp_CheckFormSettingForSendEmail @FromId", param)
                    .FirstOrDefault();

                return result;
            }
        }
        public async Task<List<FormInquiryData>> GetVendorInquiryList(int parentId, string formId)
        {
            using (var context = new TFDSolutionEntities())
            {
                var param = new SqlParameter("@ParentId", parentId);
                var param1 = new SqlParameter("@FormId", formId);
                var list = await context.Database
                    .SqlQuery<FormInquiryData>("EXEC usp_FormInquiryEmailNotification @ParentId, @FormId", param, param1)
                    .ToListAsync();

                return list;
            }
        }
        public async Task<(List<Dictionary<string, object>> master, List<Dictionary<string, object>> details)> GetFormInquiryDetail(int formPendingId, string itemSrNo)
        {
            var masterList = new List<Dictionary<string, object>>();
            var detailList = new List<Dictionary<string, object>>();
            using (var context = new TFDSolutionEntities())
            {
                if (context.Database.Connection.State == ConnectionState.Closed)
                {
                    context.Database.Connection.Open();
                }
                var parameters = new Dictionary<string, object>
                {
                    { "@FormPendingId", formPendingId },
                    { "@ITEMSRNO", itemSrNo }
                };
                DataSet ds = DbHelper.GetDataSet(
                    context,
                    "m_GetFormInquiryDetail",
                    CommandType.StoredProcedure,
                    parameters
                );
                if (ds != null && ds.Tables.Count > 0)
                {
                    masterList = CommonBusiness.DataTableToList(ds.Tables[0]);
                }
                if (ds != null && ds.Tables.Count > 1)
                {
                    detailList = CommonBusiness.DataTableToList(ds.Tables[1]);
                }
            }

            return (masterList, detailList);
        }
        public async Task<ResponseModel> AddSentEmailAuditLog(string formId, int ParentId, string UserId, string URNNo)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var Param1 = new SqlParameter("@ParentId", ParentId);
                    var Param2 = new SqlParameter("@formId", formId);
                    var Param3 = new SqlParameter("@UserId", UserId);
                    var Param4 = new SqlParameter("@URNNo", URNNo);
                    response = await context.Database.SqlQuery<ResponseModel>("EXEC m_AddSentEmailAuditLog @ParentId, @FromId, @UserId, @URNNo",
                        Param1, Param2, Param3, Param4).FirstOrDefaultAsync();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel DeleteItemDetailRecord(string ids, string TabId, int ParentId, string URNNo, string UserId, string IPAddress)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@Ids", ids);
                    var formParam2 = new SqlParameter("@TabFromId", TabId);
                    var formParam3 = new SqlParameter("@ParentId", ParentId);
                    var formParam4 = new SqlParameter("@UserId", UserId);
                    var formParam5 = new SqlParameter("@URNNo", URNNo);
                    var formParam6 = new SqlParameter("@IPAddress", IPAddress);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteItemDetailRecord @Ids,@TabFromId,@ParentId,@UserId,@URNNo,@IPAddress",
                        formParam1, formParam2, formParam3, formParam4, formParam5, formParam6).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public ResponseModel DeleteFormReport(int FormReportId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formReportId = new SqlParameter("@FormReportId", FormReportId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteFormReport @FormReportId", @formReportId).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        public ResponseModel DeleteFormSystemReport(int FormReportId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formReportId = new SqlParameter("@FormReportId", FormReportId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteFormSystemReport @FormReportId", @formReportId).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        #endregion Report

        #region "User"
        public List<UserMast> GetUserList()
        {
            List<UserMast> UserList = new List<UserMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    UserList = context.Database.SqlQuery<UserMast>("EXEC proc_GetUserMastList").ToList();
                    return UserList;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.GetUserList", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return UserList;
        }
        public UserDetail GetUser(string guid)
        {
            UserDetail user = null;
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "proc_GetUserDetail";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", guid ?? (object)DBNull.Value));
                        using (var reader = cmd.ExecuteReader())
                        {
                            // First result set: User main details
                            if (reader.Read())
                            {
                                user = new UserDetail
                                {
                                    UserId = reader["UserId"] != DBNull.Value ? (Guid)reader["UserId"] : Guid.Empty,
                                    FirstName = reader["FirstName"] as string,
                                    LastName = reader["LastName"] as string,
                                    EmailId = reader["EmailId"] as string,
                                    UserRole = reader["UserRole"] != DBNull.Value ? Convert.ToInt32(reader["UserRole"]) : default(int),
                                    UserName = reader["UserName"] as string,
                                    IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"],
                                    CreatedOn = reader["CreatedOn"] != DBNull.Value ? (DateTime)reader["CreatedOn"] : DateTime.MinValue,
                                    UpdatedOn = reader["UpdatedOn"] as DateTime?,
                                    LastLoginDate = reader["LastLoginDate"] as DateTime?,
                                    PasswordExpiredOn = reader["PasswordExpiredOn"] as DateTime?,
                                    Address1 = reader["Address1"] as string,
                                    Password = reader["Password"] as string,
                                    Address2 = reader["Address2"] as string,
                                    City = reader["City"] as int?,
                                    State = reader["State"] as int?,
                                    Country = reader["Country"] as int?,
                                    Mobile = reader["MobileNumber"] as string,
                                    Pincode = reader["Pincode"] as string,
                                    ContactNo = reader["ContactNo"] as string,
                                    ContactPerson = reader["ContactPerson"] as string,
                                    ContactEmail = reader["ContactEmail"] as string,
                                    FromEmail = reader["FromEmail"] as string,
                                    FromPassword = reader["FormEmailPassword"] as string,
                                    DisplayName = reader["DisplayName"] as string,
                                    SmtpServer = reader["SmtpServer"] as string,
                                    SmtpPort = reader["SmtpPort"] as string,
                                    EnableSSL = reader["EnableSSL"] != DBNull.Value ? (bool)reader["EnableSSL"] : false,
                                    DashboardId = reader["DefaultDashboardId"] != DBNull.Value ? Convert.ToInt32(reader["DefaultDashboardId"]) : default(int),
                                };
                            }

                            // Second result set: UserCompanies
                            if (reader.NextResult() && user != null)
                            {
                                user.UserCompanies = new List<UserCompany>();
                                while (reader.Read())
                                {
                                    user.UserCompanies.Add(new UserCompany
                                    {
                                        CompanyId = reader["CompanyId"] != DBNull.Value ? (Guid)reader["CompanyId"] : Guid.Empty,
                                        DefaultFiancialId = reader["DefaultFiancialYearId"] != DBNull.Value ? Convert.ToInt32(reader["DefaultFiancialYearId"]) : 0
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (if logging is implemented)
                CommonBusiness.LogEx(ex);
                //throw new Exception("An error occurred while fetching user details.", ex);
            }
            return user;
        }

        public ResponseModel SaveUserDetails(UserDetail userDetail)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@UserId", userDetail.UserId == Guid.Empty ? (object)DBNull.Value : userDetail.UserId),
                        new SqlParameter("@FirstName", userDetail.FirstName),
                        new SqlParameter("@LastName", userDetail.LastName),
                        new SqlParameter("@EmailId", userDetail.EmailId),
                        new SqlParameter("@UserName", userDetail.UserName),
                        new SqlParameter("@IsActive", userDetail.IsActive),
                        new SqlParameter("@Address1", userDetail is UserDetail detail ? (object)detail.Address1 ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@Address2", userDetail is UserDetail detail2 ? (object)detail2.Address2 ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@City", userDetail is UserDetail detail3 ? (object)detail3.City ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@StateId", userDetail is UserDetail detail4 ? (object)detail4.State ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@CountryId", userDetail is UserDetail detail5 ? (object)detail5.Country ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@ZipCode", userDetail is UserDetail detail6 ? (object)detail6.ZipCode ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@Pincode", userDetail is UserDetail detail7 ? (object)detail7.Pincode ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@ContactNo", userDetail is UserDetail detail8 ? (object)detail8.ContactNo ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@ContactPerson", userDetail is UserDetail detail9 ? (object)detail9.ContactPerson ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@ContactEmail", userDetail is UserDetail detail10 ? (object)detail10.ContactEmail ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@MobileNo", userDetail is UserDetail detail11 ? (object)detail11.Mobile ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@RoleId", userDetail is UserDetail detail12 ? (object)detail12.UserRole?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@FromEmail", userDetail is UserDetail detail13 ? (object)detail13.FromEmail?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@Password", userDetail is UserDetail detail14 ? (object)detail14.FromPassword?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@DisplayName", userDetail is UserDetail detail15 ? (object)detail15.DisplayName?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@SMTPServer", userDetail is UserDetail detail16 ? (object)detail16.SmtpServer?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@SMTPPort", userDetail is UserDetail detail17 ? (object)detail17.SmtpPort?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@EnableSSL", userDetail is UserDetail detail18 ? (object)detail18.EnableSSL?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@DefaultDashboardId", userDetail is UserDetail detail19 ? (object)detail19.DashboardId?? DBNull.Value : DBNull.Value),
                    };
                    // Execute the stored procedure
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_SaveUserDetail @UserId, @FirstName, @LastName, @EmailId, @UserName, @IsActive, " +
                        "@Address1, @Address2, @City, @StateId, @CountryId, @ZipCode, @Pincode, @ContactNo, @ContactPerson, @ContactEmail, @MobileNo, @RoleId, @FromEmail, @Password, " +
                        "@DisplayName, @SMTPServer, @SMTPPort, @EnableSSL, @DefaultDashboardId", parameters.ToArray()).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                    if (response.IsSuccess == true)
                    {
                        response.Response = "User and associated companies saved successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                // Log the exception (if logging is implemented)
                response.IsSuccess = false;
                response.Response = $"An error occurred while saving the user: {ex.Message}";
            }

            return response;
        }
        public ResponseModel SaveUserWithCompanies(UserDetail userDetail)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var companyTable = new DataTable();
                companyTable.Columns.Add("UserId", typeof(Guid));
                companyTable.Columns.Add("CompanyId", typeof(Guid));
                companyTable.Columns.Add("FinancialYearId", typeof(int));
                if (userDetail.UserCompanies != null && userDetail.UserCompanies.Count > 0)
                {
                    // Convert UserCompanies to DataTable
                    foreach (var company in userDetail.UserCompanies)
                    {
                        //companyTable.Rows.Add(company.CompanyId, userDetail.UserId);
                        companyTable.Rows.Add(userDetail.UserId, company.CompanyId);
                    }
                }

                using (var context = new TFDSolutionEntities())
                {
                    // Save user details
                    var userResponse = SaveUserDetails(userDetail);
                    if (!userResponse.IsSuccess.HasValue || !userResponse.IsSuccess.Value)
                    {
                        return userResponse;
                    }
                    response.Response = "User details saved successfully.";
                    // Save associated companies
                    if (companyTable != null && companyTable.Rows.Count > 0)
                    {

                        // Call SaveUserSettings for each company
                        var settingsResponse = SaveUserSettings(companyTable);
                        if (!settingsResponse.IsSuccess.HasValue || !settingsResponse.IsSuccess.Value)
                        {
                            return settingsResponse; // Return error if saving settings fails
                        }

                        response.Response = "User and associated companies saved successfully.";

                    }
                    else
                    {
                        var userIddelet = userDetail.UserId;
                        var delParams = new List<SqlParameter>
                        {
                            new SqlParameter("@UserId", userIddelet)
                        };
                        context.Database.ExecuteSqlCommand("DELETE FROM m_UserSettings WHERE UserId = @UserId", delParams.ToArray());

                    }
                    response.IsSuccess = true;

                }

            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"An error occurred while saving the user: {ex.Message}";
            }

            return response;
        }
        public ResponseModel SaveUserSettings(DataTable companyTable)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    //// Prepare parameters for the stored procedure
                    //var parameters = new List<SqlParameter>
                    //{

                    //         new SqlParameter("@CompanyTable", companyTable) { SqlDbType = SqlDbType.Structured }
                    //};
                    //context.Database.ExecuteSqlCommand("EXEC proc_setUserSettings @UserId, @CompanyId, @DefaultFiancialYearId", parameters.ToArray());
                    var userIddelet = (Guid)companyTable.Rows[0]["UserId"];
                    var delParams = new List<SqlParameter>
                    {
                        new SqlParameter("@UserId", userIddelet)
                    };
                    context.Database.ExecuteSqlCommand("DELETE FROM m_UserSettings WHERE UserId = @UserId", delParams.ToArray());

                    foreach (DataRow row in companyTable.Rows)
                    {
                        var userId = (Guid)row["UserId"];
                        var companyId = (Guid)row["CompanyId"];
                        var financialYearId = row["FinancialYearId"] != DBNull.Value ? (int?)row["FinancialYearId"] : 0;

                        var parameters = new List<SqlParameter>
                            {
                                new SqlParameter("@UserId", userId),
                                new SqlParameter("@CompanyId", companyId),
                               new SqlParameter("@DefaultYearID", (object)financialYearId ?? DBNull.Value)
                            };
                        context.Database.ExecuteSqlCommand("EXEC proc_setUserSettings @UserId, @CompanyId, @DefaultYearID", parameters.ToArray());
                    }

                    // Set success response
                    response.IsSuccess = true;
                    response.Response = "User settings saved successfully.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                // Log the exception (if logging is implemented)
                response.IsSuccess = false;
                response.Response = $"An error occurred while saving user settings: {ex.Message}";
            }

            return response;
        }

        public ResponseModel DeleteUser(Guid guid)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var userIdParam = new SqlParameter("@UserId", guid);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteUserMast @UserId", userIdParam).FirstOrDefault();

                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Error deleting user! " + ex.Message;
            }
            return response;
        }

        #endregion "User"

        #region "Roles & Permission"
        public List<RoleMast> GetRoleList()
        {
            List<RoleMast> RoleList = new List<RoleMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    RoleList = context.Database.SqlQuery<RoleMast>("EXEC proc_GetRoles").ToList();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                //throw new Exception("An error occurred while fetching Role details.", ex);
            }
            return RoleList;
        }
        public RoleDetail GetRolePermissionDetails(int RoleId)
        {
            RoleDetail roleDetail = new RoleDetail();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "proc_GetRolePermissionDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@RoleId", RoleId));

                        using (var reader = cmd.ExecuteReader())
                        {
                            // First result set: User main details
                            if (reader.Read())
                            {
                                roleDetail = new RoleDetail
                                {
                                    RoleId = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"] as string,
                                    Description = reader["Description"] as string,
                                    Status = reader["Status"] != DBNull.Value && (bool)reader["Status"],
                                    CreatedOn = reader["CreatedOn"] != DBNull.Value ? (DateTime)reader["CreatedOn"] : DateTime.MinValue,
                                };
                            }

                            // Second result set: UserCompanies
                            if (reader.NextResult() && roleDetail != null)
                            {
                                roleDetail.rolePermissions = new List<RolePermission>();
                                roleDetail.roleParentForms = new List<RoleParentForm>();
                                while (reader.Read())
                                {
                                    if (reader["ParentFormId"] == DBNull.Value)
                                    {
                                        roleDetail.roleParentForms.Add(new RoleParentForm
                                        {
                                            FormId = reader["FormId"] != DBNull.Value ? (Guid)reader["FormId"] : Guid.Empty,
                                            FormName = reader["FormName"] as string
                                        });
                                    }
                                    else
                                    {

                                        roleDetail.rolePermissions.Add(new RolePermission
                                        {
                                            RolePermissionID = reader["RolePermissionID"] != DBNull.Value ? Convert.ToInt32(reader["RolePermissionID"]) : 0,
                                            RoleID = reader["RoleID"] != DBNull.Value ? Convert.ToInt32(reader["RoleID"]) : 0,
                                            FormId = reader["FormId"] != DBNull.Value ? (Guid)reader["FormId"] : Guid.Empty,
                                            ParentFormId = reader["ParentFormId"] != DBNull.Value ? (Guid)reader["ParentFormId"] : Guid.Empty,
                                            FormName = reader["FormName"] as string,
                                            ParentFormName = reader["ParentFormName"] as string,
                                            FullAccess = reader["FullAccess"] != DBNull.Value ? (bool)reader["FullAccess"] : false,
                                            CanAdd = reader["CanAdd"] != DBNull.Value ? (bool)reader["CanAdd"] : false,
                                            CanEdit = reader["CanEdit"] != DBNull.Value ? (bool)reader["CanEdit"] : false,
                                            CanDelete = reader["CanDelete"] != DBNull.Value ? (bool)reader["CanDelete"] : false,
                                            CanPrint = reader["CanPrint"] != DBNull.Value ? (bool)reader["CanPrint"] : false,
                                            CanApprove = reader["CanApprove"] != DBNull.Value ? (bool)reader["CanApprove"] : false,
                                            CreatedBy = reader["CreatedBy"] != DBNull.Value ? (Guid)reader["CreatedBy"] : Guid.Empty,
                                            CreatedOn = reader["CreatedOn"] != DBNull.Value ? (DateTime)reader["CreatedOn"] : DateTime.MinValue,
                                            UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? (Guid?)reader["UpdatedBy"] : null,
                                            UpdatedOn = reader["UpdatedOn"] != DBNull.Value ? (DateTime?)reader["UpdatedOn"] : null

                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                //throw new Exception("An error occurred while fetching Role permission details.", ex);
            }
            return roleDetail;
        }
        public ResponseModel DeleteRole(int roleId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var RoleIdParam = new SqlParameter("@RoleId", roleId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteRole @RoleId", RoleIdParam).FirstOrDefault();

                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Error deleting user! " + ex.Message;
            }
            return response;
        }
        public ResponseModel SaveRolePermissionsRoleWise(RoleDetail model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("RolePermissionID", typeof(int));
                permissionTable.Columns.Add("RoleID", typeof(int));
                permissionTable.Columns.Add("FormId", typeof(Guid));
                permissionTable.Columns.Add("FullAccess", typeof(bool));
                permissionTable.Columns.Add("CanAdd", typeof(bool));
                permissionTable.Columns.Add("CanEdit", typeof(bool));
                permissionTable.Columns.Add("CanDelete", typeof(bool));
                permissionTable.Columns.Add("CanPrint", typeof(bool));
                permissionTable.Columns.Add("CanApprove", typeof(bool));

                if (model.rolePermissions != null && model.rolePermissions.Count > 0)
                {
                    foreach (var item in model.rolePermissions)
                    {
                        permissionTable.Rows.Add(
                            item.RolePermissionID,
                            model.RoleId,
                            item.FormId,
                            item.FullAccess,
                            item.CanAdd,
                            item.CanEdit,
                            item.CanDelete,
                            item.CanPrint,
                            item.CanApprove
                        );
                    }
                }

                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                         new SqlParameter("@RoleId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.InputOutput,
                            Value = model.RoleId > 0 ? model.RoleId : 0
                        },
                        new SqlParameter("@RoleName", (object)model.RoleName ?? DBNull.Value),
                        new SqlParameter("@Description", (object)model.Description ?? DBNull.Value),
                        new SqlParameter("@Status", model.Status),
                        new SqlParameter("@UserId", model.UserId == Guid.Empty ? (object)DBNull.Value : model.UserId),
                        new SqlParameter("@PermissionTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_RolePermissionType",
                            Value = permissionTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveRoleWithPermissions @RoleId,@RoleName,@Description,@Status,  @UserId,@PermissionTable", parameters.ToArray()).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }
        #endregion "Roles & Permission"

        #region Form Settings
        public List<FormSettings> getFormSettings(string formId, string type = "")
        {
            List<FormSettings> response = new List<FormSettings>();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    var FormId = new SqlParameter("@FormId", formId);
                    var Type = new SqlParameter("@Type", type != null ? (object)type : DBNull.Value);
                    response = content.Database.SqlQuery<FormSettings>("proc_getFormSettings @FormId, @Type",
                       FormId, Type).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.getFormSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }
        public List<FormSettings> getFormTabSettings(string formId, string tabId, string type = "")
        {
            List<FormSettings> response = new List<FormSettings>();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    var FormId = new SqlParameter("@FormId", formId);
                    var TabId = new SqlParameter("@TabId", tabId);
                    var Type = new SqlParameter("@Type", type != null ? (object)type : DBNull.Value);
                    response = content.Database.SqlQuery<FormSettings>("proc_getFormSettingsByTabId @FormId, @TabId, @Type",
                       FormId, TabId, Type).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.getFormSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }
        public ResponseModel SaveFormSettings(List<FormSettings> model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var settingTable = new DataTable();
                settingTable.Columns.Add("FormId", typeof(string));
                settingTable.Columns.Add("KeyName", typeof(string));
                settingTable.Columns.Add("KeyValue", typeof(string));
                settingTable.Columns.Add("FormTabId", typeof(string));
                if (model != null && model.Count > 0)
                {
                    foreach (var item in model)
                    {
                        settingTable.Rows.Add(item.FormId, item.KeyName, item.KeyValue, item.FormTabId);
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@FormSettings", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_FormSettingType",
                            Value = settingTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_SaveFormSettings @FormSettings", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }

        public List<FormStockSettings> getFormStockSettings(string formId)
        {
            List<FormStockSettings> response = new List<FormStockSettings>();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    response = content.Database.SqlQuery<FormStockSettings>("proc_GetFormStockSettings @FormId",
                        new SqlParameter("@FormId", formId)).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.getFormStockSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }

        public ResponseModel SaveFormStockSettings(List<FormStockSettings> model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var settingTable = new DataTable();
                settingTable.Columns.Add("FormId", typeof(string));
                settingTable.Columns.Add("FormTabId", typeof(string));
                settingTable.Columns.Add("StockEffect", typeof(string));
                settingTable.Columns.Add("StoredProcedure", typeof(string));
                if (model != null && model.Count > 0)
                {
                    foreach (var item in model)
                    {
                        settingTable.Rows.Add(
                            item.FormId,
                            item.FormTabId,
                            item.StockEffect,
                            item.StoredProcedure
                        );
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@FormStockSettings", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_FormStockSettingType",
                            Value = settingTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_SaveFormStockSettings @FormStockSettings", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }

        #endregion

        #region currency 
        public CompanyMast GetCurrencyByComapanyID(Guid ComapnyID)
        {
            CompanyMast Item = new CompanyMast();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var oId = new SqlParameter("@ComapnyID", ComapnyID);
                    //context.proc_getItem`
                    // m_CompanyMast parentForm = context.m_CompanyMast.Where(x => x.CompanyId == ComapnyID).FirstOrDefault();
                    Item = context.Database.SqlQuery<CompanyMast>("EXEC proc_getCurrentCurrencyByCompanyID @ComapnyID", @oId).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Item;
        }

        #endregion

        #region Bank Details 
        public List<CompanyBank> GetBankDetails(string CompanyId)
        {
            List<CompanyBank> BankDetails = new List<CompanyBank>();
            try
            {
                Guid cId = Guid.Parse(CompanyId);
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@CompanyId", CompanyId ),
                    };
                    BankDetails = context.Database.SqlQuery<CompanyBank>("EXEC proc_GetCompanyBankList  @CompanyId", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return BankDetails;
        }

        public ResponseModel SavebankDetails(CompanyBank modal)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    if (modal.BankId == Guid.Empty)
                    {
                        modal.BankId = Guid.NewGuid();
                    }
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@BankId", Convert.ToString(modal.BankId)),
                            new SqlParameter("@CompanyId", modal.CompanyId),
                            new SqlParameter("@BankName", (object)modal.BankName ?? DBNull.Value),
                            new SqlParameter("@BranchName", (object)modal.BranchName ?? DBNull.Value),
                            new SqlParameter("@BranchCode", (object)modal.BranchCode ?? DBNull.Value),
                            new SqlParameter("@BranchAddress1", (object)modal.BranchAddress1 ?? DBNull.Value),
                            new SqlParameter("@BranchAddress2", (object)modal.BranchAddress2 ?? DBNull.Value),
                            new SqlParameter("@CityId", modal.CityId),
                            new SqlParameter("@AccountNo", (object)modal.AccountNo ?? DBNull.Value),
                            new SqlParameter("@IFSCCode", (object)modal.IFSCCode ?? DBNull.Value),
                            new SqlParameter("@SwiftCde", (object)modal.SwiftCde ?? DBNull.Value),
                            new SqlParameter("@UserId", modal.CreatedBy),

                        };

                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveCompanyBank @BankId, @CompanyId, " +
                        "@BankName, @BranchName, @BranchCode," +
                        " @BranchAddress1, @BranchAddress2, @CityId, @AccountNo, @IFSCCode, " +
                        "@SwiftCde, @UserId", parameters.ToArray()).FirstOrDefault();

                    response.Action = "Save";
                    response.IsSuccess = true;
                    response.Response = "Record save successfully.";

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Record not saved successfully. Error: " + ex.Message;
            }
            return response;
        }

        public CompanyBank getBankDetailsForEdit(Guid BankId)
        {
            CompanyBank BankDetails = new CompanyBank();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@BankId", BankId ),
                    };
                    BankDetails = context.Database.SqlQuery<CompanyBank>("EXEC proc_getBankDetailsUpdate  @BankId", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return BankDetails;
        }

        public ResponseModel DeleteCompanyBankDetails(Guid BankId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@BankId", BankId);

                    // Use SqlQuery<string>() since we expect a list of table names (strings)
                    response = context.Database.SqlQuery<ResponseModel>("EXEC procDeleteCompanyBankDetails @BankId ", param1).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response = new ResponseModel();
                response.IsSuccess = false;
                response.Action = "Delete";
                response.Response = "Record not deleted";
            }
            return response;
        }
        #endregion

        #region UNIT details
        public List<CompanyUnit> GetUnitDetails(string CompanyId)
        {
            List<CompanyUnit> UnitDetails = new List<CompanyUnit>();
            try
            {
                Guid cId = Guid.Parse(CompanyId);
                using (var context = new TFDSolutionEntities())
                {

                    var parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@CompanyId", CompanyId ),
                        };
                    UnitDetails = context.Database.SqlQuery<CompanyUnit>("EXEC proc_GetCompanyUnitList  @CompanyId", parameters.ToArray()).ToList();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return UnitDetails;
        }

        public ResponseModel SaveUnitDetails(CompanyUnit model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    if (model.CompanyUnitId == Guid.Empty)
                    {
                        model.CompanyUnitId = Guid.NewGuid();
                    }
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@CompanyUnitId", model.CompanyUnitId),
                        new SqlParameter("@SiteName", (object)model.SiteName ?? DBNull.Value),
                        new SqlParameter("@Address1", (object)model.Address1 ?? DBNull.Value),
                        new SqlParameter("@Address2", (object)model.Address2 ?? DBNull.Value),
                        new SqlParameter("@CityId", model.CityId),
                        new SqlParameter("@StateId", model.StateId),
                        new SqlParameter("@CountryId", model.CountryId),
                        new SqlParameter("@Pincode", (object)model.Pincode ?? DBNull.Value),
                        new SqlParameter("@GSTNo", (object)model.GSTNo ?? DBNull.Value),
                        new SqlParameter("@PANNo", (object)model.PANNo ?? DBNull.Value),
                        new SqlParameter("@Phone", (object)model.Phone ?? DBNull.Value),
                        new SqlParameter("@Email", (object)model.Email ?? DBNull.Value),
                        new SqlParameter("@CompanyId", model.CompanyId == Guid.Empty ? (object)DBNull.Value : model.CompanyId),
                        new SqlParameter("@CreatedBy", model.CreatedBy),
                        new SqlParameter("@UpdatedBy", model.UpdatedBy)
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveCompanyUnit  @CompanyUnitId, @SiteName, @Address1, @Address2, @CityId, @StateId, @CountryId,@Pincode, @GSTNo, @PANNo, @Phone, @Email, @CompanyId, @CreatedBy, @UpdatedBy", parameters.ToArray()).FirstOrDefault();
                    response.Action = "Save";
                    response.IsSuccess = true;
                    response.Response = "Record save successfully.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Record not saved successfully. Error: " + ex.Message;
            }
            return response;
        }

        public CompanyUnit getUnitDetailsForEdit(Guid CompanyUnitId)
        {
            CompanyUnit BankDetails = new CompanyUnit();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@CompanyUnitId", CompanyUnitId ),
                    };
                    BankDetails = context.Database.SqlQuery<CompanyUnit>("EXEC proc_getUnitDetailsUpdate @CompanyUnitId", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return BankDetails;
        }
        public ResponseModel DeleteCompanyUnitDetails(Guid CompanyUnitId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@CompanyUnitId", CompanyUnitId);

                    // Use SqlQuery<string>() since we expect a list of table names (strings)
                    response = context.Database.SqlQuery<ResponseModel>("EXEC procDeleteCompanyUnitDetails @CompanyUnitId ", param1).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response = new ResponseModel();
                response.IsSuccess = false;
                response.Action = "Delete";
                response.Response = "Record not deleted";
            }
            return response;
        }
        #endregion

        #region "SortingOrder"

        //Added  for update SortOrder for Forms


        public ResponseModel UpdateFormSortOrder(List<FormSortOrderUpdateModel> updatedRows)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DataTable SortDataTable = CommonBusiness.GetSortingTableForModel(updatedRows);
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@SortOrderTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_SortOrderData",
                            Value = SortDataTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_UpdateFormSortOrder @SortOrderTable", parameters.ToArray()).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }
        public ResponseModel UpdateTabSortOrder(List<FormSortOrderUpdateModel> updatedTabs)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DataTable SortDataTable = CommonBusiness.GetSortingTableForModel(updatedTabs);
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@SortOrderTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_SortOrderData",
                            Value = SortDataTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_UpdateTabSortOrder @SortOrderTable", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }

        public ResponseModel UpdateFielSortOrder(List<FormSortOrderUpdateModel> updatedFields)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DataTable SortDataTable = CommonBusiness.GetSortingTableForModel(updatedFields);
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@SortOrderTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_SortOrderData",
                            Value = SortDataTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_UpdateFieldSortOrder @SortOrderTable", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }

        #endregion

        #region Next button Store Procedure Call 
        //public string GetIsNextButtonStoreProcedure(string TabId)
        //{
        //    string result = "";
        //    try
        //    {
        //        using (var context = new TFDSolutionEntities())
        //        {
        //            var param = new SqlParameter("@TabId", TabId);
        //            var query = context.Database.SqlQuery<string>("EXEC proc_GetNextButtonStoreProcedure @TabId", param);
        //            result = query.FirstOrDefault();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return result;
        //}

        //public ResponseModel NextProcedureButtonCall(int id, string NextProcedureValue,Guid userId)
        //{
        //    ResponseModel response = new ResponseModel();
        //    try
        //    {
        //        using (var context = new TFDSolutionEntities())
        //        {
        //            var param1 = new SqlParameter("@ParentId", id);
        //            var param2 = new SqlParameter("@UserId", userId);
        //            var query = context.Database.SqlQuery<string>("EXEC " + NextProcedureValue + " @ParentId,@UserId", param1,param2);
        //            response.Action = "Save";
        //            response.IsSuccess = true;
        //            response.Response = "Record Save successfully.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Action = "Error";
        //        response.Response = "Error deleting record!";
        //        response.IsSuccess = false;
        //    }
        //    return response;
        //}
        #endregion

        #region Exchange Currency

        public ResponseModel ExchnageCurrency(int id)
        {
            ResponseModel obj = new ResponseModel();
            obj.Response = "₹0";
            ExchangeRateResponse objExchangeRateResponse = new ExchangeRateResponse();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param = new SqlParameter("@id", id);
                    objExchangeRateResponse = context.Database.SqlQuery<ExchangeRateResponse>("EXEC proc_getExchangeRate @id", param).FirstOrDefault();
                    if (objExchangeRateResponse != null && !string.IsNullOrEmpty(objExchangeRateResponse.alias))
                    {
                        string _key = Convert.ToString(System.Configuration.ConfigurationSettings.AppSettings["ExchangeRateKey"]);
                        obj.IsSuccess = true;
                        obj.Response = "₹0";
                        string baseCurrencyCode = "INR";
                        //var apiUrl = $"https://api.forexrateapi.com/v1/latest?api_key=YOUR_API_KEY&base={baseCurrencyCode}&currencies={currencyCodes}";
                        var apiUrl = $"https://v6.exchangerate-api.com/v6/{_key}/pair/{objExchangeRateResponse.alias}/{baseCurrencyCode}";
                        //var apiUrl = "https://open.er-api.com/v6/latest/INR";
                        var options = new RestClientOptions(apiUrl);
                        RestClient client = new RestClient(options);
                        RestRequest request = new RestRequest("", Method.Get);
                        RestResponse response = client.Execute(request);
                        var exchangeResponse = JsonConvert.DeserializeObject<API_ConversionRate>(response.Content);
                        if (exchangeResponse != null && exchangeResponse.result == "success")
                        {
                            obj.Response = "₹" + Convert.ToString(Math.Round(exchangeResponse.conversion_rate, 2));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                obj.IsSuccess = false;
                obj.Response = "Exception Occurred : " + Convert.ToString(ex.Message);
            }
            return obj;
        }
        public ExchangeRateResponse GetCurrencyDetails(int id)
        {
            ResponseModel obj = new ResponseModel();
            obj.Response = "₹0";
            ExchangeRateResponse objExchangeRateResponse = new ExchangeRateResponse();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param = new SqlParameter("@id", id);
                    objExchangeRateResponse = context.Database.SqlQuery<ExchangeRateResponse>("EXEC proc_getExchangeRate @id", param).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.GetCurrencyDetails", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return objExchangeRateResponse;

        }
        public API_ExchangeRates ExchnageCurrencies()
        {
            API_ExchangeRates obj = new API_ExchangeRates();
            try
            {
                string _key = Convert.ToString(System.Configuration.ConfigurationSettings.AppSettings["ExchangeRateKey"]);
                var apiUrl = "https://v6.exchangerate-api.com/v6/" + _key + "/latest/INR";
                var options = new RestClientOptions(apiUrl);
                RestClient client = new RestClient(options);
                RestRequest request = new RestRequest("", Method.Get);
                RestResponse response = client.Execute(request);
                obj = JsonConvert.DeserializeObject<API_ExchangeRates>(response.Content);
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(response.Content);
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.ExchnageCurrencies", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return obj;
        }
        #endregion

        #region Export
        public async Task<DataSet> ExportPageData(FormDataModel model)
        {
            DataSet dst = new DataSet();

            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    using (var connection = context.Database.Connection)
                    {
                        if (connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "proc_GetDataTableList_Paging";
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add(new SqlParameter("@PageName", model.FormName));
                            command.Parameters.Add(new SqlParameter("@CompanyId", model.CompanyId));
                            command.Parameters.Add(new SqlParameter("@FinancialYearId", model.FinacialYearId));
                            command.Parameters.Add(new SqlParameter("@PageNumber", 1));
                            command.Parameters.Add(new SqlParameter("@PageSize", 10000000));
                            command.Parameters.Add(new SqlParameter("@SearchValue", ""));
                            command.Parameters.Add(new SqlParameter("@UserId", model.UserId));
                            SqlParameter pvNewId = new SqlParameter("@ErrorMsg", SqlDbType.VarChar, 200);
                            pvNewId.Direction = ParameterDirection.Output;
                            command.Parameters.Add(pvNewId);

                            // Fill dataset
                            DbProviderFactory factory = DbProviderFactories.GetFactory(connection);
                            using (var adapter = factory.CreateDataAdapter())
                            {
                                adapter.SelectCommand = command;
                                adapter.Fill(dst);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.ExportPageData", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return dst;
        }

        public async Task<DataSet> ExportPageTabData(FormDataModel model)
        {
            DataSet dst = new DataSet();

            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    using (var connection = context.Database.Connection)
                    {
                        if (connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "proc_GetTabDataTableList";
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter { ParameterName = "@TabId", Value = model.TabId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ParentId", Value = model.ParentId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = model.UserId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@ProcessId", Value = model.ProcessId });
                            command.Parameters.Add(new SqlParameter { ParameterName = "@DetailParentId", Value = 0 });
                            SqlParameter pvNewId = new SqlParameter("@ErrorMsg", SqlDbType.VarChar, 200);
                            pvNewId.Direction = ParameterDirection.Output;
                            command.Parameters.Add(pvNewId);

                            // Fill dataset
                            DbProviderFactory factory = DbProviderFactories.GetFactory(connection);
                            using (var adapter = factory.CreateDataAdapter())
                            {
                                adapter.SelectCommand = command;
                                adapter.Fill(dst);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.ExportPageTabData", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return dst;
        }

        #endregion

        #region Notifications
        public async Task<List<UserNotificationModel>> getUserNotification(string UserId, int record = 5)
        {
            List<UserNotificationModel> notificationModels = new List<UserNotificationModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new List<SqlParameter> { new SqlParameter("@UserId", UserId), new SqlParameter("@Record", record) };
                    notificationModels = context.Database.SqlQuery<UserNotificationModel>("EXEC m_GetUserTop5Notifications @UserId,@Record", parameters.ToArray()).ToList();
                }
            }
            catch (Exception)
            {
                notificationModels = new List<UserNotificationModel>();
            }
            return notificationModels;
        }

        public async Task<UserNotificationModel> GetNotificationDetail(int notificationId, string UserId)
        {
            UserNotificationModel response = new UserNotificationModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {

                    var parameters = new List<SqlParameter> { new SqlParameter("@UserId", UserId), new SqlParameter("@UserNotificationId", notificationId) };
                    response = context.Database.SqlQuery<UserNotificationModel>("EXEC m_GetUserNotification  @UserId, @UserNotificationId", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                response = new UserNotificationModel();
            }
            return response;
        }
        #endregion

        #region Item Advance Settings
        public List<FormItemAdvanceSettings> getFormItemAdvanceSettings()
        {
            List<FormItemAdvanceSettings> result = new List<FormItemAdvanceSettings>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    result = context.Database.SqlQuery<FormItemAdvanceSettings>("EXEC proc_getItemAdvanceSettings").ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return result;
        }
        public FormItemAdvanceSettings getFormItemAdvanceDetail(int ItemAdvanceId)
        {
            FormItemAdvanceSettings result = new FormItemAdvanceSettings();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param = new SqlParameter("@ItemAdvanceId", ItemAdvanceId);
                    result = context.Database.SqlQuery<FormItemAdvanceSettings>("EXEC proc_getItemAdvanceDetail @ItemAdvanceId", param).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return result;
        }
        public List<FormItemAdvanceField> getFormItemAdvanceFields(int ItemAdvanceId)
        {
            List<FormItemAdvanceField> result = new List<FormItemAdvanceField>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param = new SqlParameter("@ItemAdvanceId", ItemAdvanceId);
                    result = context.Database.SqlQuery<FormItemAdvanceField>("EXEC proc_getItemAdvanceFields @ItemAdvanceId", param).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return result;
        }
        public FormItemAdvanceField getFormItemAdvanceFieldDetail(string FieldId)
        {
            FormItemAdvanceField result = new FormItemAdvanceField();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param = new SqlParameter("@FieldId", FieldId);
                    result = context.Database.SqlQuery<FormItemAdvanceField>("EXEC proc_getItemAdvanceFieldDetail @FieldId", param).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return result;
        }

        public ResponseModel SaveFormItemAdvanceSettings(FormItemAdvanceSettings model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ItemAdvanceId", model.ItemAdvanceId),
                        new SqlParameter("@GridName", (object)model.GridName ?? DBNull.Value),
                        new SqlParameter("@GridTitle", (object)model.GridTitle ?? DBNull.Value),
                        new SqlParameter("@Description", (object)model.Description ?? DBNull.Value),
                        new SqlParameter("@SQLTableName", (object)model.SQLTableName ?? DBNull.Value),
                        new SqlParameter("@IsActive", (object)model.IsActive ?? DBNull.Value),
                        new SqlParameter("@UserId", (object)model.UserId ?? DBNull.Value),
                        new SqlParameter("@GetDataScript", (object)model.GetDataScript ?? DBNull.Value),
                        new SqlParameter("@SetDataScript", (object)model.SetDataScript ?? DBNull.Value),
                        new SqlParameter("@ValidDataScript", (object)model.ValidDataScript ?? DBNull.Value)
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_SaveFormItemAdvanceSettings @ItemAdvanceId,@GridName,@GridTitle,@Description,@SQLTableName,@IsActive,@UserId,@GetDataScript,@SetDataScript,@ValidDataScript",
                        parameters.ToArray()).FirstOrDefault();
                    if (response == null)
                    {
                        response = new ResponseModel
                        {
                            Response = "No response returned from procedure."
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog()
                {
                    Logger = "TransactionBusiness.SaveFormItemAdvanceSettings",
                    Exception = ex.ToString(),   // better than StackTrace only
                    Message = ex.Message,
                    LogLevel = "Error"
                });

                response = new ResponseModel
                {
                    Response = "Exception occurred, Please try again."
                };
            }

            return response;
        }
        #endregion
        public ResponseModel DeleteFormItemAdvanceSetting(DeleteFormSetting modal)
        {
            ResponseModel result = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter sqlParameter = new SqlParameter("@PrimaryId", modal.PrimaryId);
                    SqlParameter sqlParameter2 = new SqlParameter("@SQLTableName", modal.Source);
                    result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteFormItemAdvanceField @PrimaryId, @SQLTableName", new object[2] { sqlParameter, sqlParameter2 }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return result;
        }
        public ResponseModel UpdateItemAdvanceFieldSortOrder(List<FormSortOrderUpdateModel> updatedFields)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DataTable SortDataTable = CommonBusiness.GetSortingTableForModel(updatedFields);
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@SortOrderTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_SortOrderData",
                            Value = SortDataTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_UpdateItemAdvanceFieldSortOrder @SortOrderTable", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }
        public ResponseModel SaveFormItemAdvanceField(FormItemAdvanceField model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(@"EXEC dbo.proc_SaveFormItemAdvanceField
                    @FieldId,@ItemAdvanceId,@FieldTypeId,@IsActive,@FieldName,
                    @FieldCaption,@FieldLength,@IsRequired,@DDLSourceType,@DDLSourceName,
                    @DDLTextField,
                    @DDLValueField,
                    @IsReadOnly,
                    @IsVisible,
                    @IsVisibleInList,
                    @FieldSize,
                    @FieldPlaceHolder,
                    @FieldHelpText,
                    @SortOrder,
                    @IsDisable,
                    @FieldFormula,
                    @IsDependencyField,
                    @IsMultiSelection,
                    @IsUnique,
                    @FieldDecimal,
                    @AllowMultiDocument,
                    @IsSummary,
                    @TemplateId",
                  new SqlParameter("@FieldId", (object)model.FieldId ?? DBNull.Value),
                  new SqlParameter("@ItemAdvanceId", model.ItemAdvanceId),
                  new SqlParameter("@FieldTypeId", model.FieldTypeId),
                  new SqlParameter("@IsActive", model.IsActive),
                  new SqlParameter("@FieldName", model.FieldName),
                  new SqlParameter("@FieldCaption", model.FieldCaption),
                  new SqlParameter("@FieldLength", model.FieldLength),
                  new SqlParameter("@IsRequired", model.IsRequired),

                  new SqlParameter("@DDLSourceType", (object)model.DDLSourceType ?? DBNull.Value),
                  new SqlParameter("@DDLSourceName", (object)model.DDLSourceName ?? DBNull.Value),
                  new SqlParameter("@DDLTextField", (object)model.DDLTextField ?? DBNull.Value),
                  new SqlParameter("@DDLValueField", (object)model.DDLValueField ?? DBNull.Value),

                  new SqlParameter("@IsReadOnly", (object)model.IsReadOnly ?? DBNull.Value),
                  new SqlParameter("@IsVisible", (object)model.IsVisible ?? DBNull.Value),
                  new SqlParameter("@IsVisibleInList", (object)model.IsVisibleInList ?? DBNull.Value),
                  new SqlParameter("@FieldSize", (object)model.FieldSize ?? DBNull.Value),
                  new SqlParameter("@FieldPlaceHolder", (object)model.FieldPlaceHolder ?? DBNull.Value),
                  new SqlParameter("@FieldHelpText", (object)model.FieldHelpText ?? DBNull.Value),

                  new SqlParameter("@SortOrder", (object)model.SortOrder ?? DBNull.Value),
                  new SqlParameter("@IsDisable", (object)model.IsDisable ?? DBNull.Value),
                  new SqlParameter("@FieldFormula", (object)model.FieldFormula ?? DBNull.Value),
                  new SqlParameter("@IsDependencyField", (object)model.IsDependencyField ?? DBNull.Value),
                  new SqlParameter("@IsMultiSelection", (object)model.IsMultiSelection ?? DBNull.Value),
                  new SqlParameter("@IsUnique", (object)model.IsUnique ?? DBNull.Value),
                  new SqlParameter("@FieldDecimal", (object)model.FieldDecimal ?? DBNull.Value),
                  new SqlParameter("@AllowMultiDocument", (object)model.AllowMultiDocument ?? DBNull.Value),
                  new SqlParameter("@IsSummary", (object)model.IsSummary ?? DBNull.Value),
                  new SqlParameter("@TemplateId", (object)model.TemplateId ?? DBNull.Value)
                  ).FirstOrDefault();

                    if (response?.IsSuccess == true)
                    {
                        SqlParameter sqlParameter = new SqlParameter("@SQLTableName", model.SQLTableName);
                        SqlParameter sqlParameter2 = new SqlParameter("@FieldName", model.FieldName);
                        SqlParameter sqlParameter3 = new SqlParameter("@FieldType", model.FieldType);
                        SqlParameter sqlParameter4 = new SqlParameter("@FieldLength", model.FieldLength.ToString());
                        SqlParameter sqlParameter5 = new SqlParameter("@IsMultiSelection", model.IsMultiSelection);
                        context.Database.ExecuteSqlCommand("exec proc_AddField @SQLTableName , @FieldName , @FieldType, @FieldLength, @IsMultiSelection", sqlParameter, sqlParameter2, sqlParameter3, sqlParameter4, sqlParameter5);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveFormItemAdvanceField", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occured, Please try again.";
            }
            return response;
        }
        public async Task<List<AuditLogUserNameData>> getUsersForAuditLog(string UserId, int RoleId)
        {
            List<AuditLogUserNameData> userList = new List<AuditLogUserNameData>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new List<SqlParameter> { new SqlParameter("@UserId", UserId), new SqlParameter("@RoleId", RoleId) };
                    userList = context.Database.SqlQuery<AuditLogUserNameData>("EXEC m_getUserList @UserId,@RoleId", parameters.ToArray()).ToList();
                }
            }
            catch (Exception)
            {
                userList = new List<AuditLogUserNameData>();
            }
            return userList;
        }

        public async Task<List<UserAuditLogList>> getUsersAuditLogList(string UserId, string FromDate, string ToDate, string companyId)
        {
            List<UserAuditLogList> userAuditLogList = new List<UserAuditLogList>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new List<SqlParameter> { new SqlParameter("@UserId", UserId), new SqlParameter("@FromDate", FromDate), new SqlParameter("@ToDate", ToDate), new SqlParameter("@CompanyId", companyId) };
                    userAuditLogList = context.Database.SqlQuery<UserAuditLogList>("EXEC m_GetUserAuditLogList @UserId, @FromDate, @ToDate, @CompanyId", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                userAuditLogList = new List<UserAuditLogList>();
            }
            return userAuditLogList;
        }
    }
}