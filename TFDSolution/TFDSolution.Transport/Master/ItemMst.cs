using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TFDSolution.Transport.Master
{
    public class ItemMast
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; }
        [Required]
        public string ItemDescription { get; set; }
        public string AlternateDescription { get; set; }
        [Required]
        public int ItemGroup { get; set; }
        [Required]
        public int ItemUOM { get; set; }
        [Required]
        public int ItemCategory { get; set; }
        [Required]
        public int ItemType { get; set; }
        [Required]
        public int PackingType { get; set; }
        public bool ItemStatus { get; set; }
        public int? PurchaseUOM { get; set; }
        public int? ItemPerPurchase { get; set; }
        public decimal? PurchaseTaxPercentage { get; set; }
        public int? PurchaseHSNCode { get; set; }
        public int? SalesUOM { get; set; }
        public int? ItemPerSales { get; set; }
        public decimal? SalesTaxPercentage { get; set; }
        public int? SalesHSNCode { get; set; }
        public decimal? ItemWeight { get; set; }
        public decimal? MinimumQty { get; set; }
        public decimal? MaximumQty { get; set; }
        public int? TaxSlabId { get; set; }
        public decimal? ReorderQty { get; set; }
        public int? ValuationMethod { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid UserId { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>(); // List of attachments

        public AttachmentViewModel NewAttachment { get; set; } = new AttachmentViewModel(); // For file upload

    }

    public class ItemWarehouse
    {
        public Guid WarehouseId { get; set; }
        public Guid ItemId { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal StockValue { get; set; }
        public String CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public String UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SpecificationRequest
    {
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int ItemId { get; set; }
        public string FormId { get; set; }
        public string UserId { get; set; }
        public string CompanyId { get; set; }
        public int FiancialYearId { get; set; }
        public List<SpecificationField> SpecificationFields { get; set; }
    }
    public class SpecificationField
    {
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public string FieldValue { get; set; }
        public string FieldRemarks { get; set; }
    }


    public class ItemSpeficationModel
    {
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int ItemId { get; set; }
        public List<FormField> Fields { get; set; }
        public List<SpecificationField> Data { get; set; }
    }
    public class ItemDetailButtonRequest
    {
        public string FieldId { get; set; }
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int Counter { get; set; }
        public int ItemSrNo { get; set; }
        public string FormId { get; set; }
        public string CompanyId { get; set; }
        public string TabId { get; set; }
        public string UserId { get; set; }
        public string PageAction { get; set; }
        public int ReferenceFieldCounter { get; set; }
        public List<PageFieldData> FieldData { get; set; }
        public int TemplateId { get; set; }
    }
    public class ItemDetailButtonModel
    {
        public int DetailId { get; set; }
        public string FormId { get; set; }
        public int ItemSrNo { get; set; }
        public int ParentId { get; set; }
        public string FieldId { get; set; }
        public string ButtonCaption { get; set; }
        public string PageAction { get; set; }
        public FormTabData Data { get; set; }
        public List<PageFieldData> FieldData { get; set; }
        public string UserId { get; set; }
        public int TemplateId { get; set; }
    }

    public class SubmitItemDetailButton
    {
        public int DetailId { get; set; }
        public int ParentId { get; set; }
        public List<ReadingItem> FieldData { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public Nullable<System.Guid> CompanyId { get; set; }
        public int FiancialYearId { get; set; }
        public string PageType { get; set; }
        public string FormId { get; set; }
        public string TabId { get; set; }        
        public string URNNo { get; set; }
        public int ItemSrNo { get; set; }
        public int TemplateId { get; set; }
        public List<ButtonDataItem> ButtonFieldData { get; set; }

    }

    public class ButtonDataItem
    {
        public int SrNo { get; set; }
        public List<PageFieldData> ItemRowData { get; set; }
    }
    public class ReadingItem
    {
        public int SrNo { get; set; }
        public string RequireDim { get; set; }
        public string LowerLimit { get; set; }
        public string UpperLimit { get; set; }
        public string Parameter { get; set; }
        public string Reading { get; set; }
    }
    public class LanguageFieldModel
    {
        public string FieldName { get; set; }
        public string LanguageCode { get; set; }
    }
}
