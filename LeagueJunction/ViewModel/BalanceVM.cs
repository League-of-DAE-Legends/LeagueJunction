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

namespace LeagueJunction.ViewModel
{
    public class BalanceVM : ObservableObject
    {
        private string SelectedFileName { get; set; }

        public RelayCommand SelectFileCommand { get; private set; }
        public RelayCommand GenerateTeamsCommand { get; private set; }
        public RelayCommand PostToDiscordCommand { get; private set; }
        public bool IsGenerateTeamsCommandEnabled { get; private set; }

        public BalanceVM()
        {
            SelectFileCommand = new RelayCommand(SelectFileDialog);
            GenerateTeamsCommand = new RelayCommand(GenerateTeams);
            IsGenerateTeamsCommandEnabled = false;
            PostToDiscordCommand = new RelayCommand(PostToDiscord);
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
            }

        }

        private void GenerateTeams()
        {

        }

        private async void PostToDiscord()
        {
            var resourceManager = new ResourceManager("LeagueJunction.Resources.Tokens", typeof(BalanceVM).Assembly);
            var webhooklink = resourceManager.GetString("dev_webhook");
            var messageid = ulong.Parse(resourceManager.GetString("dev_messageid"));
            DiscordWebhookClient webhook = new DiscordWebhookClient(webhooklink);
            await webhook.ModifyMessageAsync(messageid, x =>
            {
                x.Content = $"This is a test message editted with discord.NET\n\nLast edit on: {DateTime.Now} by {Environment.UserName}";
            });
        }
    }
}
