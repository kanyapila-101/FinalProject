using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using ServiceStack.Text;

namespace Students_Attendance_Project.Common
{
    public class JsonServiceStackValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext");

            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            var reader = new StreamReader(controllerContext.HttpContext.Request.InputStream).BaseStream;

            return new DictionaryValueProvider<object>(JsonSerializer.DeserializeFromStream<Dictionary<string, object>>(reader).AsExpandoObject(),
                    CultureInfo.CurrentCulture);
        }
    }

    public static class CollectionExtensions
    {
        public static ExpandoObject AsExpandoObject(this IDictionary<string, object> dictionary)
        {
            var epo = new ExpandoObject();
            var epoDic = epo as IDictionary<string, object>;

            foreach (var item in dictionary)
            {
                bool processed = false;

                if (item.Value is IDictionary<string, object>)
                {
                    epoDic.Add(item.Key, AsExpandoObject((IDictionary<string, object>)item.Value));
                    processed = true;
                }
                else if (item.Value is ICollection)
                {
                    var itemList = new List<object>();
                    foreach (var item2 in (ICollection)item.Value)
                        if (item2 is IDictionary<string, object>)
                            itemList.Add(AsExpandoObject((IDictionary<string, object>)item2));
                        else
                            itemList.Add(AsExpandoObject(new Dictionary<string, object> { { "Unknown", item2 } }));

                    if (itemList.Count > 0)
                    {
                        epoDic.Add(item.Key, itemList);
                        processed = true;
                    }
                }

                if (!processed)
                    epoDic.Add(item);
            }

            return epo;
        }
    }

    public class JsonServiceStackResult : JsonResult
    {
        //public override void ExecuteResult(ControllerContext context)
        //{
        //    HttpResponseBase response = context.HttpContext.Response;
        //    response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

        //    if (ContentEncoding != null)
        //    {
        //        response.ContentEncoding = ContentEncoding;
        //    }
        //    JsConfig.ExcludeTypeInfo = true;
        //    //JsConfig.DateHandler = DateHandler.ISO8601;
        //    JsConfig<DateTime>.SerializeFn = date => new DateTime(date.Ticks, DateTimeKind.Local).ToString("dd/MM/yyyy HH:mm:ss",Shared.CultureInfoTh);
        //    JsConfig<DateTime?>.SerializeFn = date => date != null ? new DateTime(date.Value.Ticks, DateTimeKind.Local).ToString("dd/MM/yyyy HH:mm:ss", Shared.CultureInfoTh) : "";
        //    if (Data != null)
        //    {
        //        string callbackfunction = context.HttpContext.Request["callback"];
        //        if (!string.IsNullOrEmpty(callbackfunction))
        //        {

        //            response.Write(string.Format("{0}({1});", callbackfunction, JsonSerializer.SerializeToString(Data)));
        //        }
        //        else
        //        {
        //            response.Write(JsonSerializer.SerializeToString(Data));
        //        }

        //    }
        //}
    }
}