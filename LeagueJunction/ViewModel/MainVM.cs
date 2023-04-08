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
using LeagueJunction.View;

namespace LeagueJunction.ViewModel
{
    public class MainVM : ObservableObject
    {
        // Data

        // Communication with view
        public string WindowTitle { get; private set; }

        public BalancingView BalancePage { get; private set; } = new BalancingView();

        public RelayCommand OpenGithubReposCommand { get; private set; }

        public MainVM()
        {
            WindowTitle = $"League Junction v0.0.1";

            OpenGithubReposCommand = new RelayCommand(OpenGithubRepos);
        }

        // Proxy

        private void OpenGithubRepos()
        {
            Process.Start(new ProcessStartInfo("https://github.com/ReiMessely/LeagueJunction"));
        }
    }
}
