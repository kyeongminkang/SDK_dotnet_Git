using System;
using System.Collections.Generic;

namespace SDK_dotnet.Models
{
    public sealed class TransactionResponse
    {
        public string TxnHash { get; set; }

        public string Amount { get; set; }

        public string Fee { get; set; }

        public ECurrency? Currency { get; set; }

        public IList<string> Sources { get; set; }

        public IList<string> Targets { get; set; }

        public string MerchantOrderID { get; set; }

        public string Note { get; set; }

        public ETransactionStatus? Status { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }
    }
}
