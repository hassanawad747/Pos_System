using Pos_System.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    public class CustomerLedgerService
    {
        private const byte MoneyPrecision = 24;
        private const byte MoneyScale = 8;
        private readonly string connectionString;

        public CustomerLedgerService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            this.connectionString = connectionString;
        }

        public decimal GetBalance(int customerId, string currency = "USD")
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP (1) balance_after
FROM dbo.CustomerTransactions
WHERE customer_id = @customer_id AND currency = @currency
ORDER BY customer_transaction_id DESC;", conn))
            {
                cmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = NormalizeCurrency(currency);
                conn.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
            }
        }

        public List<CustomerTransaction> GetTransactions(int customerId, string currency = "USD")
        {
            var result = new List<CustomerTransaction>();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT customer_transaction_id, customer_id, transaction_type, reference_type, reference_id,
       debit, credit, balance_after, currency, description, user_id, created_at
FROM dbo.CustomerTransactions
WHERE customer_id = @customer_id AND currency = @currency
ORDER BY customer_transaction_id DESC;", conn))
            {
                cmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = NormalizeCurrency(currency);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new CustomerTransaction
                        {
                            CustomerTransactionId = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            TransactionType = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ReferenceType = reader.IsDBNull(3) ? null : reader.GetString(3),
                            ReferenceId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                            Debit = reader.GetDecimal(5),
                            Credit = reader.GetDecimal(6),
                            BalanceAfter = reader.GetDecimal(7),
                            Currency = reader.IsDBNull(8) ? "USD" : reader.GetString(8),
                            Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                            UserId = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                            CreatedAt = reader.GetDateTime(11)
                        });
                    }
                }
            }
            return result;
        }

        public int RecordPayment(int customerId, decimal amount, string paymentMethod, string referenceNumber, string notes, int userId, string currency = "USD")
        {
            ActionPermissionService.Demand("CUSTOMER.ADJUSTMENT");
            if (customerId <= 0) throw new InvalidOperationException("Select a customer.");
            if (userId <= 0) throw new InvalidOperationException("A logged-in user is required.");
            if (amount <= 0) throw new InvalidOperationException("Payment amount must be greater than zero.");

            string normalized = NormalizeCurrency(currency);
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal previousBalance;
                        using (var balanceCmd = new SqlCommand(@"
SELECT TOP (1) balance_after
FROM dbo.CustomerTransactions WITH (UPDLOCK, HOLDLOCK)
WHERE customer_id = @customer_id AND currency = @currency
ORDER BY customer_transaction_id DESC;", conn, tx))
                        {
                            balanceCmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                            balanceCmd.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = normalized;
                            object value = balanceCmd.ExecuteScalar();
                            previousBalance = value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
                        }

                        int paymentId;
                        using (var paymentCmd = new SqlCommand(@"
INSERT INTO dbo.CustomerPayments
(customer_id, amount, currency, payment_method, reference_number, notes, user_id, payment_date)
VALUES (@customer_id, @amount, @currency, @method, @reference, @notes, @user_id, SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            paymentCmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                            AddMoneyParameter(paymentCmd, "@amount", amount);
                            paymentCmd.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = normalized;
                            paymentCmd.Parameters.Add("@method", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(paymentMethod) ? "CASH" : paymentMethod.Trim().ToUpperInvariant();
                            paymentCmd.Parameters.Add("@reference", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(referenceNumber) ? (object)DBNull.Value : referenceNumber.Trim();
                            paymentCmd.Parameters.Add("@notes", SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes.Trim();
                            paymentCmd.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                            paymentId = Convert.ToInt32(paymentCmd.ExecuteScalar());
                        }

                        decimal newBalance = previousBalance - amount;
                        using (var ledgerCmd = new SqlCommand(@"
INSERT INTO dbo.CustomerTransactions
(customer_id, transaction_type, reference_type, reference_id, debit, credit, balance_after, currency, description, user_id, created_at)
VALUES (@customer_id, N'PAYMENT', N'CUSTOMER_PAYMENT', @payment_id, 0, @amount, @balance, @currency, @description, @user_id, SYSUTCDATETIME());", conn, tx))
                        {
                            ledgerCmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                            ledgerCmd.Parameters.Add("@payment_id", SqlDbType.Int).Value = paymentId;
                            AddMoneyParameter(ledgerCmd, "@amount", amount);
                            AddMoneyParameter(ledgerCmd, "@balance", newBalance);
                            ledgerCmd.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = normalized;
                            ledgerCmd.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value = "Customer payment" + (string.IsNullOrWhiteSpace(referenceNumber) ? string.Empty : " - " + referenceNumber.Trim());
                            ledgerCmd.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                            ledgerCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return paymentId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void AddMoneyParameter(SqlCommand command, string name, decimal value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = MoneyPrecision;
            parameter.Scale = MoneyScale;
            parameter.Value = Math.Round(value, MoneyScale);
        }

        private static string NormalizeCurrency(string currency)
        {
            string value = (currency ?? "USD").Trim().ToUpperInvariant();
            if (value == "L.L" || value == "LB" || value == "LBP") return "LBP";
            return "USD";
        }
    }
}
