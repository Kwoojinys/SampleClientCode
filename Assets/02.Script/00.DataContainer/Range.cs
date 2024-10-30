//!<===========================================================================================

namespace SCC.Math
{
    //!<========================================================================================
    //IntRange
    //!<========================================================================================
    public struct IntRange
    {
        public int min, max;

        public static IntRange Make(int min, int max)
        {
            return new IntRange { min = min, max = max };
        }

        public bool Contains(int v)
        {
            return v >= this.min && v <= this.max;
        }
        public int Relative(int v)
        {
            if (v >= this.min && v <= this.max)
            {
                return v - this.min;
            }

            return 0;
        }
        public int RelativeMax()
        {
            return this.max - this.min;
        }
        public int DistanceTo(int v)
        {
            if (v < this.min)
            {
                return this.min - v;
            }
            else if (v > this.max)
            {
                return v - this.max;
            }
            else
            {
                return 0;
            }
        }

        public int RandomPick()
        {
            int n = this.max - this.min + 1;
            if (n > 1)
            {
                return SCC.Math.Random.Range(this.min, this.max);
            }
            else
            {
                return this.min;
            }
        }

        public int RandomPick(System.Func<int, int, int> randomRangeFunc)
        {
            int n = this.max - this.min + 1;
            if (n > 1)
            {
                return randomRangeFunc(this.min, this.max);
            }
            else
            {
                return this.min;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}..{1}]", this.min, this.max);
        }
    }

    //!<========================================================================================
    //LongRange
    //!<========================================================================================
    public struct LongRange
    {
        public long min, max;

        public static LongRange Make(long min, long max)
        {
            return new LongRange { min = min, max = max };
        }

        public bool Contains(long v)
        {
            return v >= this.min && v <= this.max;
        }

        public long DistanceTo(long v)
        {
            if (v < this.min)
            {
                return this.min - v;
            }
            else if (v > this.max)
            {
                return v - this.max;
            }
            else
            {
                return 0;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}..{1}]", min, max);
        }
    }

    //!<========================================================================================
    //FloatRange
    //!<========================================================================================
    public struct FloatRange
    {
        public float min, max;

        public static FloatRange Make(float min, float max)
        {
            return new FloatRange { min = min, max = max };
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", this.min, this.max);
        }

        public float RandomPick()
        {
            var n = this.max - this.min + 1;
            if (n > 1)
            {
                return Random.Range(this.min, this.max + 1);
            }
            else
            {
                return this.min;
            }
        }
    }
    public struct UIntRange
    {
        public uint min, max;

        public static UIntRange Make(uint min, uint max)
        {
            return new UIntRange { min = min, max = max };
        }

        public bool Contains(uint v)
        {
            return v >= this.min && v <= this.max;
        }
        public uint Relative(uint v)
        {
            if (v >= this.min && v <= this.max)
            {
                return v - this.min;
            }

            return 0;
        }
        public uint RelativeMax()
        {
            return this.max - this.min;
        }
        public uint DistanceTo(uint v)
        {
            if (v < this.min)
            {
                return this.min - v;
            }
            else if (v > this.max)
            {
                return v - this.max;
            }
            else
            {
                return 0;
            }
        }

        public uint RandomPick(System.Func<uint, uint, uint> randomRangeFunc)
        {
            uint n = this.max - this.min + 1;
            if (n > 1)
            {
                return randomRangeFunc(this.min, this.max);
            }
            else
            {
                return this.min;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}..{1}]", this.min, this.max);
        }
    }

    //!<========================================================================================
    //ULongRange
    //!<========================================================================================
    public struct ULongRange
    {
        public ulong min, max;

        public static ULongRange Make(ulong min, ulong max)
        {
            return new ULongRange { min = min, max = max };
        }

        public bool Contains(ulong v)
        {
            return v >= this.min && v <= this.max;
        }

        public ulong DistanceTo(ulong v)
        {
            if (v < this.min)
            {
                return this.min - v;
            }
            else if (v > this.max)
            {
                return v - this.max;
            }
            else
            {
                return 0;
            }
        }
        public override string ToString()
        {
            return string.Format("[{0}..{1}]", min, max);
        }
    }

    //!<========================================================================================

}