using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Resources;
using System.Windows;
using LeagueJunction.View;
using LeagueJunction.Repository;
using LeagueJunction.Model;


namespace LeagueJunction.ViewModel
{
    public class BalanceVM : ObservableObject
    {
        //API Repo
        PlayerAPIRepository _playerApiRepo = null;
        IPlayerRepository PlayerRepository { get; set; } = new CsvRegistrationReader();
        ITeamNamesRepository TeamNamesRepository { get; set; } = new CsvTeamNamesRepository("../../Resources/SubFactions.csv");

        // Userdata
        public List<Player> Players { get; set; }

        private Player _selectedPlayer;
        public Player SelectedPlayer 
        { 
            get { return _selectedPlayer; }
            set
            {
                _selectedPlayer = value;
                OnPropertyChanged(nameof(SelectedPlayer));
            }
        }

        // Derivative
        public List<Team> Teams { get; set; }

        // Class data

        public string SelectedFileName { get; set; }

        private Team _selectedTeam;
        public Team SelectedTeam
        {
            get { return _selectedTeam; }

            set
            {
                _selectedTeam = value;
                OnPropertyChanged(nameof(SelectedTeam));
            }
        }

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
        public RelayCommand RegenerateTeamsCommand { get; private set; }
        public RelayCommand PostToDiscordCommand { get; private set; }
        public RelayCommand PostToDiscordCallBackCommand { get; private set; }
        public RelayCommand SavePlayerCommand { get;private set; }
        public RelayCommand SelectedTeamCommand { get; private set; }
        public bool IsGenerateTeamsCommandEnabled { get; private set; }
        public bool IsRegenerateTeamsCommandEnabled { get; private set; }
        private bool _shouldCallAPI = true;

        public BalanceVM()
        {
            SelectFileCommand = new RelayCommand(SelectFileDialog);
            GenerateTeamsCommand = new RelayCommand(GenerateTeams);
            RegenerateTeamsCommand = new RelayCommand(RegenerateTeams);
            IsGenerateTeamsCommandEnabled = false;
            IsRegenerateTeamsCommandEnabled = false;
            PostToDiscordCommand = new RelayCommand(PostToDiscord);
            PostToDiscordCallBackCommand = new RelayCommand(PostToDiscordCallBack);
            SavePlayerCommand = new RelayCommand(SavePlayer);

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
                
                //New file so we should call the API
                _shouldCallAPI = true;
            }

        }

        private void GenerateTeams()
        {
            if (string.IsNullOrEmpty(SelectedFileName))
            {
                TempMessage = "No file selected.";
                return;
            }
            
            if (_shouldCallAPI = false)
            {
                //No point in doing API calls, we already have all the info
                return;
            }
            TempMessage = "Loading...";
            
            Players = PlayerRepository.GetPlayers(SelectedFileName);
            OnPropertyChanged(nameof(Players));

            //API section

            FillPlayerInfoAsync(Players);
            _shouldCallAPI = false;
            
            IsRegenerateTeamsCommandEnabled = true;
            OnPropertyChanged(nameof(IsRegenerateTeamsCommandEnabled));
        }

        private void RegenerateTeams()
        {
            if (Teams.Count == 0 || Players.Count % 5 !=0)
            {
                MessageBox.Show("Cannot regen sori");
                return;
            }
           
            
            IsRegenerateTeamsCommandEnabled = false;
            OnPropertyChanged(nameof(IsRegenerateTeamsCommandEnabled));
            
            Teams = Team.SplitIntoTeams(Players,Team.Algorithm.Greedy,false);
            OnPropertyChanged(nameof(Teams));
            RandomiseTeamNames();
            
            IsRegenerateTeamsCommandEnabled = true;
            OnPropertyChanged(nameof(IsRegenerateTeamsCommandEnabled));

        }

        private void PostToDiscord()
        {
            StringBuilder str = new StringBuilder();
            foreach (var team in Teams) 
            {
                if (team != null) 
                {
                    str.Append(team.DiscordFormat());
                    str.Append("\n");
                }
            }
            PostToDiscordVM.SetPreviewText(str.ToString());
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
                Teams = Team.SplitIntoTeams(Players,Team.Algorithm.Greedy,true);
                RandomiseTeamNames();
                OnPropertyChanged(nameof(Teams));
                IsRegenerateTeamsCommandEnabled = true;
                TempMessage = "League API repos calls complete";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                MessageBox.Show("Finished API calls");
            }
        }

        private void RandomiseTeamNames()
        {
            if (TeamNamesRepository != null)
            {
                foreach (var team in Teams)
                {
                    string str = TeamNamesRepository.GetNextTeamName();
                    if (str != null)
                    {
                        team.TeamName = str;
                    }
                }
            }
        }

        private void SavePlayer()
        {
            
        }
    }
}
