using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TaskamUI.UI.Views;

namespace Taskam
{
    internal class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DashboardWindow window = new();
            window.Title = "Taskam";
            window.ShowDialog();
        }
    }
}
 