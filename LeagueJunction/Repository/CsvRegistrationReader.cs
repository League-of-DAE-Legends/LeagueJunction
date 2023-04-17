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
		public string MainOpGG { get; set; }
		[Index(3)]
		public string RegionOpGG { get; set; }
		[Index(4)]
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
			using (var reader = new StreamReader(file_path))
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
                List<RawFormsAnswer> invalidAnswers = new List<RawFormsAnswer>();
                List<int> invalidIdxs = new List<int>();

				List<Player> players = new List<Player>();
				var rawAnswers = GetRawFormsAnswers(sourceFile);
				foreach (var rawAnswer in rawAnswers)
				{
                    // Essentials
                    DecryptLink(rawAnswer.MainOpGG, out string username, out string extractedRegion);
                    Region region = Region.EUW1;
					{
						var r = GetRegionCode(extractedRegion);
						if (r != null) 
						{
							region = r.Value;
						}
                    }

                    var player = new Player(username, region);
					players.Add(player);

                    // Display name
					if (!string.IsNullOrEmpty(rawAnswer.RegionOpGG))
					{
						if (rawAnswer.RegionOpGG.StartsWith("http") || rawAnswer.RegionOpGG.StartsWith("www.op.gg"))
						{
                            DecryptLink(rawAnswer.RegionOpGG, out string displayName, out string thisReg);
                            player.Displayname = displayName;
						}
						else
						{
							Console.WriteLine("invalid regionopgg link");
                            invalidAnswers.Add(rawAnswer);
                            invalidIdxs.Add(rawAnswers.FindIndex((r) => r.Equals(rawAnswer)));
						}
					}

                    // Optional, don't care if it fails
                    try
                    {
                        player.Contact = rawAnswer.DiscordUsername;
                    }
                    catch (Exception e) { }
				}
                
                // Invalid answers
                if (invalidAnswers.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < invalidAnswers.Count; ++i) 
                    {
                        sb.Append("Line ");
                        sb.Append(invalidIdxs[i]);
                        sb.Append(": ");
                        sb.Append(invalidAnswers[i].RegionOpGG);
                        sb.Append("\n");
                    }
                    sb.Append("Players are still added but display name or main username could be incorrect.");
                    MessageBox.Show($"{invalidAnswers.Count} invalid regionOpGG links:\n{sb}");
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

		private Region? GetRegionCode(string strRegion)
		{
            Region? region = null;
            switch (strRegion)
            {
                case "na":
                    region = Region.NA1;
                    break;
                case "euw":
                    region = Region.EUW1;
                    break;
                case "eune":
                    region = Region.EUN1;
                    break;
                case "oce":
                    region = Region.OC1;
                    break;
                case "kr":
                    region = Region.KR;
                    break;
                case "jp":
                    region = Region.JP1;
                    break;
                case "br":
                    region = Region.BR1;
                    break;
                case "las":
                case "lan":
                    region = Region.LA1;
                    break;
                case "ru":
                    region = Region.RU;
                    break;
                case "tr":
                    region = Region.TR1;
                    break;
                case "sg":
                    region = Region.SG2;
                    break;
                case "ph":
                    region = Region.PH2;
                    break;
                case "tw":
                    region = Region.TW2;
                    break;
                case "vn":
                    region = Region.VN2;
                    break;
                case "th":
                    region = Region.TH2;
                    break;
            }
			return region;
        }

		private void DecryptLink(string url, out string username, out string region)
		{
            string textAfterSummoners = url.Substring(url.IndexOf("summoners/") + 10);
            string[] parts = textAfterSummoners.Split('/');
            region = parts[0];
            username = System.Web.HttpUtility.UrlDecode(parts[1]);
        }
    }
}
