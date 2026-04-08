using Microsoft.EntityFrameworkCore;
using Pos_System;
using Pos_System.Data;  // your DbContext namespace
using Pos_System.Forms;
using System;
using System.Windows.Forms;

namespace POS_System
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configure EF Core DbContext
            var options = new DbContextOptionsBuilder<POSDbContext>()
                .UseSqlServer(@"Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;")
                .Options;

            var context = new POSDbContext(options);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Pass DbContext into your first form (LoginForm)
            Application.Run(new LoginForm(context));
           // Application.Run(new Dashboard());
        }
    }
}