using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Master;

namespace TFDSolution.Transport.Integration
{
    public class EInvoiceRequest
    {
        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("TranDtls")]
        public TranDtls TranDtls { get; set; }

        [JsonProperty("DocDtls")]
        public DocDtls DocDtls { get; set; }

        [JsonProperty("SellerDtls")]
        public SellerDtls SellerDtls { get; set; }

        [JsonProperty("BuyerDtls")]
        public BuyerDtls BuyerDtls { get; set; }

        [JsonProperty("DispDtls")]
        public DispDtls DispDtls { get; set; }

        [JsonProperty("ShipDtls")]
        public ShipDtls ShipDtls { get; set; }

        [JsonProperty("ItemList")]
        public List<EInvoiceItem> ItemList { get; set; }

        [JsonProperty("ValDtls")]
        public ValDtls ValDtls { get; set; }

        public bool IsSuccess { get; set; }
        public string Response { get; set; }
    }

    public class EInvoiceItem
    {
        [JsonProperty("SlNo")]
        public string SlNo { get; set; }


        [JsonProperty("PrdDesc")]
        public string PrdDesc { get; set; }


        [JsonProperty("IsServc")]
        public string IsServc { get; set; }


        [JsonProperty("HsnCd")]
        public string HsnCd { get; set; }


        [JsonProperty("BarCode")]
        public string BarCode { get; set; }


        [JsonProperty("Qty")]
        public decimal Qty { get; set; }


        [JsonProperty("FreeQty")]
        public decimal FreeQty { get; set; }


        [JsonProperty("Unit")]
        public string Unit { get; set; }


        [JsonProperty("UnitPrice")]
        public decimal UnitPrice { get; set; }


        [JsonProperty("TotAmt")]
        public decimal TotAmt { get; set; }


        [JsonProperty("Discount")]
        public decimal Discount { get; set; }


        [JsonProperty("PreTaxVal")]
        public decimal PreTaxVal { get; set; }


        [JsonProperty("AssAmt")]
        public decimal AssAmt { get; set; }


        [JsonProperty("GstRt")]
        public decimal GstRt { get; set; }


        [JsonProperty("IgstAmt")]
        public decimal IgstAmt { get; set; }


        [JsonProperty("CgstAmt")]
        public decimal CgstAmt { get; set; }


        [JsonProperty("SgstAmt")]
        public decimal SgstAmt { get; set; }


        [JsonProperty("CesRt")]
        public decimal CesRt { get; set; }


        [JsonProperty("CesAmt")]
        public decimal CesAmt { get; set; }


        [JsonProperty("CesNonAdvlAmt")]
        public decimal CesNonAdvlAmt { get; set; }


        [JsonProperty("StateCesRt")]
        public decimal StateCesRt { get; set; }


        [JsonProperty("StateCesAmt")]
        public decimal StateCesAmt { get; set; }


        [JsonProperty("StateCesNonAdvlAmt")]
        public decimal StateCesNonAdvlAmt { get; set; }


        [JsonProperty("OthChrg")]
        public decimal OthChrg { get; set; }


        [JsonProperty("OrdLineRef")]
        public string OrdLineRef { get; set; }


        [JsonProperty("OrgCntry")]
        public string OrgCntry { get; set; }


        [JsonProperty("PrdSlNo")]
        public string PrdSlNo { get; set; }


        [JsonProperty("BchDtls")]
        public BatchDtls BchDtls { get; set; }


        [JsonProperty("AttribDtls")]
        public List<AttribDtls> AttribDtls { get; set; }


        [JsonProperty("TotItemVal")]
        public decimal TotItemVal { get; set; }
    }
    public class AttribDtls
    {

        [JsonProperty("Nm")]
        public string Nm { get; set; }


        [JsonProperty("Val")]
        public string Val { get; set; }

    }
    public class BatchDtls
    {

        [JsonProperty("Nm")]
        public string Nm { get; set; }


        [JsonProperty("ExpDt")]
        public string ExpDt { get; set; }


        [JsonProperty("WrDt")]
        public string WrDt { get; set; }

    }
    public class ValDtls
    {
        [JsonProperty("AssVal")]
        public decimal AssVal { get; set; }


        [JsonProperty("CgstVal")]
        public decimal CgstVal { get; set; }


        [JsonProperty("SgstVal")]
        public decimal SgstVal { get; set; }


        [JsonProperty("IgstVal")]
        public decimal IgstVal { get; set; }


        [JsonProperty("CesVal")]
        public decimal CesVal { get; set; }


        [JsonProperty("StCesVal")]
        public decimal StCesVal { get; set; }


        [JsonProperty("Discount")]
        public decimal Discount { get; set; }


        [JsonProperty("OthChrg")]
        public decimal OthChrg { get; set; }


        [JsonProperty("RndOffAmt")]
        public decimal RndOffAmt { get; set; }


        [JsonProperty("TotInvVal")]
        public decimal TotInvVal { get; set; }


        [JsonProperty("TotInvValFc")]
        public decimal TotInvValFc { get; set; }
    }
    public class DocDtls
    {
        [JsonProperty("Typ")]
        public string Typ { get; set; }

        [JsonProperty("No")]
        public string No { get; set; }

        [JsonProperty("Dt")]
        public string Dt { get; set; }
    }
    public class SellerDtls
    {
        [JsonProperty("Gstin")]
        public string Gstin { get; set; }

        [JsonProperty("LglNm")]
        public string LglNm { get; set; }

        [JsonProperty("TrdNm")]
        public string TrdNm { get; set; }

        [JsonProperty("Addr1")]
        public string Addr1 { get; set; }

        [JsonProperty("Addr2")]
        public string Addr2 { get; set; }

        [JsonProperty("Loc")]
        public string Loc { get; set; }

        [JsonProperty("Pin")]
        public int Pin { get; set; }

        [JsonProperty("Stcd")]
        public string Stcd { get; set; }

        [JsonProperty("Ph")]
        public string Ph { get; set; }

        [JsonProperty("Em")]
        public string Em { get; set; }
    }
    public class BuyerDtls
    {
        [JsonProperty("Gstin")]
        public string Gstin { get; set; }

        [JsonProperty("LglNm")]
        public string LglNm { get; set; }

        [JsonProperty("TrdNm")]
        public string TrdNm { get; set; }

        [JsonProperty("Pos")]
        public string Pos { get; set; }

        [JsonProperty("Addr1")]
        public string Addr1 { get; set; }

        [JsonProperty("Addr2")]
        public string Addr2 { get; set; }

        [JsonProperty("Loc")]
        public string Loc { get; set; }

        [JsonProperty("Pin")]
        public int Pin { get; set; }

        [JsonProperty("Stcd")]
        public string Stcd { get; set; }

        [JsonProperty("Ph")]
        public string Ph { get; set; }

        [JsonProperty("Em")]
        public string Em { get; set; }
    }
    public class DispDtls
    {
        [JsonProperty("Nm")]
        public string Nm { get; set; }

        [JsonProperty("Addr1")]
        public string Addr1 { get; set; }

        [JsonProperty("Addr2")]
        public string Addr2 { get; set; }

        [JsonProperty("Loc")]
        public string Loc { get; set; }

        [JsonProperty("Pin")]
        public int Pin { get; set; }

        [JsonProperty("Stcd")]
        public string Stcd { get; set; }
    }
    public class ShipDtls
    {
        [JsonProperty("Gstin")]
        public string Gstin { get; set; }

        [JsonProperty("LglNm")]
        public string LglNm { get; set; }

        [JsonProperty("TrdNm")]
        public string TrdNm { get; set; }

        [JsonProperty("Addr1")]
        public string Addr1 { get; set; }

        [JsonProperty("Addr2")]
        public string Addr2 { get; set; }

        [JsonProperty("Loc")]
        public string Loc { get; set; }

        [JsonProperty("Pin")]
        public int Pin { get; set; }

        [JsonProperty("Stcd")]
        public string Stcd { get; set; }
    }

    public class TranDtls
    {
        [JsonProperty("TaxSch")]
        public string TaxSch { get; set; }

        [JsonProperty("SupTyp")]
        public string SupTyp { get; set; }

        [JsonProperty("RegRev")]
        public string RegRev { get; set; }

        [JsonProperty("EcmGstin")]
        public string EcmGstin { get; set; }

        [JsonProperty("IgstOnIntra")]
        public string IgstOnIntra { get; set; }
    }
    public class EInvoiceHeader
    {
        public string Version { get; set; }
        public string TranDtls { get; set; }
        public string DocDtls { get; set; }
        public string SellerDtls { get; set; }
        public string BuyerDtls { get; set; }
        public string DispDtls { get; set; }
        public string ShipDtls { get; set; }
        public string ItemList { get; set; }
        public string ValDtls { get; set; }
        public string PayDtls { get; set; }
    }
}
