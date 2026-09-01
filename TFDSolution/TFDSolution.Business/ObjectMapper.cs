using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Business
{
    public static class ObjectMapper
    {
        /////

        ///// Converts a DataTable to a list with generic objects
        /////
        ///// Generic object
        ///// DataTable
        ///// List with generic objects
        //public static List DataTableToList(this DataTable table) where T : class, new()
        //{
        //    //From: https://codereview.stackexchange.com/questions/30714/converting-datatable-to-list-of-class

        //    try
        //    {
        //        List list = new List();

        //        foreach (var row in table.AsEnumerable())
        //        {
        //            T obj = new T();

        //            foreach (var prop in obj.GetType().GetProperties())
        //            {
        //                try
        //                {
        //                    PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
        //                    propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
        //                }
        //                catch
        //                {
        //                    continue;
        //                }
        //            }

        //            list.Add(obj);
        //        }

        //        return list;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


        //public static T DataRowToList(this DataRow row) where T : class, new()
        //{
        //    //Variant of DataTableToList code from: https://codereview.stackexchange.com/questions/30714/converting-datatable-to-list-of-class

        //    try
        //    {
        //        List list = new List();

        //        T obj = new T();

        //        foreach (var prop in obj.GetType().GetProperties())
        //        {
        //            try
        //            {
        //                PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
        //                propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
        //            }
        //            catch
        //            {
        //                continue;
        //            }
        //        }

        //        return obj;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


        //public static List DataRowArrayToList(this DataRow[] dataRowArray) where T : class, new()
        //{
        //    //Variant of DataTableToList code from: https://codereview.stackexchange.com/questions/30714/converting-datatable-to-list-of-class

        //    try
        //    {
        //        List list = new List();

        //        foreach (var row in dataRowArray.AsEnumerable())
        //        {
        //            T obj = new T();

        //            foreach (var prop in obj.GetType().GetProperties())
        //            {
        //                try
        //                {
        //                    PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
        //                    propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
        //                }
        //                catch
        //                {
        //                    continue;
        //                }
        //            }

        //            list.Add(obj);
        //        }

        //        return list;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

    }
}
