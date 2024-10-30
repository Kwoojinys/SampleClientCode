using System.Collections.Generic;

namespace SCC.Math
{
    public static class ShuffleExtension
    {
        private static System.Random rng = new System.Random();
        public static void SetSeed(int seed)
        {
            rng = new System.Random(seed);
        }
        public static void SetRandom()
        {
            rng = new System.Random();
        }
        public static void Shuffle<T>(List<T> list)
        {
            Shuffle(list, 0, list.Count, Random.Range);
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            int n   = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static IList<T> ToShuffle<T>(this IList<T> list)
        {
            int n   = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public static void Shuffle<T>(List<T> list, System.Func<int, int, int> randRange)
        {
            Shuffle(list, 0, list.Count, randRange);
        }

        public static void Shuffle<T>(List<T> list, int startIndex, int endIndex, System.Func<int, int, int> randRange)
        {
            if (list == null || list.Count <= 1)
                return;

            startIndex  = Mathf.Clamp(startIndex, 0, list.Count - 1);
            endIndex    = Mathf.Clamp(endIndex, startIndex, list.Count);

            var n = endIndex - startIndex;
            if (n <= 1)
                return;

            for (int i = 0; i < n; ++i)
            {
                int j = randRange(i, n);
                if (i != j)
                {
                    var v = list[i];
                    list[i] = list[j];
                    list[j] = v;
                }
            }
        }

        public static List<int> ShuffleList(List<int> list, int n, System.Func<int, int, int> randRange)
        {
            if (list == null)
            {
                list = new List<int>();
            }

            list.Clear();
            list.Capacity = n;

            for (int i = 0; i < n; ++i)
            {
                list.Add(i);
            }

            Shuffle(list, randRange);
            return list;
        }

        public static List<int> ShuffleList(int n, System.Func<int, int, int> randRange)
        {
            return ShuffleList(null, n, randRange);
        }

        public static T Pick2<T>(float p, T item1, T item2)
        {
            if (Random.Range(0.0f, 1.0f) < p)
            {
                return item1;
            }
            else
            {
                return item2;
            }
        }
    }
}
