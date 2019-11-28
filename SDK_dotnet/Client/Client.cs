using GluwProSDK.Entity;
using GluwProSDK.Models;
using Nethereum.ABI;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GluwProSDK.Clients
{
    public sealed class Client
    {
        private readonly bool m__DEV__;
        private readonly string mApiKey;
        private readonly string mSecretKey;
        private readonly string mAddress;
        private readonly string mPrivateKey;
        private readonly string mBaseUrl;
        private readonly string mWebhookSecretKey;

        public Client(string ApiKey, string SecretKey, string Address, string PrivateKey, bool __DEV__, string WebhookSecretKey)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                throw new ArgumentNullException("ApiKey");
            }
            else if (string.IsNullOrEmpty(SecretKey))
            {
                throw new ArgumentNullException("SecretKey");
            }
            else if (string.IsNullOrEmpty(Address))
            {
                throw new ArgumentNullException("Address");
            }
            else if (string.IsNullOrEmpty(PrivateKey))
            {
                throw new ArgumentNullException("PrivateKey");
            }

            mApiKey = ApiKey;
            mSecretKey = SecretKey;
            mAddress = Address;
            mPrivateKey = PrivateKey;
            m__DEV__ = __DEV__;
            mWebhookSecretKey = WebhookSecretKey;

            if (m__DEV__ == false)
            {
                mBaseUrl = "https://api.gluwa.com";
            }
            else
            {
                mBaseUrl = "https://sandbox.api.gluwa.com";
            }
        }

        private string GetContractAddress(ECurrency? currency, bool __DEV__)
        {
            if (currency == ECurrency.USDG)
            {
                if (__DEV__ == true)
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
                if (__DEV__ == true)
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

        private string GetTimestampSignature()
        {
            var signer = new EthereumMessageSigner();
            var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            var signature = signer.EncodeUTF8AndSign(Timestamp, new EthECKey(mPrivateKey));

            string gluwaSignature = $"{Timestamp}.{signature}";
            byte[] gluwaSignature_byte = new byte[gluwaSignature.Length];
            gluwaSignature_byte = System.Text.Encoding.UTF8.GetBytes(gluwaSignature);
            string encodedData = System.Convert.ToBase64String(gluwaSignature_byte);

            return encodedData;
        }

        public async Task<ResultWithNoError<BalanceResponse>> GetAddressAsync(ECurrency? currency)
        {

            var result = new ResultWithNoError<BalanceResponse>();
            string requestUri = $"{mBaseUrl}/v1/{currency}/Addresses/{mAddress}";

            using (System.Net.Http.HttpClient confClient = new System.Net.Http.HttpClient())
            {
                HttpResponseMessage message = await confClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    BalanceResponse balanceresponse = await message.Content.ReadAsAsync<BalanceResponse>();
                    result.IsSuccess = true;
                    result.Data = balanceresponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }

            return result;
        }

        private async Task<ResultWithNoError<FeeResponse>> GetFeeAsync(ECurrency? currency)
        {
            var result = new ResultWithNoError<FeeResponse>();

            string requestUri = $"{mBaseUrl}/v1/{currency}/Fee";

            using (System.Net.Http.HttpClient confClient = new System.Net.Http.HttpClient())
            {
                HttpResponseMessage message = await confClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    FeeResponse feeresponse = await message.Content.ReadAsAsync<FeeResponse>();
                    result.IsSuccess = true;
                    result.Data = feeresponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }
            return result;
        }

        public async Task<ResultWithNoError<List<TransactionResponse>>> GetListTransactionsAsync(ECurrency? currency, uint limit = 100, ETransactionStatusFilter? status = ETransactionStatusFilter.Confirmed, uint offset = 0)
        {
            var result = new ResultWithNoError<List<TransactionResponse>>();

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

            using (System.Net.Http.HttpClient confClient = new System.Net.Http.HttpClient())
            {

                confClient.DefaultRequestHeaders.Add("X-REQUEST-SIGNATURE",
                       GetTimestampSignature());
                HttpResponseMessage message = await confClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    var transactionresponse = await message.Content.ReadAsAsync<List<TransactionResponse>>();
                    result.IsSuccess = true;
                    result.Data = transactionresponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }

            return result;
        }

        public async Task<ResultWithNoError<TransactionResponse>> GetListTransactionDetailAsync(ECurrency? currency, string txnHash)
        {
            if (string.IsNullOrEmpty(txnHash))
            {
                throw new ArgumentNullException("TxnHash");
            }
            var result = new ResultWithNoError<TransactionResponse>();

            string requestUri = $"{mBaseUrl}/v1/{currency}/Transactions/{txnHash}";

            using (System.Net.Http.HttpClient confClient = new System.Net.Http.HttpClient())
            {
                confClient.DefaultRequestHeaders.Add("X-REQUEST-SIGNATURE",
                       GetTimestampSignature());
                HttpResponseMessage message = await confClient.GetAsync(requestUri);
                if (message.IsSuccessStatusCode)
                {
                    TransactionResponse transactionresponse = await message.Content.ReadAsAsync<TransactionResponse>();
                    result.IsSuccess = true;
                    result.Data = transactionresponse;

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)message.StatusCode, message.ReasonPhrase);
                }
            }

            return result;
        }

        public async Task<ResultWithNoError<PostTransctionResponse>> PostTransactionAsync(ECurrency? currency, string amount, string target, string merchantOrderID = "", string note = "", int? expiry = 1800)
        {
            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException(nameof(amount));
            }
            else if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target));
            }
            else if (string.IsNullOrEmpty(GetContractAddress(currency, m__DEV__)))
            {
                throw new ArgumentNullException(nameof(currency));
            }

            var result = new ResultWithNoError<PostTransctionResponse>();

            var requestUri = $"{mBaseUrl}/v1/Transactions";

            var getfee = GetFeeAsync(currency).Result.Data.MinimumFee;

            MakerBigInteger convertor = new MakerBigInteger();
            var convertAmount = convertor.ConvertToGluwacoinBigInteger(amount);
            var convertFee = convertor.ConvertToGluwacoinBigInteger(getfee);
            var nonce = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            var abiEncode = new ABIEncode();
            var messageHash = abiEncode.GetSha3ABIEncodedPacked(
                new ABIValue("address", GetContractAddress(currency, m__DEV__)),
                new ABIValue("address", mAddress),
                new ABIValue("address", target),
                new ABIValue("uint256", convertAmount),
                new ABIValue("uint256", convertFee),
                new ABIValue("uint256", nonce)
                );

            var signer = new EthereumMessageSigner();
            var addressRecovered = signer.Sign(messageHash, mPrivateKey);

            TransactionRequest bodyParams = new TransactionRequest
            {
                Signature = addressRecovered,
                Currency = currency,
                Target = target,
                Amount = amount,
                Fee = getfee,
                Source = mAddress,
                Nonce = nonce.ToString(),
                MerchantOrderID = merchantOrderID,
                Note = note
            };

            string json = bodyParams.ToJson();
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using (System.Net.Http.HttpClient confClient = new System.Net.Http.HttpClient())
            using (var response = await confClient.PostAsync(requestUri, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    result.IsSuccess = true;
                    result.Data = await response.Content.ReadAsAsync<PostTransctionResponse>();

                    return result;
                }
                else
                {
                    throw new HttpListenerException((int)response.StatusCode, response.ReasonPhrase);
                }
            }

            return result;
        }

        public async Task<ResultWithNoError<byte[]>> GetPaymentQRCodeAsync(EPaymentCurrency? currency, string amount, string format = "image/png", string note = "", string merchantOrderID = "", int? expiry = 1800)
        {
            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException(nameof(amount));
            }

            var result = new ResultWithNoError<byte[]>();

            var requestUri = $"{mBaseUrl}/v1/QRCode";

            byte[] format_byte = new byte[format.Length];
            format_byte = System.Text.Encoding.UTF8.GetBytes(format);
            var formatToBase64 = System.Convert.ToBase64String(format_byte);

            var queryParams = new List<string>();
            if (format != null)
            {
                queryParams.Add($"format={format}");
            }
            if (queryParams.Any())
            {
                requestUri = $"{requestUri}?{string.Join("&", queryParams)}";
            }

            QRCodeRequest bodyParams = new QRCodeRequest()
            {
                Signature = GetTimestampSignature(),
                Currency = currency,
                Target = mAddress,
                Amount = amount,
                Expiry = expiry,
                Note = note,
                MerchantOrderID = merchantOrderID
            };

            string json = bodyParams.ToJson();
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var authenticationBytes = Encoding.ASCII.GetBytes($"{mApiKey}:{mSecretKey}");
            System.Net.Http.HttpClient confClient = new System.Net.Http.HttpClient();
            confClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                 System.Convert.ToBase64String(authenticationBytes));

            using (var response = await confClient.PostAsync(requestUri, content))
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
            return result;
        }

        public bool ValidateWebhook(PayLoad mpayload, string signature)
        {
            string payload = Convert.ToJson<PayLoad>(mpayload).ToString();

            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }
            else if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentNullException(nameof(signature));
            }

            string payloadHashBase64Encode;
            byte[] key = Encoding.UTF8.GetBytes(mWebhookSecretKey);
            using (var encryptor = new HMACSHA256(key))
            {
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                byte[] payloadHashedBytes = encryptor.ComputeHash(payloadBytes);
                payloadHashBase64Encode = System.Convert.ToBase64String(payloadHashedBytes);
            }

            if (payloadHashBase64Encode == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}