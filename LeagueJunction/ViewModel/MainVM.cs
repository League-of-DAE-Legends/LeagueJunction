using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace LeagueJunction.ViewModel
{
    public class MainVM : ObservableObject
    {
        public string WindowTitle { get; private set; }
        public Color BackgroundColor { get; private set; }

        public RelayCommand OpenGithubReposCommand { get; private set; }

        public MainVM()
        {
            WindowTitle = $"League Junction v0.0.1";

            OpenGithubReposCommand = new RelayCommand(OpenGithubRepos);
        }

        private void OpenGithubRepos()
        {
            Process.Start(new ProcessStartInfo("https://github.com/ReiMessely/LeagueJunction"));
        }
    }
}
