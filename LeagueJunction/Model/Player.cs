using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static LeagueJunction.Model.TranslatedLegacy;

namespace LeagueJunction.Model
{
   /// <summary>
   /// The regions correspond with the regions specified and used by Riot API
   /// See https://developer.riotgames.com
   /// </summary>
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

        public bool PlaysTheRole(string role)
        {
            if (Fill)
            {
                return true;
            }
            switch (role)
            {
                case "top": case "TOP": case "Top":
                    return Top;
                    break;
                
                case "mid": case "MID": case "Mid":
                    return Mid;
                    break;
               
                case "jngl": case "Jngl": case "JNGL":
                    return Jngl;
                    break;
                
                case "adc": case "Adc": case "ADC":
                    return Adc;
                    break;
                
                case "Support": case "SUPPORT": case "support":
                    return Support;
                    break;
            }

            return false;
        }

        public override string ToString()
        {
            string res = String.Empty;

            if (Fill)
            {
                res += "FILL";
                return res;
            }
            
            if (Top)
            {
                res += " TOP ";
            }

            if (Jngl)
            {
                res += " JGL";
            }

            if (Mid)
            {
                res += " MID ";
            }

            if (Adc)
            {
                res += " ADC ";
            }

            if (Support )
            {
                res += " SUPPORT";
            }

            
            
            return res;
        }
    }

    public class Player 
    {
        public Player(string mainUsername, Region region)
        {
            MainUsername = mainUsername;
            Region = region;
            SoloRank = "IV";
            SoloTier = "SILVER";
            FlexRank = "IV";
            FlexTier = "SILVER";
            MMR = 0;
            FullRankHighest = SoloRank + SoloTier;
        }

        // Essential data
        public string MainUsername { get; set; }
        public Region Region { get; set; } = Region.EUW1;
        
        private string _displayName;
        /// <summary>
        /// Always use Displayname to get their username.
        /// This will be filled with their non EUW username if that exists, otherwise with the EUW username.
        /// The point is to get their main rank which is expected to be higher in their main region.
        /// </summary>
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

        
        public override string ToString()
        {
            return Displayname;
        }

        // Optional
        public string Contact { get; set; }
        public PreferedRoles PreferedRoles { get; set; }
       

        // Internal
        // The properties correspond with the properties that we need to read from RIOT API
        [JsonProperty("id")]
        public string EncSummonerId { get; set; } // Encrypted summoner id
        public string SoloRank { get; set; } // I,II,III,IV
        public string SoloTier { get; set; } // SILVER, GOLD
        public string FlexRank { get; set; } // I, II , III
        public string FlexTier { get; set; } // SILVER,GOLD

        public string FullRankHighest { get; set; }
        public string HighestTier { get; set; }
        public uint MMR { get; set; }
        
        private void UpdateMMR()
        {
            if (string.IsNullOrEmpty(SoloRank))
            {
                MessageBox.Show($"Solo Rank for {_displayName} was empty/null, setting to default IV");
            }
            
            if (string.IsNullOrEmpty(FlexRank))
            {
                MessageBox.Show($"Flex Rank for {_displayName} was empty/null, setting to default IV");
            }
            
            if (string.IsNullOrEmpty(SoloTier))
            {
                MessageBox.Show($"Solo Tier for {_displayName} was empty/null, setting to default SILVER");
            }
            if (string.IsNullOrEmpty(FlexTier))
            {
                MessageBox.Show($"Flex Tier for {_displayName} was empty/null, setting to default SILVER");
            }
            
            var tierValues = new Dictionary<string, uint>()
            {
                {"IRON", 1},
                {"BRONZE", 2},
                {"SILVER", 3},
                {"GOLD", 4},
                {"PLATINUM", 5},
                {"EMERALD",6},
                {"DIAMOND", 7},
                {"MASTER", 8},
                {"GRANDMASTER", 9},
                {"CHALLENGER", 10}
            };

            var rankValues = new Dictionary<string, uint>()
            {
                {"IV", 1},
                {"III", 2},
                {"II", 3},
                {"I", 4}
            };
            
            var soloRank = string.IsNullOrEmpty(SoloRank) ? rankValues["IV"] : rankValues[SoloRank.ToUpper()];
            var soloTier = string.IsNullOrEmpty(SoloTier) ? tierValues["SILVER"] : tierValues[SoloTier.ToUpper()];

            var flexRank = string.IsNullOrEmpty(FlexRank) ? rankValues["IV"] : rankValues[FlexRank.ToUpper()];
            var flexTier = string.IsNullOrEmpty(FlexTier) ? tierValues["SILVER"] : tierValues[FlexTier.ToUpper()];

            // MMR = (tierValue - 1) * 4 + rankValue
            //Iron 4 == 1 MMR
            //Iron 3 == 2 MMR etc
            uint soloMMR = (soloTier - 1) * 4 + soloRank;
            uint flexMMR = (flexTier - 1) * 4 + flexRank;

            if(flexMMR > soloMMR)
            {
                MMR = flexMMR;
                FullRankHighest = FlexTier + ' '+ FlexRank;
                HighestTier = FlexTier;
            }
            else
            {
                MMR = soloMMR;
                FullRankHighest = SoloTier +' ' + SoloRank;
                HighestTier = SoloTier;
            }
            
            #region oldMmrCalc
#if false
            // Currently _mmr is a value that you could see as what inbetween rank
            // am I with 0 being the lowest?
            // Using this, we can fill it in the following formula as x to calculate
            // mmr based on a graph.
            // y = a (x-b)³ + c

            // Values are determined with geogebra
            var amplitude = 0.00134;  // a in formula
            var horizontalOffset = 8; // b in formula
            var verticalOffset = 2.2; // c in formula
            var exponent = 3;
            MMR = (uint)(Math.Round((amplitude * Math.Pow(MMR - horizontalOffset, exponent) + verticalOffset) * 10000));
#endif
            #endregion
            
        }
        
        public uint GetMMR()
        {
            UpdateMMR();
            
            // https://www.leagueofgraphs.com/rankings/rank-distribution
            // First number is rank number, second number is accumilitave of total MMR (10k)
            var newMMrCalc = new Dictionary<uint, uint>()
            {
                {40,10000},
                {36,98997},
                {32,98950},
                {28,98480},
                {27,98040},
                {26,97540},
                {25,96960},
                {24,95760},
                {23,94460},
                {22,92760},
                {21,89960},
                {20,83960},
                {19,81960},
                {18,78860},
                {17,74860},
                {16,67860},
                {15,65160},
                {14,61160},
                {13,56060},
                {12,48060},
                {11,45160},
                {10,40960},
                {9,35860},
                {8,28360},
                {7,24960},
                {6,20360},
                {5,15060},
                {4,7660},
                {3,4260},
                {2,1560},
                {1,460}
            };
            return newMMrCalc[MMR];
            
        }

    }
}
