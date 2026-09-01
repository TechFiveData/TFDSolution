using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Integration;

namespace TFDSolution.Business
{
    public class IntegrationBusiness : IIntegrationBusiness
    {
        private readonly IMasterBusiness masterBusines;
        public IntegrationBusiness()
        {
            masterBusines = new MasterBusiness();
        }

        public IntegrationConfiguration GetIntegrationConfiguration(string companyId)
        {
            IntegrationConfiguration result = new IntegrationConfiguration();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string sql = @"SELECT CompanyName,GSTNo AS GSTIN,GSPUserName AS EWayBill_EWBUsername,GSPPassword AS EWayBill_EWBPassword FROM m_CompanyMast WHERE CompanyId = @CompanyId";
                    var companyIdParam = new SqlParameter("@CompanyId", companyId);
                    result = context.Database.SqlQuery<IntegrationConfiguration>(sql, companyIdParam).FirstOrDefault() ?? new IntegrationConfiguration();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public EWayBillRequest GetEWayBillRequestData(string formId, int parentId, string companyId, string userId, int financialYearId, string storedProcedure)
        {
            EWayBillRequest response = new EWayBillRequest();
            response.Items = new List<EWayBillItem>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", companyId));
                        cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                        cmd.Parameters.Add(new SqlParameter("@FinancialYearId", financialYearId));
                        cmd.Parameters.Add(new SqlParameter("@FormId", formId));
                        cmd.Parameters.Add(new SqlParameter("@ParentId", parentId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            // Header
                            if (reader.Read())
                            {
                                response.Header = new EWayBillHeader
                                {
                                    supplyType = reader["supplyType"].ToString(),
                                    subSupplyType = reader["subSupplyType"].ToString(),
                                    subSupplyDesc = reader["subSupplyDesc"].ToString(),
                                    docType = reader["docType"].ToString(),
                                    docNo = reader["docNo"].ToString(),
                                    docDate = reader["docDate"].ToString(),
                                    fromGstin = reader["fromGstin"].ToString(),
                                    fromTrdName = reader["fromTrdName"].ToString(),
                                    fromAddr1 = reader["fromAddr1"].ToString(),
                                    fromAddr2 = reader["fromAddr2"].ToString(),
                                    fromPlace = reader["fromPlace"].ToString(),
                                    fromPincode = reader["fromPincode"].ToString(),
                                    fromStateCode = reader["fromStateCode"].ToString(),
                                    actFromStateCode = reader["actFromStateCode"].ToString(),
                                    toGstin = reader["toGstin"].ToString(),
                                    toTrdName = reader["toTrdName"].ToString(),
                                    toAddr1 = reader["toAddr1"].ToString(),
                                    toAddr2 = reader["toAddr2"].ToString(),
                                    actToStateCode = reader["actToStateCode"].ToString(),
                                    cessNonAdvolValue = Convert.ToDecimal(reader["cessNonAdvolValue"]),
                                    cessValue = Convert.ToDecimal(reader["cessValue"]),
                                    shipToGSTIN = reader["shipToGSTIN"].ToString(),
                                    shipToTradeName = reader["shipToTradeName"].ToString(),
                                    toPincode = reader["toPincode"].ToString(),
                                    toPlace = reader["toPlace"].ToString(),
                                    toStateCode = reader["toStateCode"].ToString(),
                                    transactionType = Convert.ToInt32(reader["transactionType"]),
                                    otherValue = Convert.ToDecimal(reader["otherValue"]),
                                    totalValue = Convert.ToDecimal(reader["totalValue"]),
                                    cgstValue = Convert.ToDecimal(reader["cgstValue"]),
                                    sgstValue = Convert.ToDecimal(reader["sgstValue"]),
                                    igstValue = Convert.ToDecimal(reader["igstValue"]),
                                    totInvValue = Convert.ToDecimal(reader["totInvValue"]),

                                    transporterId = reader["transporterId"].ToString(),
                                    transporterName = reader["transporterName"].ToString(),

                                    vehicleNo = reader["vehicleNo"].ToString(),
                                    vehicleType = reader["vehicleType"].ToString(),

                                    transDocNo = reader["transDocNo"].ToString(),
                                    transDocDate = reader["transDocDate"].ToString(),
                                    transDistance = Convert.ToInt32(reader["transDistance"]),
                                    transMode = Convert.ToInt32(reader["transMode"])
                                };
                            }

                            // Move to second result set
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    response.Items.Add(new EWayBillItem
                                    {
                                        productName = reader["productName"].ToString(),
                                        productDesc = reader["productDesc"].ToString(),
                                        hsnCode = reader["hsnCode"].ToString(),

                                        quantity = Convert.ToDecimal(reader["quantity"]),
                                        qtyUnit = reader["qtyUnit"].ToString(),

                                        cgstRate = Convert.ToDecimal(reader["cgstRate"]),
                                        sgstRate = Convert.ToDecimal(reader["sgstRate"]),
                                        igstRate = Convert.ToDecimal(reader["igstRate"]),

                                        cessRate = Convert.ToDecimal(reader["cessRate"]),
                                        cessNonadvol = Convert.ToDecimal(reader["cessNonadvol"]),

                                        taxableAmount = Convert.ToDecimal(reader["taxableAmount"])
                                    });
                                }
                            }
                        }
                    }
                }
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            return response;
        }
        
        public bool UpdateEWayBillCancel(string sqlTableName, int parentID, string EWayBillCancelDate, string EWayCreatedBy)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string sql = $@" UPDATE {sqlTableName} SET
                    EWayBillCancelledDate = @EWayBillCancelledDate,
                    EWayBillCancelledBy = @EWayBillCancelledBy
                    WHERE Id = @ParentID";
                    int rowsAffected = context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@EWayBillCancelledDate", EWayBillCancelDate),
                        new SqlParameter("@EWayBillCancelledBy", (object)EWayCreatedBy ?? DBNull.Value),
                        new SqlParameter("@ParentID", parentID));

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return false;
            }
        }
        public bool UpdateEWayBillData(string sqlTableName, int parentID, string EWayBillNumber, string EWayBillDate, string EWayValidUpto, string EWayCreatedBy)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string sql = $@" UPDATE {sqlTableName} SET
                    EWayBillNumber = @EWayBillNumber,
                    EWayBillDate = @EWayBillDate,
                    EWayValidUpto = @EWayValidUpto,
                    EWayCreatedBy = @EWayCreatedBy,
                    EWayCreatedOn = @EWayCreatedOn WHERE Id = @ParentID";
                    int rowsAffected = context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@EWayBillNumber", EWayBillNumber),
                        new SqlParameter("@EWayBillDate", EWayBillDate),
                        new SqlParameter("@EWayValidUpto", (object)EWayValidUpto ?? DBNull.Value),
                        new SqlParameter("@EWayCreatedBy", (object)EWayCreatedBy ?? DBNull.Value),
                        new SqlParameter("@EWayCreatedOn", DateTime.Now),
                        new SqlParameter("@ParentID", parentID));

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return false;
            }
        }

        public bool UpdateEInvoiceData(string sqlTableName, int parentID, string eInvoiceAckNo, string eInvoiceAckDate, string eInvoiceIRN, string eInvoiceStatus, string eInvoiceCreatedBy)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string sql = $@" UPDATE {sqlTableName} SET
                    eInvoiceAckNo = @eInvoiceAckNo,
                    eInvoiceAckDate = @eInvoiceAckDate,
                    eInvoiceIRN = @eInvoiceIRN,
                    eInvoiceStatus = @eInvoiceStatus,
                    eInvoiceCreatedBy = @eInvoiceCreatedBy,
                    eInvoiceCreatedOn = @eInvoiceCreatedOn
                    WHERE Id = @ParentID";
                    int rowsAffected = context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@eInvoiceAckNo", eInvoiceAckNo),
                        new SqlParameter("@eInvoiceAckDate", eInvoiceAckDate),
                        new SqlParameter("@eInvoiceIRN", (object)eInvoiceIRN ?? DBNull.Value),
                        new SqlParameter("@eInvoiceStatus", (object)eInvoiceStatus ?? DBNull.Value),
                        new SqlParameter("@eInvoiceCreatedBy", (object)eInvoiceCreatedBy ?? DBNull.Value),
                        new SqlParameter("@eInvoiceCreatedOn", DateTime.Now),
                        new SqlParameter("@ParentID", parentID));

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return false;
            }
        }
        public void InsertIntegrationLogs(IntegrationAPILog model)
        {
            try
            {
                // string swl = "INSERT INTO dbo.tbl_EWB_APILog\r\n(\r\n    FormId,\r\n    CreatedBy,\r\n    ParentId,\r\n    EWBId,\r\n    ApiName,\r\n    RequestJson,\r\n    ResponseJson,\r\n    HttpStatus,\r\n    Success,\r\n    ErrorCode,\r\n    ErrorMessage\r\n)\r\nVALUES\r\n(\r\n    'FORM001',\r\n    'USER001',\r\n    1001,\r\n    123456789012,\r\n    'GenerateEWayBill',\r\n    N'{\"supplyType\":\"O\",\"docNo\":\"INV001\"}',\r\n    N'{\"success\":true,\"ewayBillNo\":\"123456789012\"}',\r\n    200,\r\n    1,\r\n    NULL,\r\n    NULL\r\n);"
                using (var context = new TFDSolutionEntities())
                {
                    string sql = @"INSERT INTO dbo.tbl_Integration_APILog (FormId,CreatedBy,ParentId,IntegrationType,Reference,ApiName,RequestJson,ResponseJson,HttpStatus,Success,ErrorCode,ErrorMessage,CreatedOn)
                        VALUES
                        (
                            @FormId,
                            @CreatedBy,
                            @ParentId,
                            @IntegrationType,
                            @Reference,
                            @ApiName,
                            @RequestJson,
                            @ResponseJson,
                            @HttpStatus,
                            @Success,
                            @ErrorCode,
                            @ErrorMessage,
                            GETDATE()
                        )";
                    context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@FormId", (object)model.FormId ?? DBNull.Value),
                        new SqlParameter("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value),
                        new SqlParameter("@ParentId", model.ParentId),
                        new SqlParameter("@IntegrationType", (object)model.IntegrationType ?? DBNull.Value),
                        new SqlParameter("@Reference", (object)model.Reference ?? DBNull.Value),
                        new SqlParameter("@ApiName", (object)model.ApiName ?? DBNull.Value),
                        new SqlParameter("@RequestJson", (object)model.RequestJson ?? DBNull.Value),
                        new SqlParameter("@ResponseJson", (object)model.ResponseJson ?? DBNull.Value),
                        new SqlParameter("@HttpStatus", (object)model.HttpStatus ?? DBNull.Value),
                        new SqlParameter("@Success", model.Success),
                        new SqlParameter("@ErrorCode", (object)model.ErrorCode ?? DBNull.Value),
                        new SqlParameter("@ErrorMessage", (object)model.ErrorMessage ?? DBNull.Value)
                    );
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }

        public void InserteWayBillData(IntegrationAPILog model)
        {
            try
            {
                // string swl = "INSERT INTO dbo.tbl_EWB_APILog\r\n(\r\n    FormId,\r\n    CreatedBy,\r\n    ParentId,\r\n    EWBId,\r\n    ApiName,\r\n    RequestJson,\r\n    ResponseJson,\r\n    HttpStatus,\r\n    Success,\r\n    ErrorCode,\r\n    ErrorMessage\r\n)\r\nVALUES\r\n(\r\n    'FORM001',\r\n    'USER001',\r\n    1001,\r\n    123456789012,\r\n    'GenerateEWayBill',\r\n    N'{\"supplyType\":\"O\",\"docNo\":\"INV001\"}',\r\n    N'{\"success\":true,\"ewayBillNo\":\"123456789012\"}',\r\n    200,\r\n    1,\r\n    NULL,\r\n    NULL\r\n);"
                using (var context = new TFDSolutionEntities())
                {
                    string sql = @"INSERT INTO dbo.tbl_Integration_APILog (FormId,CreatedBy,ParentId,IntegrationType,Reference,ApiName,RequestJson,ResponseJson,HttpStatus,Success,ErrorCode,ErrorMessage,CreatedOn)
                        VALUES
                        (
                            @FormId,
                            @CreatedBy,
                            @ParentId,
                            @IntegrationType,
                            @Reference,
                            @ApiName,
                            @RequestJson,
                            @ResponseJson,
                            @HttpStatus,
                            @Success,
                            @ErrorCode,
                            @ErrorMessage,
                            GETDATE()
                        )";
                    context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@FormId", (object)model.FormId ?? DBNull.Value),
                        new SqlParameter("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value),
                        new SqlParameter("@ParentId", model.ParentId),
                        new SqlParameter("@IntegrationType", (object)model.IntegrationType ?? DBNull.Value),
                        new SqlParameter("@Reference", (object)model.Reference ?? DBNull.Value),
                        new SqlParameter("@ApiName", (object)model.ApiName ?? DBNull.Value),
                        new SqlParameter("@RequestJson", (object)model.RequestJson ?? DBNull.Value),
                        new SqlParameter("@ResponseJson", (object)model.ResponseJson ?? DBNull.Value),
                        new SqlParameter("@HttpStatus", (object)model.HttpStatus ?? DBNull.Value),
                        new SqlParameter("@Success", model.Success),
                        new SqlParameter("@ErrorCode", (object)model.ErrorCode ?? DBNull.Value),
                        new SqlParameter("@ErrorMessage", (object)model.ErrorMessage ?? DBNull.Value)
                    );
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }

        #region e-Invoice
        public EInvoiceRequest GetEInvoiceRequestData(string formId, int parentId, string companyId, string userId, int financialYearId, string storedProcedure)
        {
            EInvoiceRequest response = new EInvoiceRequest();
            response.Version = "1.1";
            response.ValDtls = new ValDtls();
            response.ItemList = new List<EInvoiceItem>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = storedProcedure;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", companyId));
                        cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                        cmd.Parameters.Add(new SqlParameter("@FinancialYearId", financialYearId));
                        cmd.Parameters.Add(new SqlParameter("@FormId", formId));
                        cmd.Parameters.Add(new SqlParameter("@ParentId", parentId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            // Result Set 1 - Header
                            if (reader.Read())
                            {
                                response.DocDtls = new DocDtls
                                {
                                    Dt = Convert.ToDateTime(reader["InvoiceDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    No = Convert.ToString(reader["InvoiceNo"]),
                                    Typ = Convert.ToString(reader["DocumentType"]),
                                };

                                response.TranDtls = new TranDtls
                                {
                                    //EcmGstin = Convert.ToString(reader["EcmGstin"]),
                                    IgstOnIntra = Convert.ToString(reader["IGSTOnIntra"]),
                                    RegRev = Convert.ToString(reader["ReverseCharge"]),
                                    SupTyp = Convert.ToString(reader["SupplyType"]),
                                    TaxSch = Convert.ToString(reader["TaxSch"]),
                                };

                                //response.DocDtls.InvoiceNo = reader["InvoiceNo"].ToString();
                                //response.Header.InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]);
                                //response.Header.DocumentType = reader["DocumentType"].ToString();
                                //response.Header.SupplyType = reader["SupplyType"].ToString();
                                //response.Header.VehicleNo = reader["VehicleNo"].ToString();
                            }

                            // Result Set 2 - Seller
                            if (reader.NextResult() && reader.Read())
                            {
                                response.SellerDtls = new SellerDtls
                                {
                                    Gstin = reader["GSTIN"].ToString(),
                                    LglNm = reader["LegalName"].ToString(),
                                    TrdNm = reader["TradeName"].ToString(),
                                    Addr1 = reader["Address"].ToString(),
                                    Addr2 = reader["Address2"].ToString(),
                                    Loc = reader["Location"].ToString(),
                                    Pin = Convert.ToInt32(reader["Pincode"]),
                                    Stcd = reader["StateCode"].ToString(),
                                    Ph = reader["Phone"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["Phone"].ToString()) ? null : reader["Phone"].ToString(),
                                    Em = reader["Email"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["Email"].ToString()) ? null : reader["Email"].ToString()
                                };
                            }

                            // Result Set 3 - Buyer
                            if (reader.NextResult() && reader.Read())
                            {
                                response.BuyerDtls = new BuyerDtls
                                {
                                    Pos = Convert.ToString(reader["POS"]),
                                    Gstin = Convert.ToString(reader["GSTIN"]),
                                    LglNm = Convert.ToString(reader["LegalName"]),
                                    TrdNm = Convert.ToString(reader["TradeName"]),
                                    Addr1 = Convert.ToString(reader["Address1"]),
                                    Addr2 = Convert.ToString(reader["Address2"]),
                                    Loc = Convert.ToString(reader["City"]),
                                    Pin = Convert.ToInt32(reader["Pincode"]),
                                    Stcd = Convert.ToString(reader["StateCode"]),
                                    Ph = reader["Phone"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["Phone"].ToString()) ? null : reader["Phone"].ToString(),
                                    Em = reader["Email"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["Email"].ToString()) ? null : reader["Email"].ToString()
                                };

                            }
                            // Result Set 4 - ShipDtls (Optional)
                            if (reader.NextResult() && reader.Read())
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["GSTIN"])))
                                {
                                    response.ShipDtls = new ShipDtls
                                    {
                                        Gstin = Convert.ToString(reader["GSTIN"]),
                                        LglNm = Convert.ToString(reader["LegalName"]),
                                        TrdNm = Convert.ToString(reader["TradeName"]),
                                        Addr1 = Convert.ToString(reader["Address1"]),
                                        Addr2 = Convert.ToString(reader["Address2"]),
                                        Loc = Convert.ToString(reader["City"]),
                                        Pin = Convert.ToInt32(reader["Pincode"]),
                                        Stcd = Convert.ToString(reader["StateCode"]),
                                    };
                                }
                            }

                            // Result Set 4 - Invoice Values
                            if (reader.NextResult() && reader.Read())
                            {
                                response.ValDtls = new ValDtls
                                {
                                    AssVal = Convert.ToDecimal(reader["AssVal"]),
                                    CgstVal = Convert.ToDecimal(reader["CgstVal"]),
                                    SgstVal = Convert.ToDecimal(reader["SgstVal"]),
                                    IgstVal = Convert.ToDecimal(reader["IgstVal"]),
                                    TotInvVal = Convert.ToDecimal(reader["TotInvVal"]),
                                    CesVal = Convert.ToDecimal(reader["CesVal"]),
                                    Discount = Convert.ToDecimal(reader["Discount"]),
                                    OthChrg = Convert.ToDecimal(reader["OthChrg"]),
                                    RndOffAmt = Convert.ToDecimal(reader["RndOffAmt"]),
                                    StCesVal = Convert.ToDecimal(reader["StCesVal"]),
                                    TotInvValFc = Convert.ToDecimal(reader["TotInvValFc"]),
                                };
                            }

                            // Result Set 5 - Items
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    var item = new EInvoiceItem();
                                    item.IsServc = Convert.ToString(reader["IsService"]);
                                    item.SlNo = Convert.ToString(reader["SlNo"]);
                                    item.PrdDesc = Convert.ToString(reader["ItemDescription"]);
                                    item.HsnCd = Convert.ToString(reader["HSNCode"]);
                                    item.FreeQty = Convert.ToDecimal(reader["FreeQty"]);
                                    item.Qty = Convert.ToDecimal(reader["Quantity"]);
                                    item.Unit = Convert.ToString(reader["Unit"]);
                                    item.UnitPrice = Convert.ToDecimal(reader["UnitPrice"]);
                                    item.TotAmt = (Convert.ToDecimal(reader["TotAmt"]) - Convert.ToDecimal(reader["Discount"]));
                                    item.GstRt = Convert.ToDecimal(reader["GSTRate"]);
                                    item.CgstAmt = Convert.ToDecimal(reader["CGSTAmount"]);
                                    item.SgstAmt = Convert.ToDecimal(reader["SGSTAmount"]);
                                    item.IgstAmt = Convert.ToDecimal(reader["IGSTAmount"]);
                                    item.AssAmt = Convert.ToDecimal(reader["AssAmt"]);
                                    item.CesAmt = Convert.ToDecimal(reader["CessAmount"]);
                                    item.CesRt = Convert.ToDecimal(reader["CessRate"]);
                                    item.Discount = Convert.ToDecimal(reader["Discount"]);
                                    item.StateCesRt = Convert.ToDecimal(reader["StateCessRate"]);
                                    item.StateCesAmt = Convert.ToDecimal(reader["StateCessAmount"]);
                                    item.OthChrg = Convert.ToDecimal(reader["OtherCharges"]);
                                    item.TotItemVal = Convert.ToDecimal(reader["TotalAmount"]);
                                    response.ItemList.Add(item);
                                }
                            }
                        }
                    }
                }
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            return response;
        }

        public bool UpdateEInvoiceCancel(string sqlTableName, int parentID, string EInvoiceCancelDate, string EInvoiceCreatedBy)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string sql = $@" UPDATE {sqlTableName} SET
                    eInvoiceCancelledDate = @EWayBillCancelledDate,
                    eInvoiceCancelledBy = @EWayBillCancelledBy
                    WHERE Id = @ParentID";
                    int rowsAffected = context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@EWayBillCancelledDate", EInvoiceCancelDate),
                        new SqlParameter("@EWayBillCancelledBy", (object)EInvoiceCreatedBy ?? DBNull.Value),
                        new SqlParameter("@ParentID", parentID));

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return false;
            }
        }
        #endregion

    }
}