using System;
using System.Collections.Generic;
using System.Linq;

namespace SCC.UTIL
{
    //!<===========================================================================================
    public static class ContainerExtension
    {
        private static readonly System.Random rng = new Random();

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            return source.OrderBy((item) => rng.Next());
        }

        public static void ForEach<T, VALUE>(this IDictionary<T, VALUE> dic, Action<T, VALUE> action)
        {
            foreach (var i in dic)
            {
                action.Invoke(i.Key, i.Value);
            }
        }
        public static void ForEach<VALUE>(this VALUE[] array, Action<VALUE> action)
        {
            for(var i = 0; i < array.Length; ++i){
                action.Invoke(array[i]);
            }
        }
        public static void ForEach<T>(this IReadOnlyList<T> source, Action<T> action)
        {
            for (var i = 0; i < source.Count; ++i)
            {
                action.Invoke(source[i]);
            }
        }
        
        public static T PickRandom<T>(this IReadOnlyList<T> source)
        {
            if (source.Count <= 0)
            {
                return default;
            }

            if (source.Count == 1)
            {
                return source[0];
            }

            return source[rng.Next(source.Count)];
        }

        public static T PickRandom<T>(this T[] source)
        {
            if (source.Length <= 0)
            {
                return default;
            }

            if (source.Length == 1)
            {
                return source[0];
            }

            return source[rng.Next(source.Length)];
        }
        public static int RandomLengthAsMax(this int source, int max)
        {
            return rng.Next(source, max);
        }
        public static int RandomLengthAsMin(this int source, int min)
        {
            return rng.Next(min, source);
        }
        public static T PickupRandom<T>(this IList<T> source)
        {
            if (source.Count <= 0)
            {
                return default;
            }

            if (source.Count == 1)
            {
                var pick = source[0];

                source.RemoveAt(0);
                return pick;
            }
            else
            {
                var idx = rng.Next(source.Count);
                var pick = source[idx];

                source.RemoveAt(idx);
                return pick;
            }
        }

