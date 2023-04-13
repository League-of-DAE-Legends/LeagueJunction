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
using CsvHelper;
using LeagueJunction.Repository;
using LeagueJunction.Model;
using System.Diagnostics;

namespace LeagueJunction.ViewModel
{
    public class BalanceVM : ObservableObject
    {
        //API Repo
        PlayerAPIRepository _playerApiRepo = null;

        // Userdata
        public List<RawFormsAnswer> RawFormsAnswers { get; set; }

        // Derivative
        public List<Team> Teams { get; set; }

        // Class data
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

            _playerApiRepo = new PlayerAPIRepository();

            var resourceManager = new ResourceManager("LeagueJunction.Resources.Tokens", typeof(BalanceVM).Assembly);
            var apiKey = resourceManager.GetString("api_key");
            _playerApiRepo.ApiKey = apiKey;
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
            if (string.IsNullOrEmpty(SelectedFileName))
            {
                TempMessage = "No file selected.";
                return;
            }

            TempMessage = "Loading...";

            RawFormsAnswers = CsvRegistrationReader.GetRawFormsAnswers(SelectedFileName);
            OnPropertyChanged(nameof(RawFormsAnswers));

            TempMessage = "Loaded players.";

            //API section

            Debug.Assert(false, "Still using temp player list to pull data from API");
            Player player1 = new Player("TTT Alternative", Region.EUW1);
            Player player2 = new Player("TTT Wardergrip", Region.EUW1);
            List<Player> players = new List<Player>();
            players.Add(player1);
            players.Add(player2);

            FillPlayerInfoAsync(players);

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

        private async void FillPlayerInfoAsync(List<Player> players)
        {
            try
            {
                await _playerApiRepo.TryFillPlayerInfoAsync(players);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
