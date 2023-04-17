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
				List<Player> players = new List<Player>();
				var rawAnswers = GetRawFormsAnswers(sourceFile);
				foreach (var rawAnswer in rawAnswers)
				{
					string url = rawAnswer.MainOpGG;
					string textAfterSummoners = url.Substring(url.IndexOf("summoners/") + 10);
					string[] parts = textAfterSummoners.Split('/');
					string extractedRegion = parts[0];
					string username = parts[1];
					username = System.Web.HttpUtility.UrlDecode(username);
					Region region = Region.EUW1;
					switch (extractedRegion)
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
					var player = new Player(username, region);
					if (!string.IsNullOrEmpty(rawAnswer.RegionOpGG))
					{
						if (rawAnswer.RegionOpGG.StartsWith("http") || rawAnswer.RegionOpGG.StartsWith("www.op.gg"))
						{
							url = rawAnswer.RegionOpGG;
							string tas = url.Substring(url.IndexOf("summoners/") + 10);
							string[] pts = tas.Split('/');
							string reg = pts[0];
							string user = pts[1];
							player.Displayname = user;
						}
						else
						{
							Console.WriteLine("invalid regionopgg link");
						}
					}
					players.Add(player);
				}
				return players;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				using (StreamWriter writer = new StreamWriter("../../csvRegistrationLog.txt"))
				{
					writer.WriteLine($"{DateTime.Now} Log file for CsvRegistrationReader");
					writer.WriteLine(ex.Message);
				}
#if DEBUG
				throw ex;
#else
				return null;
#endif
			}
        }

    }
}
