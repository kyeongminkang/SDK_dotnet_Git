using System.ComponentModel.DataAnnotations;

namespace SDK_dotnet.Models
{
    public sealed class FeeResponse
    {
        [Required]
        public ECurrency? Currency { get; set; }
    
        [Required]
        public string MinimumFee { get; set; }
    }
}
