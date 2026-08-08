using System;
using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    public class CashSessionService
    {
        private readonly string connectionString;
        public CashSessionService(string connectionString) { this.connectionString = connectionString; }

        public int EnsureRegister(string computerName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TOP 1 register_id FROM dbo.CashRegisters WHERE computer_name=@computer AND is_active=1", conn))
                {
                    cmd.Parameters.Add("@computer", SqlDbType.NVarChar, 150).Value = computerName;
                    object value = cmd.ExecuteScalar();
                    if (value != null && value != DBNull.Value) return Convert.ToInt32(value);
                }
                using (var cmd = new SqlCommand(@"INSERT dbo.CashRegisters(name,computer_name,location,is_active) VALUES(@name,@computer,@location,1); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
                {
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = "Register - " + computerName;
                    cmd.Parameters.Add("@computer", SqlDbType.NVarChar, 150).Value = computerName;
                    cmd.Parameters.Add("@location", SqlDbType.NVarChar, 250).Value = computerName;
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int? GetOpenSessionId(int registerId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT TOP 1 cash_session_id FROM dbo.CashSessions WHERE register_id=@r AND status=N'OPEN' ORDER BY opened_at DESC", conn))
            {
                cmd.Parameters.Add("@r", SqlDbType.Int).Value = registerId;
                conn.Open(); object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
            }
        }

        public int OpenSession(int registerId, int userId, decimal openingUsd, decimal openingLbp)
        {
            if (GetOpenSessionId(registerId).HasValue) throw new InvalidOperationException("This register already has an open shift.");
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"INSERT dbo.CashSessions(register_id,user_id,opening_usd,opening_lbp,status) VALUES(@r,@u,@usd,@lbp,N'OPEN'); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
            {
                cmd.Parameters.Add("@r", SqlDbType.Int).Value = registerId;
                cmd.Parameters.Add("@u", SqlDbType.Int).Value = userId;
                Money(cmd,"@usd",openingUsd); Money(cmd,"@lbp",openingLbp);
                conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void RecordMovement(int sessionId, string type, decimal amount, string currency, string referenceType, int? referenceId, string description, int userId)
        {
            if (amount < 0) throw new InvalidOperationException("Movement amount cannot be negative.");
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id) VALUES(@s,@t,@a,@c,@rt,@ri,@d,@u)", conn))
            {
                cmd.Parameters.Add("@s",SqlDbType.Int).Value=sessionId;
                cmd.Parameters.Add("@t",SqlDbType.NVarChar,30).Value=type;
                Money(cmd,"@a",amount);
                cmd.Parameters.Add("@c",SqlDbType.NVarChar,10).Value=currency;
                cmd.Parameters.Add("@rt",SqlDbType.NVarChar,30).Value=string.IsNullOrWhiteSpace(referenceType)?(object)DBNull.Value:referenceType;
                cmd.Parameters.Add("@ri",SqlDbType.Int).Value=referenceId.HasValue?(object)referenceId.Value:DBNull.Value;
                cmd.Parameters.Add("@d",SqlDbType.NVarChar,1000).Value=string.IsNullOrWhiteSpace(description)?(object)DBNull.Value:description;
                cmd.Parameters.Add("@u",SqlDbType.Int).Value=userId;
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void GetExpected(int sessionId, out decimal usd, out decimal lbp)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT s.opening_usd + ISNULL(SUM(CASE WHEN m.currency=N'USD' THEN CASE WHEN m.movement_type IN(N'REFUND',N'EXPENSE',N'WITHDRAWAL') THEN -m.amount ELSE m.amount END ELSE 0 END),0),
       s.opening_lbp + ISNULL(SUM(CASE WHEN m.currency=N'LBP' THEN CASE WHEN m.movement_type IN(N'REFUND',N'EXPENSE',N'WITHDRAWAL') THEN -m.amount ELSE m.amount END ELSE 0 END),0)
FROM dbo.CashSessions s LEFT JOIN dbo.CashMovements m ON m.cash_session_id=s.cash_session_id WHERE s.cash_session_id=@s GROUP BY s.opening_usd,s.opening_lbp", conn))
            {
                cmd.Parameters.Add("@s",SqlDbType.Int).Value=sessionId; conn.Open();
                using(var r=cmd.ExecuteReader()){ if(!r.Read()) throw new InvalidOperationException("Shift not found."); usd=r.GetDecimal(0); lbp=r.GetDecimal(1); }
            }
        }

        public void CloseSession(int sessionId, decimal actualUsd, decimal actualLbp, string notes)
        {
            GetExpected(sessionId,out decimal expectedUsd,out decimal expectedLbp);
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"UPDATE dbo.CashSessions SET closed_at=SYSUTCDATETIME(),expected_usd=@eu,expected_lbp=@el,actual_usd=@au,actual_lbp=@al,difference_usd=@du,difference_lbp=@dl,status=N'CLOSED',notes=@n WHERE cash_session_id=@s AND status=N'OPEN'", conn))
            {
                cmd.Parameters.Add("@s",SqlDbType.Int).Value=sessionId;
                Money(cmd,"@eu",expectedUsd);Money(cmd,"@el",expectedLbp);Money(cmd,"@au",actualUsd);Money(cmd,"@al",actualLbp);Money(cmd,"@du",actualUsd-expectedUsd);Money(cmd,"@dl",actualLbp-expectedLbp);
                cmd.Parameters.Add("@n",SqlDbType.NVarChar,1000).Value=string.IsNullOrWhiteSpace(notes)?(object)DBNull.Value:notes;
                conn.Open(); if(cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Open shift not found.");
            }
        }

        private static void Money(SqlCommand cmd,string name,decimal value){var p=cmd.Parameters.Add(name,SqlDbType.Decimal);p.Precision=24;p.Scale=8;p.Value=value;}
    }
}
