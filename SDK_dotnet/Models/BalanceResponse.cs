using GluwProSDK.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace GluwProSDK.Models
{
    public sealed class BalanceResponse
    {
        public string Balance { get; set; }

        public ECurrency? Currency { get; set; }

        public long? Nonce { get; set; }
    }
}