        //!<===========================================================================================
    }

    //!<===========================================================================================

    public interface IKeyHint
    {
        int _key { get; }
    }

    //!<===========================================================================================

    public interface IKeyHintT<T>
    {
        T _key { get; }
    }

    //!<===========================================================================================

    public class IndexList<TValue> where TValue : IKeyHint
    {
        public Dictionary<int, int> Index;
        public List<TValue> Items;
        public int Capacity { get; set; } = 20;
        public IReadOnlyList<TValue> ItemList => this._TryInit().Items;
        public IndexList(int capacity)
        {
            this.Capacity = capacity;
        }
        public IndexList()
        {
            this.Capacity = 10;
        }
        public IndexList<TValue> _TryInit()
        {
            if (this.Items == null)
            {
                this.Items = new List<TValue>()
                {
                    Capacity = this.Capacity,
                };
            }
            if (this.Index == null)
            {
                this.Index = new Dictionary<int, int>();
            }

            return this;
        }
        public int Count
        {
            get
            {
                this._TryInit();
                return this.Items.Count;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return this.Count <= 0;
            }
        }
        public IndexList<TValue> Create(IReadOnlyList<TValue> values)
        {
            for (var i = 0; i < values.Count; ++i){
                this.Add(values[i]);
            }

            return this;
        }
        public void Foreach(System.Action<TValue> action)
        {
            if (this.Items == null) return;

            for (var i = 0; i < this.Items.Count; ++i)
            {
                action.Invoke(this.Items[i]);
            }
        }
        public TValue GetAt(int index)
        {
            this._TryInit();
            return this.Items.Count > index ? this.Items[index] : default;
        }
        public TValue GetValue(int key)
        {
            this._TryInit();

            var index = -1;
            return this.Index.TryGetValue(key, out index) ? this.Items[index] : default;
        }
        public bool Add(TValue value)
        {
            this._TryInit();

            if (this.Index.ContainsKey(value._key) != false) return false;
            this.Index.Add(value._key, this.Items.Count);
            this.Items.Add(value);

            return true;
        }
        public bool Delete(TValue value)
        {
            this._TryInit();

            return this.Delete(value._key);
        }
        public bool Delete(int key)
        {
            this._TryInit();
            var index = 0;

            if (this.Index.TryGetValue(key, out index) != true) return false;
            var endIndex = this.Items.Count - 1;
            var end = this.Items[endIndex];

            this.Items[endIndex] = this.Items[index];
            this.Items[index] = end;
            this.Index[end._key] = index;

            this.Index.Remove(key);
            this.Items.RemoveAt(endIndex);
            return true;
        }
        public bool HasValue(int key)
        {
            this._TryInit();

            return this.Index.ContainsKey(key);
        }
        public bool HasValue(TValue value)
        {
            this._TryInit();

            return this.HasValue(value._key);
        }
        public void Update(TValue value)
        {
            this._TryInit();

            var tValue = this.GetValue(value._key);
            if (tValue != null)
            {
                tValue = value;
            }
            else
            {
                this.Add(value);
            }
        }

        public void Clear()
        {
            this._TryInit();
            this.Index.Clear();
            this.Items.Clear();
        }
    }

    //!<===========================================================================================

    public class ListIndexer<TValue> where TValue : IKeyHint
    {
        private Dictionary<int, int> _indexer = null;
        private List<TValue> _refTable = null;

        public IReadOnlyList<TValue> RefTable => this._refTable;

        private void _tryInit()
        {
            if (this._indexer == null)
            {
                this._indexer = new Dictionary<int, int>();
            }
        }
        public ListIndexer<TValue> Create(List<TValue> table)
        {
            this._tryInit();
            this._indexer.Clear();
            this._refTable = table;

            this.Build();

            return this;
        }
        public int Count { get { return this._refTable.Count; } }

        public void Foreach(System.Action<TValue> action)
        {
            if (this._refTable == null) return;

            for (var i = 0; i < this._refTable.Count; ++i)
            {
                action.Invoke(this._refTable[i]);
            }
        }
        public void Build()
        {
            if (this._refTable == null) return;

            this._indexer.Clear();

            for (var i = 0; i < this._refTable.Count; ++i)
            {
                var s = this._refTable[i];

                if (this._indexer.ContainsKey(s._key) == true)
                {
#if DEV_BUILD_VERSION
                    throw new System.ArgumentException($"key is already TyeName: {_refTable.ToString()},Key:{s._key}");
#else

                    UnityEngine.Debug.LogError($"key is already TyeName: {_refTable.ToString()},Key:{s._key}");
#endif
                }
                else
                {
                    this._indexer.Add(s._key, i);
                }
            }
        }
        public bool Add(TValue value)
        {
            if (this._refTable == null || this._indexer == null)
                return false;

            if (this.HasValue(value._key) == false)
            {
                this._refTable.Add(value);
                this._indexer.Add(value._key, this._refTable.Count - 1);
                return true;
            }

            return false;
        }
        public TValue GetAt(int index)
        {
            return this._refTable.Count > index ? this._refTable[index] : default;
        }
        public TValue Swap(TValue value, int newkey)
        {
            if (this._refTable == null || this._indexer == null)
                return default;

            var i = -1;
            if (this._indexer.TryGetValue(value._key, out i) == true)
            {
                this._indexer.Remove(value._key);
                this._indexer.Add(newkey, i);

                return value;
            }
            else
            {
                return default;
            }
        }
        public bool Delete(TValue value)
        {
            return this.Delete(value._key);
        }
        public void DeleteAll()
        {
            if(this._refTable != null)
            {
                this._refTable.Clear();
                this._indexer.Clear();
            }
        }
        public bool Delete(int key)
        {
            var i = -1;
            if (this._indexer.TryGetValue(key, out i) != true) return false;

            var endIndex = this._refTable.Count - 1;
            var end = this._refTable[endIndex];

            this._refTable[endIndex] = this._refTable[i];
            this._refTable[i] = end;
            this._indexer[end._key] = i;

            this._indexer.Remove(key);
            this._refTable.RemoveAt(endIndex);
            return true;
        }

        public TValue GetValue(int key)
        {
            var i = -1;

            if (this._indexer == null)
            {
                return default;
            }

            return this._indexer.TryGetValue(key, out i) == true ? this._refTable[i] : default;
        }
        public bool HasValue(int key)
        {
            return this._indexer.ContainsKey(key);
        }
    }

    //!<===========================================================================================

    public class Indexer<TValue> where TValue : class, IKeyHint
    {
        public Dictionary<int, TValue> index = new Dictionary<int, TValue>();

        public void Clear()
        {
            this.index.Clear();
        }
        public bool AddValue(TValue value)
        {
            if (this.index.ContainsKey(value._key) == false)
            {
                this.index.Add(value._key, value);
                return true;
            }
            else
            {
                //!< error
                UnityEngine.Debug.LogErrorFormat("error Indexer AddValue!!!{0}={1}", this.index.ToString(), value._key);
            }
            return false;
        }
        public void AddValues(IEnumerable<TValue> values)
        {
            foreach (var value in values)
            {
                this.AddValue(value);
            }
        }
        public bool HasKey(int Key)
        {
            return this.index.ContainsKey(Key);
        }
        public TValue GetValue(int key)
        {
            this.index.TryGetValue(key, out TValue v);
            return v;
        }
    }

    //!<===========================================================================================

    public class ListIndexerT<TKey, TValue> where TValue : IKeyHintT<TKey>
    {
        private Dictionary<TKey, int> _indexer  = null;
        private List<TValue> _refTable          = null;

        private void _tryInit()
        {
            if (this._indexer == null)
            {
                this._indexer = new Dictionary<TKey, int>();
            }
        }
        public ListIndexerT<TKey, TValue> Create(List<TValue> table)
        {
            this._tryInit();
            this._indexer.Clear();
            this._refTable = table;

            this.Build();

            return this;
        }
        public int Count { get { return this._refTable?.Count ?? 0; } }
        public IReadOnlyList<TValue> RefTable => this._refTable;

        public void ForEach(System.Action<TValue> action)
        {
            if (this._refTable == null) return;

            for (var i = 0; i < this._refTable.Count; ++i)
            {
                action.Invoke(this._refTable[i]);
            }
        }
        public void Build()
        {
            if (this._refTable == null) return;

            this._indexer.Clear();

            for (var i = 0; i < this._refTable.Count; ++i)
            {
                var s = this._refTable[i];

                if (this._indexer.ContainsKey(s._key) == true)
                {
#if DEV_BUILD_VERSION
                    throw new System.ArgumentException($"key is already TyeName: {_refTable.ToString()},Key:{s._key}");
#else

                    UnityEngine.Debug.LogError($"key is already TyeName: {_refTable.ToString()},Key:{s._key}");
#endif
                }
                else
                {
                    this._indexer.Add(s._key, i);
                }
            }

        }
        public bool Add(TValue value)
        {
            if (this._indexer == null || this._refTable == null) return false;

            if (this._indexer.ContainsKey(value._key) == false)
            {
                this._refTable.Add(value);
                this._indexer.Add(value._key, this._refTable.Count - 1);
                return true;
            }
            return false;
        }
        public TValue GetAt(int index)
        {
            if (this._indexer == null || this._refTable == null)
                return default;

            return this._refTable.Count > index ? this._refTable[index] : default;
        }
        public bool Delete(TKey key)
        {
            if (this._indexer == null) return false;

            var i = -1;
            if (this._indexer.TryGetValue(key, out i) != true) return false;

            var endIndex = this._refTable.Count - 1;
            var end = this._refTable[endIndex];

            this._refTable[endIndex] = this._refTable[i];
            this._refTable[i] = end;
            this._indexer[end._key] = i;

            this._indexer.Remove(key);
            this._refTable.RemoveAt(endIndex);
            return true;
        }
        public TValue GetValue(TKey key)
        {
            if (this._indexer == null || this._refTable == null)
            {
                return default;
            }

            var i = -1;
            return this._indexer.TryGetValue(key, out i) == true ? this._refTable[i] : default;
        }
        public bool HasValue(TKey key)
        {
            if (this._indexer == null) return false;

            return this._indexer.ContainsKey(key);
        }


    }

    //!<===========================================================================================
    public class IndexListT<TKey, TValue> where TValue : IKeyHintT<TKey>
    {
        public Dictionary<TKey, int> Index;
        public List<TValue> Items;

        public IndexListT()
        {

        }
        public IndexListT(int capacity)
        {
            this.Items = new List<TValue>()
            {
                Capacity = capacity
            };

            this._TryInit();
        }
        public IndexListT<TKey, TValue> CreateRef(List<TValue> table)
        {
            if (this.Index == null)
            {
                this.Index = new Dictionary<TKey, int>();
            }
            this.Index.Clear();
            this.Items = table;

            this.Build();

            return this;
        }
        public IndexListT<TKey, TValue> Create(IReadOnlyList<TValue> table)
        {
            this._TryInit();
            this.Index.Clear();
            this.Items = table.ToList();

            this.Build();

            return this;
        }
        public void Build()
        {
            if (this.Items == null)
                return;

            this.Index.Clear();

            for (var i = 0; i < this.Items.Count; ++i)
            {
                var s = this.Items[i];

                if (this.Index.ContainsKey(s._key) == true)
                {
#if DEV_BUILD_VERSION
                    throw new System.ArgumentException($"key is already TyeName: {this.Items.ToString()},Key:{s._key}");
#else

                    UnityEngine.Debug.LogError($"key is already TypeName: {this.Items},Key:{s._key}");
#endif
                }
                else
                {
                    this.Index.Add(s._key, i);
                }
            }
        }
        public void _TryInit()
        {
            if (this.Items == null)
            {
                this.Items = new List<TValue>();
            }

            if (this.Index == null)
            {
                this.Index = new Dictionary<TKey, int>();
            }
        }
        public int Count
        {
            get
            {
                this._TryInit();
                return this.Items.Count;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return this.Count <= 0;
            }
        }
        public void ForEach(System.Action<TValue> action)
        {
            if (this.Items == null) return;

            for (var i = 0; i < this.Items.Count; ++i)
            {
                action.Invoke(this.Items[i]);
            }
        }
        public TValue GetAt(int index)
        {
            this._TryInit();
            return this.Items.Count > index ? this.Items[index] : default;
        }
        public TValue GetValue(TKey key)
        {
            this._TryInit();

            var index = -1;
            return this.Index.TryGetValue(key, out index) ? this.Items[index] : default;
        }
        public bool Add(TValue value)
        {
            this._TryInit();

            if (this.Index.ContainsKey(value._key) != false) return false;
            this.Index.Add(value._key, this.Items.Count);
            this.Items.Add(value);

            return true;
        }
        public bool Delete(TKey key)
        {
            this._TryInit();

            if (this.Index.TryGetValue(key, out int index) != true) return false;

            var endIndex = this.Items.Count - 1;
            var end = this.Items[endIndex];

            this.Items[endIndex]    = this.Items[index];
            this.Items[index]       = end;
            this.Index[end._key]    = index;

            this.Index.Remove(key);
            this.Items.RemoveAt(endIndex);
            return true;
        }
        public bool Delete(TValue value)
        {
            return this.Delete(value._key);
        }
        public bool HasValue(TKey key)
        {
            this._TryInit();

            return this.Index.ContainsKey(key);
        }

        public void Update(TValue value)
        {
            this._TryInit();

            var tValue = this.GetValue(value._key);
            if (tValue != null)
            {
                tValue = value;
            }
            else
            {
                this.Add(value);
            }
        }

        public void Clear()
        {
            this._TryInit();
            this.Index.Clear();
            this.Items.Clear();
        }
    }

    public struct KeyMultipleHint<T1, T2>
        where T1 : IComparable
        where T2 : IComparable
    {
        public T1 KEY_1;
        public T2 KEY_2;

        public KeyMultipleHint(T1 key01, T2 key02)
        {
            this.KEY_1 = key01;
            this.KEY_2 = key02;
        }
        public static bool operator ==(KeyMultipleHint<T1,T2> _lhs, KeyMultipleHint<T1, T2> _rhs)
        {
            return _lhs.Equals(_rhs);
        }
        public static bool operator !=(KeyMultipleHint<T1, T2> _lhs, KeyMultipleHint<T1, T2> _rhs)
        {
            return _lhs.Equals(_rhs) == false;
        }
        public bool Equals(KeyMultipleHint<T1, T2> other)
        {
            return other.KEY_1.Equals(this.KEY_1) &&
                other.KEY_2.Equals(this.KEY_2);
        }
        public override bool Equals(object p)
        {
            return p == null ? false :
                p is KeyMultipleHint<T1, T2> @obj ? this.Equals(@obj) : false;
        }
        public override int GetHashCode()
        {
            return HashCode
                .Combine(this.KEY_1,this.KEY_2);
        }
    }
    public struct KeyMultipleHint<T1, T2,T3>
       where T1 : IComparable
       where T2 : IComparable
       where T3 : IComparable
    {
        public T1 KEY_1;
        public T2 KEY_2;
        public T3 KEY_3;

        public KeyMultipleHint(T1 key01, T2 key02,T3 key03)
        {
            this.KEY_1 = key01;
            this.KEY_2 = key02;
            this.KEY_3 = key03;
        }
        public static bool operator ==(KeyMultipleHint<T1, T2, T3> _lhs, KeyMultipleHint<T1, T2, T3> _rhs)
        {
            return _lhs.Equals(_rhs);
        }
        public static bool operator !=(KeyMultipleHint<T1, T2, T3> _lhs, KeyMultipleHint<T1, T2, T3> _rhs)
        {
            return _lhs.Equals(_rhs) == false;
        }
        public bool Equals(KeyMultipleHint<T1, T2, T3> other)
        {
            return other.KEY_1.Equals(this.KEY_1) &&
                other.KEY_2.Equals(this.KEY_2) &&
                other.KEY_3.Equals(this.KEY_3);
        }
        public override bool Equals(object p)
        {
            return p == null ? false :
                p is KeyMultipleHint<T1, T2, T3> @obj ? this.Equals(@obj) : false;
        }
        public override int GetHashCode()
        {
            return HashCode
                .Combine(this.KEY_1, this.KEY_2,this.KEY_3);
        }
    }
    //!<===========================================================================================
    //IndexListGroup
    public class IndexListGroupT<TGroupKey,TKey,TValue> where TValue : IKeyHintT<TKey>
    {
        public Dictionary<TGroupKey, int> Index;
        public List<IndexListT<TKey,TValue>> Items;

        public IReadOnlyList<IndexListT<TKey, TValue>> ItemList => this.Items;

        public void _TryInit()
        {
            if (this.Items == null){
                this.Items = new List<IndexListT<TKey, TValue>>();
            }

            if (this.Index == null){
                this.Index = new Dictionary<TGroupKey, int>();
            }
        }

        public bool Add(TGroupKey group,TValue value)
        {
            this._TryInit();

            IndexListT<TKey, TValue> itemGroup;
            if (this.Index.TryGetValue(group, out var num) == false)
            {
                this.Index.Add(group, this.Items.Count);
                this.Items.Add(itemGroup = new IndexListT<TKey, TValue>());
                itemGroup.Add(value);

                return true;
            }
           
            return false;
        }
        public bool Delete(TGroupKey group,TValue value)
        {
            this._TryInit();

            if (this.Index.TryGetValue(group, out var num) == true)
            {
                var itemGroup = this.Items[num];
                if (itemGroup.Delete(value._key) == true && itemGroup.Count == 0)
                {
                    var endIndex = this.Items.Count - 1;
                    var end      = this.Items[endIndex];

                    this.Items[endIndex]    = this.Items[num];
                    this.Items[num]         = end;
                    this.Index[group] = num;

                    this.Index.Remove(group);
                    this.Items.RemoveAt(endIndex);
                    return true;
                }
            }
            return false;
        }
        public IndexListT<TKey, TValue> FindGroup(TGroupKey key)
        {
            this._TryInit();

            if (this.Index.TryGetValue(key, out var num) == true)
            {
                return this.Items[num];
            }
            return null;
        }
        public IndexListT<TKey, TValue> FindGroupOrAdd(TGroupKey key)
        {
            this._TryInit();

            if (this.Index.TryGetValue(key, out var num) == false)
            {
                this.Index.Add(key, this.Items.Count);
                this.Items.Add(new IndexListT<TKey, TValue>());
            }

            return this.Items[num];
        }
        public void ForEach(System.Action<IndexListT<TKey, TValue>> action)
        {
            this.Items.ForEach(action);
        }
        public void ForEachItem(TGroupKey group, System.Action<TValue> action)
        {
            if (this.Index.TryGetValue(group, out var num) == true)
            {
                this.Items[num]?
                    .ForEach(action);
            }
        }
    }
    //!<===========================================================================================
    public class ActionContainer<T>
    {
        //!<=======================================================================================

        protected List<System.Action<T>> Items  = null;
        protected Dictionary<int, int> Index    = null;

        //!<=======================================================================================

        public IReadOnlyList<System.Action<T>> Callbacks => this.Items;

        //!<=======================================================================================
        public ActionContainer(int capacity = 30)
        {
            this.Items = new List<Action<T>>() { Capacity = capacity };
            this.Index = new Dictionary<int, int>();
        }
        public bool Add(System.Action<T> callback)
        {
            var hash = callback.GetHashCode();
            if (this.Index.ContainsKey(hash) != false)
            {
                return false;
            }
            this.Index.Add(hash, this.Items.Count);
            this.Items.Add(callback);

            return true;
        }
        public bool Delete(int hashcode)
        {
            if (this.Index.TryGetValue(hashcode, out var index) != false)
            {
                return false;
            }

            var endIndex = this.Items.Count - 1;
            var end      = this.Items[endIndex];

            this.Items[endIndex]    = this.Items[index];
            this.Items[index]       = end;
            this.Index[hashcode]     = index;
            this.Index.Remove(hashcode);
            this.Items.RemoveAt(endIndex);
            return true;
        }
        public bool Delete(System.Action<T> callback)
        {
            return this.Delete(callback.GetHashCode());
        }
        public void Foreach(System.Action<System.Action<T>> callback)
        {
            for (var i = 0; i < this.Items.Count; ++i)
            {
                callback.Invoke(this.Items[i]);
            }
        }
        public void OnFire(T value)
        {
            for(var i = 0; i < this.Items.Count; ++i)
            {
                this.Items[i]?
                    .Invoke(value);
            }
        }
        public void Clear()
        {
            this.Items.Clear();
            this.Index.Clear();
        }
    }
    public class VoidActionContainer
    {
        //!<=======================================================================================

        protected List<System.Action>  Items = null;
        protected Dictionary<int, int> Index = null;

        //!<=======================================================================================

        public IReadOnlyList<System.Action> Callbacks => this.Items;

        //!<=======================================================================================
        public VoidActionContainer(int capacity = 30)
        {
            this.Items = new List<Action>() { Capacity = capacity };
            this.Index = new Dictionary<int, int>();
        }
        public bool Add(System.Action callback)
        {
            var hash = callback.GetHashCode();
            if (this.Index.ContainsKey(hash) != false)
            {
                return false;
            }
            this.Index.Add(hash, this.Items.Count);
            this.Items.Add(callback);

            return true;
        }
        public bool Delete(int hashcode)
        {
            if (this.Index.TryGetValue(hashcode, out var index) == false)
            {
                return false;
            }

            var endIndex = this.Items.Count - 1;
            var end      = this.Items[endIndex];

            this.Items[endIndex] = this.Items[index];
            this.Items[index]    = end;
            this.Index[hashcode]  = index;
            this.Index.Remove(hashcode);
            this.Items.RemoveAt(endIndex);
            return true;
        }
        public bool Delete(System.Action callback)
        {
            return this.Delete(callback.GetHashCode());
        }
        public void Foreach(System.Action<System.Action> callback)
        {
            for (var i = 0; i < this.Items.Count; ++i)
            {
                callback.Invoke(this.Items[i]);
            }
        }
        public void OnFire()
        {
            for (var i = 0; i < this.Items.Count; ++i)
            {
                this.Items[i]?
                    .Invoke();
            }
        }
        public void Clear()
        {
            this.Items.Clear();
            this.Index.Clear();
        }
    }
  
    //!<===========================================================================================

    public class CallbacksContainer<T> where T : class
    {
        //!<=======================================================================================

        protected List<T>         Items       = null;
        protected Dictionary<int, int> Index  = null;

        //!<=======================================================================================

        public IReadOnlyList<T> Callbacks    => this.Items;
        public Type Interface { get; protected set; }

        //!<=======================================================================================
        public CallbacksContainer(int capacity = 30)
        {
            this.Items = new List<T>() { Capacity = capacity };
            this.Index = new Dictionary<int, int>();
            this.Interface = typeof(T);
        }
        public bool Add(T callback)
        {
            var hash = callback.GetHashCode();
            if (this.Index.ContainsKey(hash) != false)
            {
                return false;
            }
            this.Index.Add(hash, this.Items.Count);
            this.Items.Add(callback);

            return true;
        }
        public bool Delete(int hashcode)
        {
            if (this.Index.TryGetValue(hashcode, out var index) != false)
            {
                return false;
            }

            var endIndex = this.Items.Count - 1;
            var end = this.Items[endIndex];

            this.Items[endIndex] = this.Items[index];
            this.Items[index] = end;
            this.Index[hashcode] = index;
            this.Index.Remove(hashcode);
            this.Items.RemoveAt(endIndex);
            return true;
        }
        public bool Delete(T value)
        {
            return this.Delete(value.GetHashCode());
        }
        public void Foreach(System.Action<T> callback)
        {
            for (var i = 0; i < this.Items.Count; ++i)
            {
                callback.Invoke(this.Items[i]);
            }
        }
        public void Clear()
        {
            this.Items.Clear();
            this.Index.Clear();
        }
        public bool TryAdd(object callack)
        {
            if(callack is T target)
            {
                return this.Add(target);
            }
            return false;
        }
        public bool TryDelete(object callack)
        {
            if (callack is T target)
            {
                return this.Delete(target);
            }
            return false;
        }
    }

    //!<===========================================================================================

    public class IndexListSimpleT<TKey, TValue> where TValue : class
    {
        //!<=======================================================================================

        protected Dictionary<TKey, TValue> Index;
        protected List<TValue> Items;

        //!<=======================================================================================

        public IReadOnlyList<TValue> ItemList => this.Items;

        //!<=======================================================================================

        public void _TryInit(int capacity = 0)
        {
            if (this.Items == null)
            {
                this.Items = new List<TValue>() { Capacity = capacity};
            }

            if (this.Index == null)
            {
                this.Index = new Dictionary<TKey, TValue>();
            }
        }
        public int Count
        {
            get
            {
                this._TryInit();
                return this.Items.Count;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return this.Count <= 0;
            }
        }
        public void ForEach(System.Action<TValue> action)
        {
            if (this.Items == null) return;

            for (var i = 0; i < this.Items.Count; ++i)
            {
                action.Invoke(this.Items[i]);
            }
        }
        public TValue GetAt(int index)
        {
            this._TryInit();
            return this.Items.Count > index ? this.Items[index] : default;
        }
        public TValue GetValue(TKey key)
        {
            this._TryInit();

            return this.Index.TryGetValue(key, out TValue value)
                ? value : default;
        }
        public bool Add(TKey key,TValue value)
        {
            this._TryInit();

            if (this.Index.ContainsKey(key) != false) return false;
            this.Index.Add(key, value);
            this.Items.Add(value);

            return true;
        }
        public bool Delete(TKey key)
        {
            this._TryInit();

            if (this.Index.TryGetValue(key, out TValue value) != true) 
                return false;

            this.Index.Remove(key);
            this.Items.Remove(value);
            return true;
        }
        public bool HasValue(TKey key)
        {
            this._TryInit();

            return this.Index.ContainsKey(key);
        }
        public void Clear()
        {
            this._TryInit();
            this.Index.Clear();
            this.Items.Clear();
        }
    }

    //!<===========================================================================================
}