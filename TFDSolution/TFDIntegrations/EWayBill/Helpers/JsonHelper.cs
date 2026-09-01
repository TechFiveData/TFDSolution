using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Helpers
{
    public static class JsonHelper
    {
        /// <summary>
        /// Serialize object to JSON.
        /// </summary>
        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(
                data,
                Formatting.None);
        }


        /// <summary>
        /// Deserialize JSON.
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(
                json);
        }
    }
}