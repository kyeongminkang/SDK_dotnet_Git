using Nethereum.ABI;
using Nethereum.Signer;
using SDK_dotnet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDK_dotnet.Client
{
    public sealed class Client
    {
        private readonly bool mIsDevEnv;
        private readonly string mApiKey;
        private readonly string mSecretKey;
        private readonly string mAddress;
        private readonly string mPrivateKey;
        private readonly string mBaseUrl;
        private readonly string mWebhookSecretKey;

        public Client(string apiKey, string secretKey, string address, string privateKey, bool isDevEnv, string webhookSecretKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }
            else if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException(nameof(secretKey));
            }
            else if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address));
            }
            else if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            mApiKey = apiKey;
            mSecretKey = secretKey;
            mAddress = address;
            mPrivateKey = privateKey;
            mIsDevEnv = isDevEnv;
            mWebhookSecretKey = webhookSecretKey;

            if (mIsDevEnv)
            {
                mBaseUrl = "https://sandbox.api.gluwa.com";
            }
            else
            {
                mBaseUrl = "https://api.gluwa.com";
            }
        }

        public async Task<Result<BalanceResponse>> getAddressAsync(ECurrency? currency)
        {
            var result = new Result<BalanceResponse>();
            string requestUri = $"{mBaseUrl}/v1/{currency}/Addresses/{mAddress}";

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage message = await httpClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    BalanceResponse balanceResponse = await message.Content.ReadAsAsync<BalanceResponse>();
                    result.IsSuccess = true;
                    result.Data = balanceResponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }
        }

        public async Task<Result<List<TransactionResponse>>> getListTransactionsAsync(ECurrency? currency,
            uint limit = 100, ETransactionStatusFilter status = ETransactionStatusFilter.Confirmed, uint offset = 0)
        {
            var result = new Result<List<TransactionResponse>>();

            string requestUri = $"{mBaseUrl}/v1/{currency}/Addresses/{mAddress}/Transactions";

            var queryParams = new List<string>();
            if (offset > 0)
            {
                queryParams.Add($"offset={offset}");
            }
            if (limit > 0)
            {
                queryParams.Add($"limit={limit}");
            }
            if (status != null)
            {
                queryParams.Add($"status={status}");
            }
            if (queryParams.Any())
            {
                requestUri = $"{requestUri}?{string.Join("&", queryParams)}";
            }

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-REQUEST-SIGNATURE", getTimestampSignature());
                HttpResponseMessage message = await httpClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    List<TransactionResponse> transactionResponse = await message.Content.ReadAsAsync<List<TransactionResponse>>();
                    result.IsSuccess = true;
                    result.Data = transactionResponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }
        }

        public async Task<Result<TransactionResponse>> GetListTransactionDetailAsync(ECurrency? currency, string txnHash)
        {
            if (currency == null)
            {
                throw new ArgumentNullException(nameof(currency));
            }
            else if (string.IsNullOrEmpty(txnHash))
            {
                throw new ArgumentNullException(nameof(txnHash));
            }
            var result = new Result<TransactionResponse>();

            string requestUri = $"{mBaseUrl}/v1/{currency}/Transactions/{txnHash}";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-REQUEST-SIGNATURE", getTimestampSignature());
                HttpResponseMessage message = await httpClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    TransactionResponse transactionResponse = await message.Content.ReadAsAsync<TransactionResponse>();
                    result.IsSuccess = true;
                    result.Data = transactionResponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }
        }

        public async Task<Result<HttpStatusCode>> postTransactionAsync(ECurrency? currency, string amount, string target, string merchantOrderID = "", string note = "", int expiry = 1800)
        {
            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException(nameof(amount));
            }
            else if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target));
            }
            else if (string.IsNullOrEmpty(getContractAddressOrNull(currency, mIsDevEnv)))
            {
                throw new ArgumentNullException(nameof(currency));
            }

            var result = new Result<HttpStatusCode>();

            var requestUri = $"{mBaseUrl}/v1/Transactions";

            var getFee = getFeeAsync(currency).Result.Data.MinimumFee;

            BigInteger convertAmount = Converter.ConvertToGluwacoinBigInteger(amount);
            BigInteger convertFee = Converter.ConvertToGluwacoinBigInteger(getFee);
            int nonce = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            ABIEncode abiEncode = new ABIEncode();
            byte[] messageHash = abiEncode.GetSha3ABIEncodedPacked(
                new ABIValue("address", getContractAddressOrNull(currency, mIsDevEnv)),
                new ABIValue("address", mAddress),
                new ABIValue("address", target),
                new ABIValue("uint256", convertAmount),
                new ABIValue("uint256", convertFee),
                new ABIValue("uint256", nonce)
                );

            var signer = new EthereumMessageSigner();
            string addressRecovered = signer.Sign(messageHash, mPrivateKey);

            TransactionRequest bodyParams = new TransactionRequest
            {
                Signature = addressRecovered,
                Currency = currency,
                Target = target,
                Amount = amount,
                Fee = getFee,
                Source = mAddress,
                Nonce = nonce.ToString(),
                MerchantOrderID = merchantOrderID,
                Note = note
            };

            string json = bodyParams.ToJson();
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient httpClient = new HttpClient())
            using (var response = await httpClient.PostAsync(requestUri, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    result.IsSuccess = true;
                    result.Data = await response.Content.ReadAsAsync<HttpStatusCode>();

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)response.StatusCode, response.ReasonPhrase);
                }
            }
        }

        public async Task<Result<byte[]>> getPaymentQRCodeAsync(EPaymentCurrency? currency, 
            string amount, string format = "image/png", string note = "", 
            string merchantOrderID = "", int expiry = 1800)
        {
            if(currency == null)
            {
                throw new ArgumentNullException(nameof(currency));
            }
            else if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException(nameof(amount));
            }

            var result = new Result<byte[]>();

            var requestUri = $"{mBaseUrl}/v1/QRCode";

            byte[] formatByte = new byte[format.Length];
            formatByte = Encoding.UTF8.GetBytes(format);

            var queryParams = new List<string>();
            if (format != null)
            {
                queryParams.Add($"format={format}");
                requestUri = $"{requestUri}?{string.Join("&", queryParams)}";
            }
           
            QRCodeRequest bodyParams = new QRCodeRequest()
            {
                Signature = getTimestampSignature(),
                Currency = currency,
                Target = mAddress,
                Amount = amount,
                Expiry = expiry,
                Note = note,
                MerchantOrderID = merchantOrderID
            };

            string json = bodyParams.ToJson();
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            byte[] authenticationBytes = Encoding.ASCII.GetBytes($"{mApiKey}:{mSecretKey}");
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                     System.Convert.ToBase64String(authenticationBytes));
                using (HttpResponseMessage response = await httpClient.PostAsync(requestUri, content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result.IsSuccess = true;
                        result.Data = await response.Content.ReadAsByteArrayAsync();

                        return result;
                    }
                    else
                    {
                        throw new HttpListenerException((int)response.StatusCode, response.ReasonPhrase);
                    }
                };
            }
        }

        public bool ValidateWebhook(PayLoad payLoad, string signature)
        {
            string payload = Converter.ToJson<PayLoad>(payLoad).ToString();

            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }
            else if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentNullException(nameof(signature));
            }

            string payloadHashBase64;
            byte[] key = Encoding.UTF8.GetBytes(mWebhookSecretKey);
            using (var encryptor = new HMACSHA256(key))
            {
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                byte[] payloadHashedBytes = encryptor.ComputeHash(payloadBytes);
                payloadHashBase64 = System.Convert.ToBase64String(payloadHashedBytes);
            }

            if (payloadHashBase64 == signature)
            {
                return true;
            }
                return false;
        }

        private string getContractAddressOrNull(ECurrency? currency, bool mIsDevEnv)
        {
            if (currency == ECurrency.USDG)
            {
                if (mIsDevEnv)
                {
                    return "0x8e9611f8ebc9323EdDA39eA2d8F31bbb2436adEE";
                }
                else
                {
                    return "0xfb0aaa0432112779d9ac483d9d5e3961ece18eec";
                }
            }
            else if (currency == ECurrency.KRWG)
            {
                if (mIsDevEnv)
                {
                    return "0x408b7959b3e15b8b1e8495fa9cb123c0180d44db";
                }
                else
                {
                    return "0x4cc8486f2f3dce2d3b5e27057cf565e16906d12d";
                }
            }
            return null;
        }

        private string getTimestampSignature()
        {
            var signer = new EthereumMessageSigner();
            string Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string signature = signer.EncodeUTF8AndSign(Timestamp, new EthECKey(mPrivateKey));

            string gluwaSignature = $"{Timestamp}.{signature}";
            byte[] gluwaSignatureByte = new byte[gluwaSignature.Length];
            gluwaSignatureByte = Encoding.UTF8.GetBytes(gluwaSignature);
            string encodedData = System.Convert.ToBase64String(gluwaSignatureByte);

            return encodedData;
        }

        private async Task<Result<FeeResponse>> getFeeAsync(ECurrency? currency)
        {
            var result = new Result<FeeResponse>();

            string requestUri = $"{mBaseUrl}/v1/{currency}/Fee";

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage message = await httpClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    FeeResponse feeResponse = await message.Content.ReadAsAsync<FeeResponse>();
                    result.IsSuccess = true;
                    result.Data = feeResponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }
        }
    }
}