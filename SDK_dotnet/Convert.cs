using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace GluwProSDK
{
    internal static class Convert
    {
        public static JsonSerializerSettings Settings
        {
            get;
            private set;
        }
        
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}