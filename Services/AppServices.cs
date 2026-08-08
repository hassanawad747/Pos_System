using Microsoft.Extensions.DependencyInjection;
using Pos_System.Forms;
using System;

namespace Pos_System.Services
{
    internal static class AppServices
    {
        private static readonly object Sync = new object();
        private static IServiceProvider provider;

        public static T Get<T>() where T : class
        {
            EnsureBuilt();
            return provider.GetRequiredService<T>();
        }

        public static void Reset()
        {
            lock (Sync)
            {
                IDisposable disposable = provider as IDisposable;
                if (disposable != null) disposable.Dispose();
                provider = null;
            }
        }

        private static void EnsureBuilt()
        {
            if (provider != null) return;
            lock (Sync)
            {
                if (provider != null) return;
                string cs = POS_System.Program.SettingsManager.ConnectionString;
                var services = new ServiceCollection();
                services.AddSingleton(new SalesLifecycleService(cs));
                services.AddSingleton(new InventoryOperationsService(cs));
                services.AddSingleton(new StockLossService(cs));
                services.AddSingleton(new PricingService(cs));
                services.AddSingleton(new BusinessIntelligenceService(cs));
                services.AddSingleton(new WhishPaymentService());
                services.AddTransient<SalesLifecycleForm>();
                services.AddTransient<InventoryOperationsForm>();
                services.AddTransient<StockLossForm>();
                services.AddTransient<PricingAdminForm>();
                services.AddTransient<BusinessIntelligenceForm>();
                services.AddTransient<SecurityAdminForm>();
                provider = services.BuildServiceProvider();
            }
        }
    }
}
