using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class Rep_StockSummaryModel
    {
        public int Sr_No { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public double Opening { get; set; }
        public double Inward { get; set; }
        public double Outward { get; set; }
        public double Closing { get; set; }
        public string UOM { get; set; }
    }
}
