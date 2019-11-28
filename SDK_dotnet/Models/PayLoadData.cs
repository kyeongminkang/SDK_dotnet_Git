using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GluwProSDK.Models
{
    public sealed class PayLoadData
    {
        public string MerchantOrderID { get; set; }

        [Required]
        public int EventType { get; set; }

        [Required]
        public int Type { get; set; }

        public string ResourceID { get; set; }
    }
}
