using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class ReceiptPrinterService
    {
        public static void ShowPreview(PrintPreviewDialog preview, IWin32Window owner)
        {
            if (preview == null)
            {
                return;
            }

            preview.ShowDialog(owner);
        }
    }
}
