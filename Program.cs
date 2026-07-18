using System;
using System.Windows.Forms;

namespace ParcareCamioane
{
    static class Program
    {
        /// <summary>
        /// Punctul principal de intrare pentru aplicație.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}