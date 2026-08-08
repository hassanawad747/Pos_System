using System;
using System.Linq;

namespace Pos_System.Services
{
    internal sealed class WhishPaymentService
    {
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
            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                return new AuthorizationResult
                {
                    IsAuthorized = false,
                    Status = "INVALID_PHONE",
                    Message = "Enter a valid Lebanese phone number for the Whish payer."
                };
            }

            if (amount <= 0m)
            {
                return new AuthorizationResult
                {
                    IsAuthorized = false,
                    Status = "INVALID_AMOUNT",
                    Message = "Whish payment amount must be greater than zero."
                };
            }

            string normalizedCurrency = (currency ?? string.Empty).Trim().ToUpperInvariant();
            if (normalizedCurrency != "USD" && normalizedCurrency != "LBP")
            {
                return new AuthorizationResult
                {
                    IsAuthorized = false,
                    Status = "INVALID_CURRENCY",
                    Message = "Whish payment currency must be USD or LBP."
                };
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
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Status = "PROVIDER_NOT_CONFIGURED",
                Message = "Whish live merchant gateway is not configured. Official merchant API documentation and credentials are required before this payment can be accepted as PAID."
            };
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
