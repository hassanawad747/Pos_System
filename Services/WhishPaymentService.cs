using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Pos_System.Services
{
    internal sealed class WhishPaymentService
    {
        private readonly string connectionString;

        public WhishPaymentService(string connectionString = null)
        {
            this.connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? POS_System.Program.SettingsManager.ConnectionString
                : connectionString;
        }

        public sealed class AuthorizationResult
        {
            public bool IsAuthorized { get; set; }
            public string Status { get; set; }
            public string ProviderTransactionId { get; set; }
            public string Message { get; set; }
        }

        public AuthorizationResult Authorize(string phoneNumber, decimal amount, string currency, string clientReference)
        {
            string normalizedPhone = NormalizeLebanonPhone(phoneNumber);
            AuthorizationResult result;

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                result = new AuthorizationResult
                {
                    IsAuthorized = false,
                    Status = "INVALID_PHONE",
                    Message = "Enter a valid Lebanese phone number for the Whish payer."
                };
                RecordAttempt(null, phoneNumber, amount, currency, clientReference, result);
                return result;
            }

            if (amount <= 0m)
            {
                result = new AuthorizationResult
                {
                    IsAuthorized = false,
                    Status = "INVALID_AMOUNT",
                    Message = "Whish payment amount must be greater than zero."
                };
                RecordAttempt(null, normalizedPhone, amount, currency, clientReference, result);
                return result;
            }

            string normalizedCurrency = (currency ?? string.Empty).Trim().ToUpperInvariant();
            if (normalizedCurrency != "USD" && normalizedCurrency != "LBP")
            {
                result = new AuthorizationResult
                {
                    IsAuthorized = false,
                    Status = "INVALID_CURRENCY",
                    Message = "Whish payment currency must be USD or LBP."
                };
                RecordAttempt(null, normalizedPhone, amount, normalizedCurrency, clientReference, result);
                return result;
            }

            /*
             * SECURITY / PROVIDER CONTRACT GATE
             * ---------------------------------
             * The public Whish website confirms Whish Pay exists for merchants/custom integrations,
             * but this repository does not yet have the merchant API contract, endpoint schema,
             * authentication/signature rules, webhook verification rules, or production credentials.
             *
             * Do NOT guess those fields and do NOT mark a payment as PAID from a phone number or a
             * cashier-entered reference. Until the official contract is supplied, live Whish rows
             * fail closed so customer balances and cash reports cannot be falsely settled.
             *
             * When the official contract is available, implement the provider call on the SERVER
             * payment bridge and return an authorized result only after provider verification.
             */
            result = new AuthorizationResult
            {
                IsAuthorized = false,
                Status = "PROVIDER_NOT_CONFIGURED",
                Message = "Whish live merchant gateway is not configured. Official merchant API documentation and credentials are required before this payment can be accepted as PAID."
            };

            RecordAttempt(null, normalizedPhone, amount, normalizedCurrency, clientReference, result);
            return result;
        }

        private void RecordAttempt(
            int? saleId,
            string phoneNumber,
            decimal amount,
            string currency,
            string clientReference,
            AuthorizationResult result)
        {
            try
            {
                string normalizedPhone = NormalizeLebanonPhone(phoneNumber) ?? (phoneNumber ?? string.Empty).Trim();
                string normalizedCurrency = string.Equals(currency, "LBP", StringComparison.OrdinalIgnoreCase) ? "LBP" : "USD";
                string reference = string.IsNullOrWhiteSpace(clientReference)
                    ? "POS-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
                    : clientReference.Trim();

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.WhishPaymentAttempts',N'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.WhishPaymentAttempts
        (sale_id,payer_phone,amount,currency,client_reference,provider_transaction_id,
         provider_status,provider_message,requested_by,requested_at,verified_at)
    VALUES
        (@sale_id,@payer_phone,@amount,@currency,@client_reference,@provider_transaction_id,
         @provider_status,@provider_message,@requested_by,SYSUTCDATETIME(),@verified_at);
END;", connection))
                {
                    command.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId.HasValue ? (object)saleId.Value : DBNull.Value;
                    command.Parameters.Add("@payer_phone", SqlDbType.NVarChar, 40).Value = string.IsNullOrWhiteSpace(normalizedPhone) ? (object)"UNKNOWN" : normalizedPhone;
                    var amountParameter = command.Parameters.Add("@amount", SqlDbType.Decimal);
                    amountParameter.Precision = 24;
                    amountParameter.Scale = 8;
                    amountParameter.Value = amount > 0m ? amount : 0.00000001m;
                    command.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = normalizedCurrency;
                    command.Parameters.Add("@client_reference", SqlDbType.NVarChar, 100).Value = reference;
                    command.Parameters.Add("@provider_transaction_id", SqlDbType.NVarChar, 150).Value = string.IsNullOrWhiteSpace(result?.ProviderTransactionId) ? (object)DBNull.Value : result.ProviderTransactionId;
                    command.Parameters.Add("@provider_status", SqlDbType.NVarChar, 40).Value = string.IsNullOrWhiteSpace(result?.Status) ? "UNKNOWN" : result.Status;
                    command.Parameters.Add("@provider_message", SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(result?.Message) ? (object)DBNull.Value : result.Message;
                    command.Parameters.Add("@requested_by", SqlDbType.Int).Value = AppSession.UserId > 0 ? (object)AppSession.UserId : DBNull.Value;
                    command.Parameters.Add("@verified_at", SqlDbType.DateTime2).Value = result != null && result.IsAuthorized ? (object)DateTime.UtcNow : DBNull.Value;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                // Payment authorization result takes precedence. Audit persistence must never turn a
                // safe provider rejection into an application crash or a false successful payment.
            }
        }

        public static string NormalizeLebanonPhone(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

            string digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("961"))
                digits = digits.Substring(3);
            if (digits.StartsWith("0") && digits.Length >= 8)
                digits = digits.Substring(1);

            if (digits.Length < 7 || digits.Length > 8)
                return null;

            return "+961" + digits;
        }
    }
}
