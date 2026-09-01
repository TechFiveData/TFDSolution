using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class PartyMst
    {

        public Guid PartyId { get; set; }
        public string PartyCode { get; set; }

        [Required]
        public int PartyType { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string PartyName { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string AlternateName { get; set; }

        [Required]
        public int PartyGroup { get; set; }

        [Required]
        public int Currency { get; set; }

        [RegularExpression(@"^[0-9]{7,15}$", ErrorMessage = "Invalid phone number (7-15 digits only).")]
        public string Telephone1 { get; set; }

        [RegularExpression(@"^[0-9]{7,15}$", ErrorMessage = "Invalid phone number (7-15 digits only).")]
        public string Telephone2 { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(10, ErrorMessage = "Length cannot exceed 10 digits.")]
        public string Mobile { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string Email { get; set; }

        [Url(ErrorMessage = "Invalid website URL (e.g., https://example.com).")]
        [MaxLength(100, ErrorMessage = "Length cannot exceed 100 Character.")]
        public string WebSite { get; set; }

        [RegularExpression(@"^[0-9]{7,15}$", ErrorMessage = "Invalid Fax number (7-15 digits only).")]
        public string Fax { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string ContactPerson { get; set; }
        public int? Industry { get; set; }
        [Required]
        public bool PartyStatus { get; set; } // 1 for Active, 0 for Inactive
        public int? SalesPerson { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 250 Character.")]
        public string Remarks { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string BillToName { get; set; }

        [MaxLength(200, ErrorMessage = "Length cannot exceed 200 Character.")]
        public string BillToAddress1 { get; set; }
        [MaxLength(200, ErrorMessage = "Length cannot exceed 200 Character.")]
        public string BillToAddress2 { get; set; }
        public int? BillToCity { get; set; }
        public int? BillToState { get; set; }
        public int? BillToCountry { get; set; }

        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Invalid PIN Code.")]
        [MaxLength(6, ErrorMessage = "Length cannot exceed 6 digits.")]
        public string BillToPincode { get; set; }

        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(10, ErrorMessage = "Length cannot exceed 10 digits.")]
        public string BillToContactNo { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string BillToContactPerson { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",ErrorMessage = "Invalid email format.")]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string BillToEmail { get; set; }

        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string ShipToName { get; set; }
        [MaxLength(200, ErrorMessage = "Length cannot exceed 200 Character.")]
        public string ShipToAddress1 { get; set; }
        [MaxLength(200, ErrorMessage = "Length cannot exceed 200 Character.")]
        public string ShipToAddress2 { get; set; }
        public int? ShipToCity { get; set; }
        public int? ShipToState { get; set; }
        public int? ShipToCountry { get; set; }

        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Invalid PIN Code.")]
        [MaxLength(6, ErrorMessage = "Length cannot exceed 6 digits.")]
        public string ShipToPincode { get; set; }

        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(10, ErrorMessage = "Length cannot exceed 10 digits.")]
        public string ShipToContactNo { get; set; }

        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 Character.")]
        [RegularExpression(@"^[A-Za-z\s]{2,50}$", ErrorMessage = "Only alphabets and spaces allowed.")]
        public string ShipToContactPerson { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string ShipToEmail { get; set; }
        [MaxLength(20, ErrorMessage = "GST number cannot exceed 20 digits.")]
        public string GSTNo { get; set; }
        public int? GSTType { get; set; }

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format.")]
        [MaxLength(10, ErrorMessage = "Length cannot exceed 10 digits.")]
        public string PANNo { get; set; }

        public String CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public String UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public Guid UserId { get; set; }

    }
}
