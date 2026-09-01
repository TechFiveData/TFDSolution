using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class ResponseModel
    {
        public Nullable<bool> IsSuccess { get; set; }
        public string Response { get; set; }
        public string Action { get; set; }
        public Nullable<System.Guid> PrimaryId { get; set; }

        public int Id { get; set; }
        public string URNNo { get; set; }
    }
    public class ExchangeRateResponse
    {
        public string Symbol { get; set; }
        public string alias { get; set; }
        public Dictionary<string, decimal> rates { get; set; }
    }  
}