using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class OtherChargesModel
    {
        public string TaxSlab { get; set; }
        public string ChargesType { get; set; }
        public int ChargesId { get; set; }
        public string FormId { get; set; }
        public string MultipleFormId { get; set; }
        public string Description { get; set; }
        public string ShortName { get; set; }
        public int LedgerId { get; set; }
        public int TaxSlabId { get; set; }
        public int ChargesTypeId { get; set; }
        public int Status { get; set; } = 1;
        public string CompanyId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Nullable<DateTime> UpdatedOn { get; set; }
        public Guid UserId { get; set; }
        public List<OtherChargesDetail> Details { get; set; }
        public string LedgerIdName { get; set; }
    }

    public class OtherChargesDetail
    {
        public int ChargesId { get; set; }
        public int OtherChargesDetailId { get; set; }
        public int SrNo { get; set; }
        public string Alias { get; set; }
        public string TaxChargeName { get; set; }
        public int LedgerId { get; set; }        
        public string PerAmount { get; set; }
        public string BasedOn { get; set; }
        public double ChargesValue { get; set; }
        public Nullable<int> DecimalPoint { get; set; } = 0;
        public string Formula { get; set; }
        public string Effect { get; set; }
        public bool ChangeValue { get; set; }
        public bool AddInCost { get; set; }
        public Guid UserId { get; set; }
        public bool? IsDisplayOnItemsPopup { get; set; }

        public double StoredAmount { get; set; }
        public double StoredPer { get; set; }
        public string LedgerIdName { get; set; }
    }
}
