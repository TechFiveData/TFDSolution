using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class ChartDataResult : ResponseModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> Values { get; set; }
        public List<object> data { get; set; }

    }
}
