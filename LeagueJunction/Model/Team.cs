using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueJunction.Model
{
    public sealed class Team
    {
        // +=============== NON-STATICS ===============+
        // Data
        public int MaxTeamSize { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

        public string TeamName { get; set; }

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
                Players.Add(p);
                return;
            }
            int emptyIndex = Players.FindIndex(item => item == null);
            if (emptyIndex == -1)
            {
                throw new Exception("Failed to add player to team.");
            }
            Players.Insert(emptyIndex, p);
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
                    Debug.Assert(false, "MMR is not implemented yet!");
                    //mmr += Rank.GetMMR(p.Rank);
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
        public string DiscordFormat(string bulletPoint = "- ", string prefix = "```\n", string suffix = "```\n")
        {
            StringBuilder message = new StringBuilder();
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
                s += "\n";
                s += player.Displayname;
            }
            return s;
        }


        // +================ STATICS ================+

        private static ushort _teamCounter = 0;
        public static List<Team> SplitIntoTeams(List<Player> players, int playersPerTeam = 5)
        {
            if (players == null) throw new Exception("Players is null!");

            if (players.Count % playersPerTeam != 0)
            {
                throw new unequal_player_divide();
            }

            List<Team> teams = new List<Team>();
            for (int i = 0; i < players.Count / playersPerTeam; i++)
            {
                teams.Add(new Team(playersPerTeam));
            }

            // Sort from lowest rank to highest rank

            Debug.Assert(false, "MMR is not implemented yet!");
            //players.Sort((p1, p2) => Rank.GetMMR(p1.Rank).CompareTo(Rank.GetMMR(p2.Rank)));
            int backIdx = players.Count - 1;
            int frontIdx = 0;
            bool anyOfTeamsNeedPlayers = true;

            Comparison<Team> isT2BetterthanT1 = (t1, t2) => t1.AverageMMR().Value.CompareTo(t2.AverageMMR().Value);

            while (anyOfTeamsNeedPlayers)
            {
                // Sort from lowest rank to highest rank
                teams.Sort(isT2BetterthanT1);
                foreach (var team in teams)
                {
                    if (team.NeedsPlayers())
                    {
                        team.AddPlayer(players[backIdx]);
                        backIdx--;
                    }
                }

                // Sort from highest rank to lowest rank
                teams.Sort(isT2BetterthanT1);
                teams.Reverse();
                foreach (var team in teams)
                {
                    if (team.NeedsPlayers())
                    {
                        team.AddPlayer(players[frontIdx]);
                        frontIdx++;
                    }
                }

                anyOfTeamsNeedPlayers = teams.Any(t => t.NeedsPlayers());
            }

            return teams;
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
