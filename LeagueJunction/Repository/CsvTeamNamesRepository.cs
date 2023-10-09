using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueJunction.Repository
{
    public class CsvTeamNamesRepository : ITeamNamesRepository
    {
        private class TeamName { public string Name { get; set; } }

        private List<TeamName> _teamNames;
        private int _teamNameIdx = -1;
        private string SourceFile { get; set; }


        public CsvTeamNamesRepository(string sourceFile) 
        { 
            SourceFile = sourceFile;
        }
        public string GetNextTeamName()
        {
            if (SourceFile == null)
            {
                Console.WriteLine($"SourceFile is null!");
                throw new Exception("Source file is null");
            }
            try
            {
                if (_teamNames == null)
                {
                    _teamNames = new List<TeamName>();

                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false,
                        BadDataFound = null
                    };
                    using (var reader = new StreamReader(SourceFile))
                    using (var csv = new CsvReader(reader, config))
                    {
                        _teamNames = csv.GetRecords<TeamName>().ToList();
                    }
                    HelperFuncts.Shuffle(_teamNames);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Something went wrong when retrieving the team names from the csv file. {ex.Message}");
                return null;
            }

            return _teamNames[(++_teamNameIdx % _teamNames.Count)].Name;
        }
    }
}
