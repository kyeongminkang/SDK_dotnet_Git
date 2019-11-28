using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GluwProSDK
{
    internal class MakerBigInteger
    {
        public BigInteger ConvertToGluwacoinBigInteger(string amount)
        {
            BigDecimal bigDecimalAmount = BigDecimal.Parse(amount);
            return BigInteger.Parse((bigDecimalAmount * new BigDecimal(1, 18)).Floor().ToString());
        }
    }
}
