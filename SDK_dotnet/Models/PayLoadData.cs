using System.ComponentModel.DataAnnotations;

namespace SDK_dotnet.Models
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
