using System;
using UnityEngine;

namespace SCC.Math
{
    public static class Mathf
    {
        //!<============================================================================

        public static readonly float PI      = 3.14159274F;
        public static readonly float PI2     = PI * 2;
        public static readonly float RadDeg  = 180.0f / PI;
        public static readonly float DegRad  = PI / 180.0f;
        public static readonly float Rad2Deg = 57.29578f;

        //!<============================================================================
        public static Vector3 LerpNoneClamp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        //!<==============================================================================
        //Lerp
        //!<==============================================================================
        public static float LerpNoneClamp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        //!<============================================================================

        static public UnityEngine.Vector2 xy(this UnityEngine.Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
        static public UnityEngine.Vector3 xyz(this UnityEngine.Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }
        static public UnityEngine.Vector2 ToVector2(this UnityEngine.Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
        static public UnityEngine.Vector3 ToVector3(this UnityEngine.Vector2 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, 0);
        }
        static public float NormalizeRadian(float f)
        {
            float n = (f + Mathf.PI) / (Mathf.PI * 2.0f);
            return (n - Mathf.Floor(n)) * (Mathf.PI * 2.0f) - Mathf.PI;
        }

        static public float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        //!<============================================================================
        public static float Floor(float f)
        {
            return (float)System.Math.Floor(f);
        }
        public static int FloorToInt(float f)
        {
            return (int)System.Math.Floor(f);
        }
        public static int PercentToInt(int v0, float per)
        {
            return Mathf.FloorToInt((float)(v0 * per) / 100.0f);
        }
        public static long PercentToLong(long v0, float per)
        {
            return (long)System.Math.Floor((double)(v0 * (double)per) / 100.0);
        }
        //!<============================================================================

        public static ulong PercentToUlong(ulong v0, float per)
        {
            return (ulong)(v0 * per) / (ulong)100.0f;
        }

        //!<============================================================================

        //public static long PercentToLong(long v0, float per)
        //{
        //    return (long)(v0 * per) / (long)100.0;
        //}

        //!<============================================================================

        static public ulong Clamp(ulong value, ulong min, ulong max)
        {
            return value >= ulong.MaxValue ? min : value < min ? min : value > max ? max : value;
        }

        //!<============================================================================

        static public long Clamp(long value, long min, long max)
        {
            return value < min ? min : value > max ? max : value;
        }

        //!<============================================================================

        static public int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
        //!<============================================================================
        static public uint Clamp(uint value, uint min, uint max)
        {
            return value >= uint.MaxValue ? min : value < min ? min : value > max ? max : value;
        }
        //!<============================================================================

        static public float KmPerHrToMPerSec(float f)
        {
            return f * (1000.0f / (60.0f * 60.0f));
        }

        //!<============================================================================

        static public float KmPerHrToCmPerSec(float f)
        {
            return f * (1000.0f * 100.0f / (60.0f * 60.0f));
        }

        //!<============================================================================
        static public float MPerHrToMPerSec(float f)
        {
            return f * (60.0f * 60.0f);
        }
        //!<============================================================================

        public static System.Type TypeOfBool    = typeof(bool);
        public static System.Type TypeOfInt     = typeof(int);
        public static System.Type TypeOfUInt    = typeof(uint);
        public static System.Type TypeOfFloat   = typeof(float);
        public static System.Type TypeOfDouble  = typeof(double);
        public static System.Type TypeOfDecimal = typeof(decimal);
        public static System.Type TypeOfLong    = typeof(long);
        public static System.Type TypeOfULong   = typeof(ulong);
        public static System.Type TypeOfShort   = typeof(short);
        public static System.Type TypeOfUShort  = typeof(ushort);
        public static System.Type TypeOfSByte   = typeof(sbyte);
        public static System.Type TypeOfByte    = typeof(byte);

        //!<============================================================================

        public static bool IsNumeric(object value)
        {
            return value is int || value is uint
                || value is float || value is double
                || value is decimal
                || value is long || value is ulong
                || value is short || value is ushort
                || value is sbyte || value is byte;
        }
        //!<============================================================================

        public static bool IsNumeric(System.Type value)
        {
            return
                value == TypeOfInt      || value == TypeOfUInt      ||
                value == TypeOfFloat    || value == TypeOfDouble    ||
                value == TypeOfDecimal  || value == TypeOfLong      ||
                value == TypeOfULong    || value == TypeOfShort     ||
                value == TypeOfUShort   || value == TypeOfSByte     ||
                value == TypeOfByte;
        }
        //!<============================================================================
    }
}