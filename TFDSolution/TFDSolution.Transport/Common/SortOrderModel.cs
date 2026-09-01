using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Common
{
     //Added Model for receiving SortOrder updates
        public class SortOrderUpdateRequest
        {
            public System.Guid ParentFormId { get; set; }
            public List<FormSortOrderUpdateModel> Rows { get; set; }
        }
        public class FormSortOrderUpdateModel
        {
            public System.Guid FormId { get; set; }
            public string FormName { get; set; }
            public int SortOrder { get; set; }
        }

    public class SortOrderFieldUpdate
    {
        public int ItemAdvanceId { get; set; }
        public List<FormSortOrderUpdateModel> Rows { get; set; }
    }

}
