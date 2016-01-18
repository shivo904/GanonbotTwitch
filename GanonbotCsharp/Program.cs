using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GanonbotCsharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static MainMenu mainForm;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new MainMenu();
            Application.Run(mainForm);
        }
    }
}
