using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueJunction.Model
{
    internal class TranslatedLegacy
    {
        // This is translated C++ code (untested as the time of writing)
        // This is code from the first "leagueOfBalance" program.

        struct PreferedRoles
        {
            public bool Top { get; set; }
            public bool Jngl { get; set; }
            public bool Mid { get; set; }
            public bool Adc { get; set; }
            public bool Support { get; set; }
            public bool Fill { get; set; }

            public int Amount()
            {
                // Fill means that they can play all roles
                if (Fill) return 5;
                int amount = 0;
                amount += Convert.ToInt32(Top);
                amount += Convert.ToInt32(Jngl);
                amount += Convert.ToInt32(Mid);
                amount += Convert.ToInt32(Adc);
                amount += Convert.ToInt32(Support);
                return amount;
            }
        }

        public class RawFormsAnswer
        {
            public string TimeStamp { get; set; }
            public string Discord { get; set; }
            public string OpggMain { get; set; }
            public string OpggRegion { get; set; }
            public string PreferedRoles { get; set; }
        }

        public static class Rank
        {
            private static Dictionary<Tier, uint> m_RankToMMR = new Dictionary<Tier, uint>();
            private static bool m_Initialised = false;

            public enum Tier
            {
                Iron4, Iron3, Iron2, Iron1,
                Bronze4, Bronze3, Bronze2, Bronze1,
                Silver4, Silver3, Silver2, Silver1,
                Gold4, Gold3, Gold2, Gold1,
                Plat4, Plat3, Plat2, Plat1,
                Dia4, Dia3, Dia2, Dia1,
                Master,
                GrandMaster,
                Challenger
            }

            public static Tier GetRandomRankTier()
            {
                return (Tier)new Random().Next(0, (int)Tier.Challenger + 1);
            }

            public static uint GetMMR(Tier rankTier)
            {
                if (m_Initialised)
                {
                    return m_RankToMMR[rankTier];
                }

                // MMR Graph
                for (int i = (int)Tier.Iron4; i < (int)Tier.Challenger; i++)
                {
                    Tier rank = (Tier)i;
                    m_RankToMMR[rank] = (uint)(i * i);
                }
                //

                m_Initialised = true;
                return m_RankToMMR[rankTier];
            }
        }

        public class Player
        {
            public string Id { get; set; }
            public Rank.Tier Rank { get; set; }
        }

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

        public class Team
        {
            public Team(int size = 5)
            {
                maxTeamSize = size;
            }

            public int maxTeamSize = 5;
            public List<Player> players = new List<Player>();

            public void AddPlayer(Player p)
            {
                if (players.Count < maxTeamSize)
                {
                    players.Add(p);
                    return;
                }
                int emptyIndex = players.FindIndex(item => item == null);
                if (emptyIndex == -1)
                {
                    throw new Exception("Failed to add player to team.");
                }
                players.Insert(emptyIndex, p);
            }

            public bool NeedsPlayers()
            {
                if (players.Count < maxTeamSize)
                {
                    return true;
                }
                for (int i = 0; i < maxTeamSize; i++)
                {
                    if (players[i] == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            public uint? AverageMMR()
            {
                if (players.All(item => item == null))
                {
                    return null;
                }
                uint mmr = 0;
                uint count = 0;
                foreach (Player p in players)
                {
                    if (p != null)
                    {
                        mmr += Rank.GetMMR(p.Rank);
                        count++;
                    }
                }
                return mmr / count;
            }

            //public string Format(string prefix = "```\n", string suffix = "```\n", string bulletPoint = "- ")
            //{
            //    StringBuilder message = new StringBuilder();
            //    message.Append(prefix);
            //    foreach (Player player in players)
            //    {
            //        message.Append(bulletPoint).Append(player.formsAnswer.discord).Append("\n");
            //    }
            //    message.Append(suffix);
            //    return message.ToString();
            //}
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
                players.Sort((p1, p2) => Rank.GetMMR(p1.Rank).CompareTo(Rank.GetMMR(p2.Rank)));
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
                    teams.Reverse();
                    teams.Sort(isT2BetterthanT1);
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
        }
    }

}
