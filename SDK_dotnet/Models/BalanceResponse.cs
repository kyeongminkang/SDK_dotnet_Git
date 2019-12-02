using System.ComponentModel.DataAnnotations;

namespace SDK_dotnet.Models
{
    public sealed class BalanceResponse
    {
        public string Balance { get; set; }

        public ECurrency? Currency { get; set; }

        [Range(0, ulong.MaxValue)]
        public long? Nonce { get; set; }
    }
}
