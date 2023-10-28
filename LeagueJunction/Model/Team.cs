using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LeagueJunction.Model
{
    public sealed class Team
    {
        // +=============== NON-STATICS ===============+
        // Data
        public int MaxTeamSize { get; set; }
        
        public List<Player> Players { get; set; } = new List<Player>();

        public string TeamName { get; set; }
        
        public uint? TeamAverageMMR
        {
            get { return AverageMMR(); }
        }
        
        Dictionary<string, int> _laneCounts = new Dictionary<string, int>
        {
            { "top", 0 },
            { "mid", 0 },
            { "jngl", 0 },
            { "adc", 0 },
            { "support", 0 },
        };

        public Dictionary<string, int> LaneCounts
        {
            get { return _laneCounts; }
        }
        
        // Constructor
        public Team(int size = 5)
        {
            MaxTeamSize = size;
            if (string.IsNullOrEmpty(TeamName))
            {
                ReturnToDefaultTeamName();
            }
        }

        // Methods
        public void AddPlayer(Player p)
        {
            if (Players.Count < MaxTeamSize)
            {
                UpdateCoveredLanes(p.PreferedRoles);
                Players.Add(p);
                return;
            }
            int emptyIndex = Players.FindIndex(item => item == null);
            if (emptyIndex == -1)
            {
                throw new Exception("Failed to add player to team.");
            }
            Players.Insert(emptyIndex, p);
            UpdateCoveredLanes(p.PreferedRoles);
        }

        private void UpdateCoveredLanes(PreferedRoles preferedRoles)
        {
            if (preferedRoles.Fill)
            {
                _laneCounts["top"]++;
                _laneCounts["jngl"]++;
                _laneCounts["mid"]++;
                _laneCounts["adc"]++;
                _laneCounts["support"]++;
                return;
            }
            if (preferedRoles.Top)
            {
                _laneCounts["top"]++;
            }
            if (preferedRoles.Jngl)
            {
                _laneCounts["jngl"]++;
            }
            if (preferedRoles.Mid)
            {
                _laneCounts["mid"]++;
            }
            if (preferedRoles.Adc)
            {
                _laneCounts["adc"]++;
            }
            if (preferedRoles.Support)
            {
                _laneCounts["support"]++;
            }
            
        }

        bool AreAllRolesPicked()
        {
            foreach (var count in _laneCounts.Values)
            {
                if (count==0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool NeedsPlayers()
        {
            if (Players.Count < MaxTeamSize)
            {
                return true;
            }
            foreach (Player player in Players)
            {
                if (player == null)
                {
                    return true;
                }
            }
            return false;
        }

        public uint? TotalMMR()
        {
            if (Players.All(item => item == null))
            {
                return null;
            }
            uint mmr = 0;
            
            foreach (Player p in Players)
            {
                if (p != null)
                {
                    mmr += p.GetMMR();

                }
            }
            return mmr;
        }
        public uint? AverageMMR()
        {
            if (Players.All(item => item == null))
            {
                return null;
            }
            uint mmr = 0;
            uint count = 0;
            foreach (Player p in Players)
            {
                if (p != null)
                {
                    mmr += p.GetMMR();
                    count++;
                }
            }
            return mmr / count;
        }

        public void ReturnToDefaultTeamName()
        {
            ++_teamCounter;
            TeamName = $"Team {_teamCounter}";
        }
        public string DiscordFormat(bool showMmr = false,string bulletPoint = "- ", string prefix = "```\n", string suffix = "```\n")
        {
            StringBuilder message = new StringBuilder();
            message.Append("**__");
            message.Append(TeamName);
            if (showMmr)
            {
                var mmr = AverageMMR();
                message.Append($" {(mmr.HasValue ? mmr.Value : uint.MaxValue)}");
            }
            message.Append("__**");
            message.Append(prefix);
            foreach (Player player in Players)
            {
                message.Append(bulletPoint)
                    .Append(string.IsNullOrEmpty(player.Contact) ? player.Displayname : player.Contact)
                    .Append("\n");
            }
            message.Append(suffix);
            return message.ToString();
        }

        public override string ToString()
        {
            string s = TeamName;
            foreach(Player player in Players) 
            {
                s += " \n";
                s += player.Displayname;
            }
            return s;
        }


        // +================ STATICS ================+
        private static ushort _teamCounter = 0;
        private static Dictionary<int, List<Player>> _playersByAmountRoles;
        private static Dictionary<string, int> _roleCounts;
        private static long _teamTargetMMR = 0;
        public static List<Team> SplitIntoTeams(List<Player> players , bool shouldSort, int playersPerTeam = 5)
        {
            if (players == null) throw new Exception("Players is null!");
            
            if (players.Count % playersPerTeam != 0)
            {
                throw new unequal_player_divide();
            }

            
            
            //Group players in containers based on how many roles they can play
            _playersByAmountRoles = new Dictionary<int, List<Player>>();
            GroupPlayersByAmountPreferredRoles(_playersByAmountRoles,players);
               
            
            _roleCounts = new Dictionary<string, int>
            {
                { "top", 0 },
                { "jngl", 0 },
                { "mid", 0 },
                { "adc", 0 },
                { "support", 0 },
                { "fill", 0 }
            };
            //Count how many of each laners we have
            CountPreferredRoles(players);

            //Sort based on which roles have the most players (ascending)
            //Fill is always last 
            _roleCounts = _roleCounts.OrderBy(x => x.Key.Equals("fill") ? int.MaxValue : x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
            
            List<Team> teams = new List<Team>();
            for (int i = 0; i < players.Count / playersPerTeam; i++)
            {
                teams.Add(new Team(playersPerTeam));
            }

            _teamTargetMMR = players.Sum(player => player.GetMMR()) / teams.Count;
            
            // Sort from lowest rank to highest rank
            if (shouldSort)
            {
                foreach (var playersList in _playersByAmountRoles.Values)
                {
                    playersList.Sort((p1,p2) => p1.GetMMR().CompareTo(p2.GetMMR()));
                }
                
            }

            
            
            Comparison<Team> isT2BetterthanT1 = (t1, t2) =>
            {
                var t2AvaMmr = t2.AverageMMR();
                var t1AvaMmr = t1.AverageMMR();
                if (t2 == null || t2AvaMmr == null)
                {
                    return -1;
                }
                else if (t1 == null || t1AvaMmr == null)
                {
                    return 1;
                }
                else
                {
                    return t2AvaMmr.Value.CompareTo(t1AvaMmr.Value);
                }
            };
            DistributeSimpleGreedy(teams,isT2BetterthanT1);
            
            return teams;
        }
        
        private static void DistributeSimpleGreedy(List<Team> teams, Comparison<Team> isT2BetterthanT1)
        {
            int backIdx = 0;
            bool anyOfTeamsNeedPlayers = true;
            
            while (anyOfTeamsNeedPlayers)
            {
                backIdx = teams.Count - 1;
                // Sort from highest rank to lowest rank
                teams.Sort(isT2BetterthanT1);
                teams.Reverse();

                var setOfPlayers = GetSetOfPlayersToDistribute(teams.Count());
                setOfPlayers.Sort((p1,p2) => p1.GetMMR().CompareTo(p2.GetMMR()));
                ShufflePlayersInSameTier(setOfPlayers);
                foreach (var team in teams)
                {
                    if (team.NeedsPlayers())
                    {
                        team.AddPlayer(setOfPlayers[backIdx]);
                        backIdx--;
                    }
                }
                
                anyOfTeamsNeedPlayers = teams.Any(t => t.NeedsPlayers());
            }
        }
        
        private static int _currentRoleIdx = 0;
        private static List<Player> GetSetOfPlayersToDistribute(int amountOfPlayers)
        {
            List<Player> playersToDistribute = new List<Player>();

            if (_currentRoleIdx >=5)
            {
                _currentRoleIdx = 0;
            }
            
            //Get which role we need to look for
            var roleList = _roleCounts.ToList();
            var keyValue = roleList[_currentRoleIdx];
            string key = keyValue.Key;
            
            //Make sure next time we are here, we look for the next role
            ++_currentRoleIdx;

            int currentAddedPlayers = 0;
            for (int i = 1; i <= 5; i++)
            {
                List<Player> playersInCurrentList = _playersByAmountRoles[i];
                playersInCurrentList.Reverse();
                //copy of the list because we cannot remove items inside foreach from the original list
                foreach (var player in playersInCurrentList.ToList())
                {
                    if (player.PreferedRoles.PlaysTheRole(key))
                    {
                        playersToDistribute.Add(player);
                        playersInCurrentList.Remove(player);
                        ++currentAddedPlayers;
                    }

                    if (currentAddedPlayers >= amountOfPlayers)
                    {
                        return playersToDistribute;
                    }
                }
            }
          
            
            return playersToDistribute;
        }
        private static void CountPreferredRoles(List<Player> players)
        {
            
            foreach (var player in players)
            {
                if (player.PreferedRoles.Fill)
                {
                    _roleCounts["fill"]++;
                    continue;
                }
                if (player.PreferedRoles.Top)
                {
                    _roleCounts["top"]++;
                }
                if (player.PreferedRoles.Jngl)
                {
                    _roleCounts["jngl"]++;
                }
                if (player.PreferedRoles.Mid)
                {
                    _roleCounts["mid"]++;
                }
                if (player.PreferedRoles.Adc)
                {
                    _roleCounts["adc"]++;
                }
                if (player.PreferedRoles.Support)
                {
                    _roleCounts["support"]++;
                }
               
            }
           
        }
        private static void  GroupPlayersByAmountPreferredRoles(Dictionary<int, List<Player>> playersGrouped,List<Player> players)
        {
            //Initialize Lists for 1 to 5 roles
            for (int i = 1; i <= 5; ++i)
            {
                //Creates 5 lists to sort players that can play 1,2,3 .. fill roles
                playersGrouped[i] = new List<Player>();
            }

            //Put each player in their corresponding container
            foreach (var player in players)
            {
                int numberOfRoles = player.PreferedRoles.Amount();
                if (numberOfRoles >=1 && numberOfRoles <=5)
                {
                    playersGrouped[numberOfRoles].Add(player);
                }
            }

            foreach (var playersList in playersGrouped.Values)
            {
                playersList.Sort((p1,p2) => p1.GetMMR().CompareTo(p2.GetMMR()));
            }
        }
        private static void ShufflePlayersInSameTier(List<Player> players)
        {
            Random random = new Random();

            var groupedPlayers = players.GroupBy(p => p.HighestTier);
            
            foreach (var group in groupedPlayers)
            {
                List<Player> playersInSameTier = group.ToList();
                
                //Fisher-Yates shuffle
                int n = playersInSameTier.Count;
                while (n > 1)
                {
                    n--;
                    int k= random.Next(n + 1);
                    
                    //Swap without temp variable
                    //https://stackoverflow.com/questions/804706/swap-two-variables-without-using-a-temporary-variable
                    (playersInSameTier[k], playersInSameTier[n]) = (playersInSameTier[n], playersInSameTier[k]);
                }

                for (int i = 0; i < playersInSameTier.Count; i++)
                {
                    int originalIndex = players.FindIndex(p => p == group.ElementAt(i));
                    players[originalIndex] = playersInSameTier[i];
                }
              
            }
        }
        // +================ OTHERS ================+
        // Exception definition

        public class unequal_player_divide : Exception
        {
            public unequal_player_divide()
            {
            }
            public unequal_player_divide(string message)
            : base(message)
            {
            }
            public unequal_player_divide(string message, Exception inner)
            : base(message, inner)
            {
            }
        }
    }
}
