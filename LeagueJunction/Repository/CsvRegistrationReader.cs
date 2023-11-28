using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using LeagueJunction.Model;
using System.Web;
using System.Security.Policy;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;

namespace LeagueJunction.Repository
{
    public class RawFormsAnswer
    {
		[Index(0)]
		public string Timestamp { get; set; }
		[Index(1)]
		public string DiscordUsername { get; set; }
		[Index(2)]
		public string RiotID { get; set; }
		[Index(3)]
		public string PreferredRoles { get; set; }
    }


    public class CsvRegistrationReader : IPlayerRepository
    {
        static public List<RawFormsAnswer> GetRawFormsAnswers(string file_path)
        {
			// For some reason some lines give baddata
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				BadDataFound = null
			};
			using (var reader = new StreamReader(file_path,Encoding.GetEncoding("ISO-8859-1")))
			using (var csv = new CsvReader(reader, config))
			{
				var records = csv.GetRecords<RawFormsAnswer>().ToList();
                return records;
			}
		}

        public List<Player> GetPlayers(string sourceFile)
		{
			try
			{
				List<Player> players = new List<Player>();
				var rawAnswers = GetRawFormsAnswers(sourceFile);
				foreach (var rawAnswer in rawAnswers)
				{
					string[] fullRiotId = rawAnswer.RiotID.Split('#');
					if (fullRiotId.Length !=2)
					{
						throw new Exception($"Riot ID for {rawAnswer.DiscordUsername} has incorrect format");
					}
                    var player = new Player(fullRiotId[0], fullRiotId[1]);
					players.Add(player);
					
					//Preferred role
					FillPreferredRoles(rawAnswer.PreferredRoles,player);
					
                    // Optional, don't care if it fails
                    try
                    {
                        player.Contact = rawAnswer.DiscordUsername;
                    }
                    catch (Exception) { }
				}
				
				return players;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
                MessageBox.Show($"[Exception in CsvRegistrationReader]\n {ex.Message}");
#if DEBUG
				throw ex;
#else
				return null;
#endif
			}
        }


        private void FillPreferredRoles(string rawAnswer, Player player)
        {
	        string[] prefRoles = rawAnswer.Split(';');
	        player.PreferedRoles = new PreferedRoles();
	        foreach (var role in prefRoles)
	        {
		        if (role.ToUpper().Equals("TOP"))
		        {
			        player.PreferedRoles.Top = true;
		        }
		        if (role.ToUpper().Equals("JNGL"))
		        {
			        player.PreferedRoles.Jngl = true;
		        }
		        if (role.ToUpper().Equals("MID"))
		        {
			        player.PreferedRoles.Mid = true;
		        }
		        if (role.ToUpper().Equals("ADC"))
		        {
			        player.PreferedRoles.Adc = true;
		        }
		        if (role.ToUpper().Equals("SUPPORT"))
		        {
			        player.PreferedRoles.Support = true;
		        }
		        if (role.ToUpper().Equals("FILL"))
		        {
			        player.PreferedRoles.Fill = true;
		        }
	        }

        }
		
		
    }
}
