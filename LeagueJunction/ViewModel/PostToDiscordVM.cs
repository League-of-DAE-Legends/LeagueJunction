using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LeagueJunction.ViewModel
{
    public class PostToDiscordVM : ObservableObject
    {
        // Statics
        private static string _previewText = "<empty>";
        public static void SetPreviewText(string previewText) { _previewText = previewText; }
        public string PreviewText 
        { 
            get
            {
                return _previewText;
            }
            set
            {
                _previewText = value;
                OnPropertyChanged(nameof(PreviewText));
            }
        }

        // Non statics
        public Window ThisWindow { get; set; }
        public RelayCommand CallBackCommand { get; set; }

        public RelayCommand PostToDevDiscordCommand { get; private set; }
        public RelayCommand PostToPublicDiscordCommand { get; private set; }

        public PostToDiscordVM()
        {
            PostToDevDiscordCommand = new RelayCommand(PostToDevDiscord);
            PostToPublicDiscordCommand = new RelayCommand(PostToPublicDiscord);
        }

        private async void PostToDevDiscord()
        {
            if (PreviewText.Equals("<empty>"))
            {
                return;
            }

            var resourceManager = new ResourceManager("LeagueJunction.Resources.Tokens", typeof(BalanceVM).Assembly);
            var webhooklink = resourceManager.GetString("dev_webhook");
            var messageid = ulong.Parse(resourceManager.GetString("dev_messageid"));
            DiscordWebhookClient webhook = new DiscordWebhookClient(webhooklink);
            await webhook.ModifyMessageAsync(messageid, x =>
            {
                x.Content = PreviewText;
            });

            if (CallBackCommand != null)
            {
                CallBackCommand.Execute(this);
            }
            ThisWindow.Close();
        }

        private async void PostToPublicDiscord()
        {
            if (PreviewText.Equals("<empty>"))
            {
                return;
            }

            var resourceManager = new ResourceManager("LeagueJunction.Resources.Tokens", typeof(BalanceVM).Assembly);
            var webhooklink = resourceManager.GetString("public_webhook");
            var messageid = ulong.Parse(resourceManager.GetString("public_messageid"));
            DiscordWebhookClient webhook = new DiscordWebhookClient(webhooklink);
            await webhook.ModifyMessageAsync(messageid, x =>
            {
                x.Content = PreviewText;
            });

            if (CallBackCommand != null)
            {
                CallBackCommand.Execute(this);
            }
            ThisWindow.Close();
        }
    }
}
