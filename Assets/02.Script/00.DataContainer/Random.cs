using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SCC.Math
{
    //!<===========================================================================================
    public static class Random
    {
        //!<=======================================================================================

        private static System.Random float_Random   = new System.Random();
        private static System.Random integer_Random = new System.Random();

        //!<=======================================================================================

        public static float Range(float minInclusive, float maxInclusive)
        {
            return float_Random.NextFloat(minInclusive, maxInclusive);
        }
        public static int Range(int minInclusive, int maxInclusive)
        {
            return integer_Random.Next(minInclusive, maxInclusive);
        }
    }
}
