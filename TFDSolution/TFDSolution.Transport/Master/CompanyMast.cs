using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class CompanyData
    {
        public List<CompanyMast> Company { get; set; }
    }
    public class CompanyMast
    {
        public Guid CompanyId { get; set; }
        public string CompanyCode { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Email { get; set; }
        public string WebStite { get; set; }
        public string Fax { get; set; }
        public string RegisterAddress1 { get; set; }
        public string RegisterAddress2 { get; set; }
        public Nullable<int> RegiterCityId { get; set; }
        public Nullable<int> RegiterStateId { get; set; }
        public Nullable<int> RegisterCountryId { get; set; }
        public string RegisterPincode { get; set; }
        public string FactoryAddress1 { get; set; }
        public string FactoryAddress2 { get; set; }
        public Nullable<int> FactoryCityId { get; set; }
        public Nullable<int> FactoryStateId { get; set; }
        public Nullable<int> FactoryCountryId { get; set; }
        public string FactoryPincode { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }
        public string CINNo { get; set; }
        public string ECCNo { get; set; }
        public string IECCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<DateTime> UpdatedOn { get; set; }
        public Guid UserId { get; set; }
        public string PageAction { get; set; }
        public int? DefaultCurrencyId { get; set; }
        public string FromEmail { get; set; }
        public string FromPassword { get; set; }
        public string DisplayName { get; set; }
        public string SmtpServer { get; set; }
        public int? SmtpPort { get; set; }
        public bool EnableSSL { get; set; }
        public List<CompanyFinancial> Financials { get; set; }
        public string RegistrationNo { get; set; }
        public string MSMENo { get; set; }
        public string TANNo { get; set; }
        public string PFNo { get; set; }
        public string ESICNo { get; set; }
        public string PrefessionTaxNo { get; set; }
        public string LWFPrefessionNo { get; set; }
        public Nullable<DateTime> RegistrationDate { get; set; }
    }
    public class CompanyBank
    {
        public Guid BankId { get; set; }
        public Guid CompanyId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress1 { get; set; }
        public string BranchAddress2 { get; set; }
        public int CityId { get; set; }
        public string AccountNo { get; set; }
        public string IFSCCode { get; set; }
        public string SwiftCde { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Nullable<Guid> UpdatedBy { get; set; }
        public Nullable<DateTime> UpdatedOn { get; set; }

    }
    public class CompanyUnit
    {
        public Guid CompanyUnitId { get; set; }
        public string SiteName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string Pincode { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Nullable<Guid> UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public Guid CompanyId { get; set; }
    }

    public class CompanyFinancial
    {
        public int FiancialYearId { get; set; }
        public Guid CompanyId { get; set; }
        public string Alias { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Guid UserId { get; set; }
    }
}
