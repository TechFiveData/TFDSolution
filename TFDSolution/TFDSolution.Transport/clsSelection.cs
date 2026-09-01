using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class clsSelection
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public  class FieldSelectionResponse
    {
        public List<dynamic> Data { get; set; }
        public bool HasMore { get; set; }
    }
}
