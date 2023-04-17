using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueJunction.Model;

namespace LeagueJunction.Repository
{
    public interface IPlayerRepository
    {
        List<Player> GetPlayers(string sourceFile = "");
    }
}
