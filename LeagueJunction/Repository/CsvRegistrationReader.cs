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
			List<Player> players = new List<Player>();
			var rawAnswers = GetRawFormsAnswers(sourceFile);
			foreach (var rawAnswer in rawAnswers) 
			{
				//var player = new Player();
                //players.Add(player);
			}
			return players;
        }

    }
}
