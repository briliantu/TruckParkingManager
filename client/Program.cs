using System;
using System.Windows.Forms;

namespace TruckParkingManager
{
    static class Program
    {
        
        /// Punctul principal de intrare pentru aplicație.
       
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