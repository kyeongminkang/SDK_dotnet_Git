﻿using System.ComponentModel.DataAnnotations;

namespace SDK_dotnet.Models
{
    internal sealed class QRCodeRequest
    {
        [Required]
        public string Signature { get; set; }

        [Required]
        public EPaymentCurrency? Currency { get; set; }

        [Required]
        public string Target { get; set; }

        [Required]
        public string Amount { get; set; }

        [StringLength(60)]
        public string MerchantOrderID { get; set; }

        [StringLength(280)]
        public string Note { get; set; }

        [Range(0, int.MaxValue)]
        public int? Expiry { get; set; }
    }
}