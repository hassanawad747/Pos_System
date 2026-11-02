namespace Pos_System.Services
{
    internal static class SalePaymentService
    {
        public static decimal Change(decimal paid, decimal total)
        {
            return paid - total;
        }
    }
}
