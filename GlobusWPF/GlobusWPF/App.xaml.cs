using System.Windows;
using GlobusWPF.Models;

namespace GlobusWPF
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
    }
}