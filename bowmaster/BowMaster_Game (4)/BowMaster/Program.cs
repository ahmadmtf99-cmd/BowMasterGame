using System;
using System.Windows.Forms;

namespace BowMaster
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MenuForm menu = new MenuForm();
            if (menu.ShowDialog() == DialogResult.OK)
                Application.Run(new GameForm());
        }
    }
}
