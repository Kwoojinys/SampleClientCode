using UnityEngine;

namespace SCC
{
    //!<===============================================================================
    //ResourcesKeyFinder
    //!<===============================================================================
    public static class ResourcesKeyFinder
    {

        //!<===========================================================================
        public static string GetKeyFightCharLogic(int charnumber) =>
            $"FightCharLogic_{charnumber:D3}";
    }
}
