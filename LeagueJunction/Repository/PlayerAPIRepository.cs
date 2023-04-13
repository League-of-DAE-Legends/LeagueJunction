using LeagueJunction.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueJunction.Repository
{
    public class PlayerAPIRepository
    {
        //To avoid having Too Many Request exceptions
        private int _delayBetweenRequestsMs = 1000;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(15, 15);
        public string ApiKey { get; set; }


        /// <summary>
        /// Makes API calls for each player and tries to fill all the necessary data from RIOT API.
        /// Might throw exceptions if too many requests are made.
        /// Use this in try catch block and handle exceptions
        /// </summary>
        public async Task TryFillPlayerInfoAsync(List<Player> players)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", ApiKey);
                List<Task> parallelTasks = new List<Task>();
                try
                {
                    foreach (var player in players)
                    {
                       parallelTasks.Add(LoadPlayerIDAsync(player,client));
                    }

                    //Wait untill every player's encrypted ID is loaded 
                    await Task.WhenAll(parallelTasks);
                    parallelTasks.Clear();

                    //If the first API calls fail, the next one will fail too
                    foreach (var player in players)
                    {
                        parallelTasks.Add(LoadPlayerRankInfoAsync(player, client));
                    }
                    await Task.WhenAll(parallelTasks);
                    parallelTasks.Clear();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// Makes API calls for 1 player and fills all the necessary data from RIOT API.
        /// Might throw exceptions
        /// Use this in try catch block and handle exceptions
        /// </summary>
        public async Task TryFillPlayerInfoAsync(Player outPlayer)
        {
            using(HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", ApiKey);

                try
                {
                    await LoadPlayerIDAsync(outPlayer, client);
                    await LoadPlayerRankInfoAsync(outPlayer, client);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
              
            }
        }
        

        /// <summary>
        /// Makes an API call to Summoner V-4 endpoint and fills in the EncSummonerID property of the player
        /// </summary>
        private async Task LoadPlayerIDAsync(Player outPlayer,HttpClient client)
        {
            string region = outPlayer.Region.ToString().ToLower();
            string endpoint = $"https://{region}.api.riotgames.com/lol/summoner/v4/summoners/by-name/{outPlayer.MainUsername}";
           
            try
            {
                //Wait untill there is a free thread
                await _semaphore.WaitAsync();

                //Communicate with the API
                HttpResponseMessage response = await client.GetAsync(endpoint);

                //Wait untill processing
                await Task.Delay(_delayBetweenRequestsMs);

                if (!response.IsSuccessStatusCode)
                {
                    string message = $"Execption thrown from Summoner-V4 endpoint. Player: {outPlayer.MainUsername}. API Response: {response.ReasonPhrase}";
                    throw new Exception(message);
                }

                string json = await response.Content.ReadAsStringAsync();
                JObject summonerObject = JObject.Parse(json);
                outPlayer.EncSummonerId = (string)summonerObject["id"];
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Free the thread 
                _semaphore.Release();
            }

        }

        /// <summary>
        /// Makes an API call to League V-4 endpoint and fills in the Rank and Tier properties of the player
        /// </summary>
        private async Task LoadPlayerRankInfoAsync(Player outPlayer,HttpClient client)
        {
            string region = outPlayer.Region.ToString().ToLower();
            string endpoint = $"https://{region}.api.riotgames.com/lol/league/v4/entries/by-summoner/{outPlayer.EncSummonerId}";

            try
            {
                //Wait untill there is a free thread
                await _semaphore.WaitAsync();

                //Communicate with the API
                HttpResponseMessage response = await client.GetAsync(endpoint);

                //Wait untill processing
                await Task.Delay(_delayBetweenRequestsMs);

                if (!response.IsSuccessStatusCode)
                {
                    string message = $"Execption thrown from League-V4 endpoint. Player: {outPlayer.MainUsername}. API Response: {response.ReasonPhrase}";
                    throw new Exception(message);
                }

                string json = await response.Content.ReadAsStringAsync();
           
                JArray ranks = JArray.Parse(json);

                foreach (JToken rank in ranks)
                {
                    string queueType = rank["queueType"].ToString();
                    string tier = rank["tier"].ToString();
                    string rankString = rank["rank"].ToString();

                    if (queueType == "RANKED_SOLO_5x5")
                    {
                        outPlayer.SoloRank = rankString;
                        outPlayer.SoloTier = tier;
                    }
                    else if (queueType == "RANKED_FLEX_SR")
                    {
                        outPlayer.FlexRank = rankString;
                        outPlayer.FlexTier = tier;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _semaphore.Release();
            }
        }
   
    }
}
