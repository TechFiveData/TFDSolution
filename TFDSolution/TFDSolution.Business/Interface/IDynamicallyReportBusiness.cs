using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business.Interface
{
    public interface IDynamicallyReportBusiness
    {
        ProcedureSchema GetSourceColumns(string tableName);
    }
}
