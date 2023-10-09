using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueJunction.Repository
{
    public interface ITeamNamesRepository
    {
        string GetNextTeamName();
    }
}
