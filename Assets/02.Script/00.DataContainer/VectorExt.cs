using UnityEngine;
using System.Collections;

namespace SCC.Math
{
    public static class VectorExt
    {
        public static float Angle(Vector2 dp)
        {
            return UnityEngine.Mathf.Atan2(dp.y, dp.x) * UnityEngine.Mathf.Rad2Deg;
        }

        public static Color WithA(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, a);
        }
    }

    public struct Int2
    {

        public int x, y;

        public static readonly Int2 zero = new Int2(0, 0);
        public static readonly Int2 one = new Int2(1, 1);
        public static readonly Int2 left = new Int2(-1, 0);
        public static readonly Int2 right = new Int2(1, 0);
        public static readonly Int2 up = new Int2(0, 1);
        public static readonly Int2 down = new Int2(0, -1);

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Int2(Vector2 v)
        {
            this.x = (int)v.x;
            this.y = (int)v.y;
        }

        public override bool Equals(object p)
        {
            if (p == null)
                return false;
            if (p is Int2)
            {
                return this == (Int2)p;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return x + y * 23;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }

        public static Int2 Parse(string s, Int2 defaultVal)
        {
            if (string.IsNullOrEmpty(s))
            {
                return defaultVal;
            }

            s = s.Trim('(', ')');
            int i = s.IndexOf(',');
            if (i > 0 && i < s.Length)
            {
                return new Int2(int.Parse(s.Substring(0, i)), int.Parse(s.Substring(i + 1)));
            }
            else
            {
                return defaultVal; //new Int2(int.Parse(s), 0);
            }
        }

        public static bool operator ==(Int2 a, Int2 b) { return a.x == b.x && a.y == b.y; }
        public static bool operator !=(Int2 a, Int2 b) { return a.x != b.x || a.y != b.y; }

        public static Int2 operator -(Int2 a) { return new Int2(-a.x, -a.y); }
        public static Int2 operator +(Int2 a, Int2 b) { return new Int2(a.x + b.x, a.y + b.y); }
        public static Int2 operator -(Int2 a, Int2 b) { return new Int2(a.x - b.x, a.y - b.y); }

        public static Int2 operator *(Int2 a, Int2 b) { return new Int2(a.x * b.x, a.y * b.y); }
        public static Int2 operator /(Int2 a, Int2 b) { return new Int2(a.x / b.x, a.y / b.y); }
        public static Int2 operator %(Int2 a, Int2 b) { return new Int2(a.x % b.x, a.y % b.y); }

        public static Int2 operator *(int a, Int2 b) { return new Int2(a * b.x, a * b.y); }

        public static Int2 operator *(Int2 a, int b) { return new Int2(a.x * b, a.y * b); }
        public static Int2 operator /(Int2 a, int b) { return new Int2(a.x / b, a.y / b); }
        public static Int2 operator %(Int2 a, int b) { return new Int2(a.x % b, a.y % b); }

        public Vector2 ToVector { get { return new Vector2(x, y); } }
        public int ChebyshevDistance
        {
            get
            {
                return UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(x), UnityEngine.Mathf.Abs(y));
            }
        }

        public int ManhattanDistance
        {
            get
            {
                return UnityEngine.Mathf.Abs(x) + UnityEngine.Mathf.Abs(y);
            }
        }

        public int Area { get { return x * y; } }

        public Int2 RotateLeft { get { return new Int2(-y, x); } }
        public Int2 RotateRight { get { return new Int2(y, -x); } }
    }
}
