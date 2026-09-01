using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TFDSolution.Business;
using static TFDSolution.Business.CommonEnum;

namespace TFDSolution.Common
{
    public static class CommonHelper
    {
        private static readonly HttpClient client = new HttpClient();
        public static string GetEnumDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? enumValue.ToString();
        }
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public static void CleanupOldFiles(string folderPath, string reportTitle)
        {
            var oldFiles = Directory
                .EnumerateFiles(folderPath)
                .Where(f => Path.GetFileName(f)
                    .StartsWith(reportTitle, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in oldFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                    // Log delete failure if needed
                }
            }
        }

        public static async Task<string> Translate(Language fromLang, Language toLang, string text)
        {
            try
            {
                string fromCode = CommonBusiness.GetLanguageCode(fromLang);
                string toCode = CommonBusiness.GetLanguageCode(toLang);

                string url =$"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromCode}&tl={toCode}&dt=t&q={Uri.EscapeDataString(text)}";

                string result = await client.GetStringAsync(url);

                JArray json = JArray.Parse(result);

                return json[0][0][0]?.ToString();
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return text;
            }
        }

        public static async Task<string> TranslateCode(string fromCode, string toCode, string text)
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromCode}&tl={toCode}&dt=t&q={Uri.EscapeDataString(text)}";

                string result = await client.GetStringAsync(url);

                JArray json = JArray.Parse(result);

                return json[0][0][0]?.ToString();
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return text;
            }
        }

        public static string SqlSafe(string value)
        {
            return value == null ? "" : value.Replace("'", "''");
        }
    }
}