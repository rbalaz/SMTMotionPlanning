using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMTMotionPlanning
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PlanningForm form = new PlanningForm();
            form.Width = 640;
            form.Height = 480;
            form.Text = "Motion Planning with SMT";
            Application.Run(form);
        }
    }
}
