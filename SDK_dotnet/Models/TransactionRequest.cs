using System;
using System.ComponentModel.DataAnnotations;

namespace SDK_dotnet.Models
{
    internal sealed class TransactionRequest
    {
        [Required]
        public string Signature { get; set; }

        [Required]
        public string Amount { get; set; }

        [Required]
        public string Fee { get; set; }

        [Required]
        public ECurrency? Currency { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        public string Target { get; set; }

        [StringLength(60)]
        public string MerchantOrderID { get; set; }

        [StringLength(280)]
        public string Note { get; set; }

        public string Nonce { get; set; }

        public Guid? Idem { get; set; }

        public Guid? PaymentID { get; set; }

        public string PaymentSig { get; set; }
    }
}
