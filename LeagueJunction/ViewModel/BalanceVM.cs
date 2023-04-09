using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Webhook;
using System.Resources;
using System.Windows;
using LeagueJunction.View;

namespace LeagueJunction.ViewModel
{
    public class BalanceVM : ObservableObject
    {
        public string SelectedFileName { get; set; }

        private string _tempMessage;
        public string TempMessage 
        { 
            get
            {
                return _tempMessage;
            }
            set
            {
                _tempMessage = value;
                OnPropertyChanged(nameof(TempMessage));
            }
        }

        public RelayCommand SelectFileCommand { get; private set; }
        public RelayCommand GenerateTeamsCommand { get; private set; }
        public RelayCommand PostToDiscordCommand { get; private set; }
        public RelayCommand PostToDiscordCallBackCommand { get; private set; }
        public bool IsGenerateTeamsCommandEnabled { get; private set; }

        public BalanceVM()
        {
            SelectFileCommand = new RelayCommand(SelectFileDialog);
            GenerateTeamsCommand = new RelayCommand(GenerateTeams);
            IsGenerateTeamsCommandEnabled = false;
            PostToDiscordCommand = new RelayCommand(PostToDiscord);
            PostToDiscordCallBackCommand = new RelayCommand(PostToDiscordCallBack);
        }

        // Proxy
        private void SelectFileDialog()
        {
            // Source: https://wpf-tutorial.com/dialogs/the-openfiledialog/

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Csv files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFileName = openFileDialog.FileName;
                Console.WriteLine(SelectedFileName);
                IsGenerateTeamsCommandEnabled = true;
                OnPropertyChanged(nameof(IsGenerateTeamsCommandEnabled));
                OnPropertyChanged(nameof(SelectedFileName));
            }

        }

        private void GenerateTeams()
        {
            TempMessage = "Currently an useless button";
        }

        private void PostToDiscord()
        {
            PostToDiscordVM.SetPreviewText($"This is a test message\n\nEditted on {DateTime.Now} by {Environment.UserName}");
            PostToDiscordVM postToDiscordVM = new PostToDiscordVM();
            Window postDiscordWindow = new PostToDiscordWindow { DataContext = postToDiscordVM };
            postToDiscordVM.ThisWindow = postDiscordWindow;
            postToDiscordVM.CallBackCommand = PostToDiscordCallBackCommand;

            postDiscordWindow.Show();
        }

        private void PostToDiscordCallBack()
        {
            TempMessage = "Message posted.";
        }
    }
}
