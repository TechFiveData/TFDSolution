using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class FormFieldData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string ShortName { get; set; }

    }

    public class ProcedureSchema
    {
        public List<string> Columns { get; set; }
        public List<ProcedureRequest> Request { get; set; }

        public List<string> SPColumns { get; set; }
    }
    public class ProcedureRequest
    {
        public int ORDINAL_POSITION { get; set; }
        public string PARAMETER_NAME { get; set; }
        public string PARAMETER_MODE { get; set; }
        public string DATA_TYPE { get; set; }
        public Nullable<int> CHARACTER_MAXIMUM_LENGTH { get; set; }
        public string PARAMETER_Value { get; set; }

    }
}
