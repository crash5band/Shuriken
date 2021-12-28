using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.ViewModels
{
    internal class AboutViewModel : ViewModelBase
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string GithubLink => "https://github.com/crash5band/Shuriken";

        public AboutViewModel()
        {
            DisplayName = "About";
            IconCode = "\xf05a";
        }
    }
}
