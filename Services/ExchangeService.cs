namespace Pos_System.Services
{
    internal static class ExchangeService
    {
        public static decimal Rate()
        {
            return POS_System.Program.SettingsManager.GetExchangeRate();
        }
    }
}
