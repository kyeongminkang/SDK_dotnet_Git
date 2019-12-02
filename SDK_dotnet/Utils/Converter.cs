using Nethereum.Util;
using Newtonsoft.Json;
using System.Numerics;

namespace SDK_dotnet
{
    internal static class Converter
    {
        public static JsonSerializerSettings Settings { get; private set; }

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static BigInteger ConvertToGluwacoinBigInteger(string amount)
        {
            BigDecimal bigDecimalAmount = BigDecimal.Parse(amount);
            return BigInteger.Parse((bigDecimalAmount * new BigDecimal(1, 18)).Floor().ToString());
        }
    }
}