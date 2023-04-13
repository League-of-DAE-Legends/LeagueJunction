using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueJunction.Model.TranslatedLegacy;

namespace LeagueJunction.Model
{
    public enum Region
    {
        BR1, EUN1, EUW1, JP1, KR, LA1, LA2,
        NA1, OC1, PH2, RU, SG2, TH2, TR1, TW2, VN2
    }

    public sealed class PreferedRoles
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

    public class Player
    {
        public Player(string mainUsername, Region region)
        {
            MainUsername = mainUsername;
            Region = region;
        }

        // Essential data
        public string MainUsername { get; set; }
        public Region Region { get; set; } = Region.EUW1;

        // Derivative
        private string _displayName;
        public string Displayname 
        { 
            get
            {
                return string.IsNullOrEmpty(_displayName) ? MainUsername : _displayName;
            }
            set
            {
                _displayName = value;
            }
        }

        // Optional
        public string Contact { get; set; }
        public PreferedRoles PreferedRoles { get; set; }

        // Internal
        [JsonProperty("id")]
        public string EncSummonerId { get; set; } // Encrypted summoner id
        public string SoloRank { get; set; } // I,II,III,IV
        public string SoloTier { get; set; } // SILVER, GOLD
        public string FlexRank { get; set; } // I, II , III
        public string FlexTier { get; set; } // SILVER,GOLD
        private uint _mmr = 0;

        public uint GetMMR()
        {
            if (SoloRank == string.Empty || SoloTier == string.Empty || FlexRank == string.Empty || FlexTier == string.Empty)
            {
                throw new Exception("Rank info is empty, fill in rank info first");
            }

            if(_mmr != 0)
            {
                return _mmr;
            }

            var tierValues = new Dictionary<string, uint>()
             {
                    {"IRON", 1},
                    {"BRONZE", 2},
                    {"SILVER", 3},
                    {"GOLD", 4},
                    {"PLATINUM", 5},
                    {"DIAMOND", 6},
                    {"MASTER", 7},
                    {"GRANDMASTER", 8},
                    {"CHALLENGER", 9}
             };

            var rankValues = new Dictionary<string, uint>()
             {
                    {"IV", 1},
                    {"III", 2},
                    {"II", 3},
                    {"I", 4}
             };

            var soloRank = rankValues[SoloRank.ToUpper()];
            var soloTier = tierValues[SoloTier.ToUpper()];

            var flexRank = rankValues[FlexRank.ToUpper()];
            var flexTier = tierValues[FlexTier.ToUpper()];

            // MMR = (tierValue - 1) * 4 + rankValue
            //Iron 4 == 1 MMR
            //Iron 3 == 2 MMR etc
            uint soloMMR = (soloTier - 1) * 4 + soloRank;
            uint flexMMR = (flexTier - 1) * 4 + flexRank;

            if(flexMMR > soloMMR)
            {
                _mmr = flexMMR;
            }
            else
            {
                _mmr = soloMMR;
            }

            return _mmr;
        }

      

    }
}
