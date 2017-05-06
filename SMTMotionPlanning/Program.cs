using System;
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
            form.Width = 672;
            form.Height = 556;
            form.Text = "Motion Planning with SMT";
            Application.Run(form);
        }
    }
}
