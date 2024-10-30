using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using SimpleJSON;

namespace SCC.JsonIO {

	public class JsonReader : IEnumFieldsWithJson
    {
        public static readonly string NULL_STR = "null";

		static JsonReader instance = new JsonReader ();
        public void OnField(JSONNode root, object obj, FieldProp f, BindingFlags bindingAttrBba)
        {
            var fieldjson = root;

            if (string.IsNullOrEmpty(f.attr.Name) == false)
            {
                fieldjson = string.IsNullOrEmpty(f.attr.Group) == false ? 
                    Get(root, $"{f.attr.Group}/{f.attr.Name}") : 
                    Get(root, f.attr.Name);
            }

            object valueObj = LoadObject(fieldjson, null, f.FieldType, bindingAttrBba);
            if (valueObj != null){
                f.SetValue(obj, valueObj);
            }
        }

		public static T ReadFromString<T>(string txt, BindingFlags bindingAttr) where T : new()
        {
			if (string.IsNullOrEmpty (txt))
				return default;

			var json = JSON.Parse(txt);
			return (T)LoadObject(json, null, typeof(T), bindingAttr);
		}
        public static object ReadFromString(string txt, object obj, Type type, BindingFlags bindingAttr)
        {
            if (string.IsNullOrEmpty(txt))
                return null;

            var json = JSON.Parse(txt);
            return LoadObject(json, obj, type, bindingAttr);
        }
        public static T ReadFromString<T>(string txt,Type type, BindingFlags bindingAttr)where T : new()
        {
            if (string.IsNullOrEmpty(txt))
                return default(T);

            var json = JSON.Parse(txt);
            return (T)LoadObject(json, null, type, bindingAttr);
        }
        public static bool ReadFromString<T>(string txt, T @object, BindingFlags bindingAttr) where T : new()
        {
            if (string.IsNullOrEmpty(txt))
                return false;

            var json = JSON.Parse(txt);

            return LoadObject(json, @object, typeof(T), bindingAttr) != null;
        }
        public static T ReadFromFile<T>(string localPath, BindingFlags bindingAttr) where T : class, new()
        {
            var txt = Util.ReadTextFile(localPath);
            if (string.IsNullOrEmpty(txt))
                return null;

            var json = JSON.Parse(txt);
            return (T)LoadObject(json, null, typeof(T), bindingAttr);
        }

        public static object LoadObject(JSONNode root, object obj, Type type, BindingFlags bindingAttr)
        {
            if (root == null){
                return null;
            }

            if (type.IsEnum)
            {
				obj = ToEnum (type, root);
			}
            else if (type == typeof(bool))
            {
				obj = ToBool (root);
			}
            else if (type == typeof(int))
            {
				obj = ToInt (root);
			}
            else if (type == typeof(uint))
            {
                obj = ToUInt(root);
            }
            else if (type == typeof(float))
            {
				obj = ToFloat (root);
			}
            else if (type == typeof(double))
            {
				obj = ToDouble (root);
			}
            else if (type == typeof(long))
            {
                obj = ToLong(root);
            }
            else if (type == typeof(ulong))
            {
                obj = ToUlong(root);
            }
            else if(type == typeof(short))
            {
                obj = Toshort(root);
            }
            else if (type == typeof(string))
            {
                if (root == null || (string.IsNullOrEmpty(root.Value) || root.Value == JsonReader.NULL_STR))
                {
                    obj = null;
                }
                else
                {
                    obj = root.Value;
                }
			}
            else if (type == typeof(System.DateTime)) {
				obj = ToDateTime(root);
            }
            //else if (type == typeof(System.TimeSpan))
            //{
            //    obj = ToTimeSpan(root);
            //}
            else if(type == typeof(UnityEngine.Vector2))
            {
                obj = ToVector2(root);
            }
            else if (Util.IsNullable(type))
            {
				if (root == null || (string.IsNullOrEmpty(root.Value) || root.Value.ToLower() == JsonReader.NULL_STR) && root.Count == 0)
                {
					obj = null;
				}
                else
                {
					var argType = type.GetGenericArguments()[0];
					var arg = LoadObject(root, null, argType, bindingAttr);
					obj = System.Activator.CreateInstance(type, arg);
				}
			}
            else if (type.IsArray && type.GetElementType() == typeof(byte))
            {
				if (string.IsNullOrEmpty(root.Value)) {
					obj = null;
				}
                else {
					obj = System.Convert.FromBase64String(root.Value);
				}
			}
            else if (Util.IsGenericList(type))
            {
                if(root.Count > 0)
                {
                    if (obj == null)
                    {
                        obj = System.Activator.CreateInstance(type);
                    }

                    var argType = type.GetGenericArguments()[0];
                    var list    = (IList)obj;
                    for (var i = 0; i < root.Count; ++i)
                    {
                        list.Add(LoadObject(root[i], null, argType, bindingAttr));
                    }
                }
            }
            else if(Util.IsArray(type) == true)
            {
                if (root.Count > 0)
                {
                    if (obj == null)
                    {
                        obj = System.Activator.CreateInstance(type, root.Count);
                    }

                    var argType = type.GetElementType();
                    var array = (IList)obj;
                    for (var i = 0; i < root.Count; ++i)
                    {
                        array[i] = LoadObject(root[i], null, argType, bindingAttr);
                    }
                }
            }
            else if (Util.IsGenericListDicKeyValue(type))
            {
                if (root.Count > 0)
                {
                    if (obj == null)
                    {
                        obj = System.Activator.CreateInstance(type);
                    }

                    var argKey      = type.GetGenericArguments()[0];
                    var argValue    = type.GetGenericArguments()[1];
                    var dic         = (IDictionary)obj;
                    for (var i = 0; i < root.Count; ++i)
                    {
                        var v = root.GetKeyValueAt(i);
                        if(v != null)
                        {
                            var value_ptr = v.Value;
                            var key     = LoadObject(value_ptr.Key, null, argKey, bindingAttr);
                            var value   = LoadObject(value_ptr.Value, null, argValue, bindingAttr);
                            dic.Add(key, value);
                        }
                    }
                }
            }
            else if (Util.IsCustomCollection(type) == true)
            {
                var rootcount = root.Count;
                if (rootcount > 0)
                {
                    obj ??= System.Activator.CreateInstance(type);
                    var collection  = (ICustomSerializableCollection)obj;
                    var argTypes    = type.GetGenericArguments();
                    var list        = new System.Collections.Generic.List<object>() { Capacity = rootcount };

                    for (var i = 0; i < rootcount; ++i)
                    {
                        list.Add(LoadObject(root[i], null, argTypes[1], bindingAttr));
                    }

                    collection.AfterDeserializeCollection(list);
                }
            }
            else
            {
                if (root == null || root.Value == null || root.Value == JsonReader.NULL_STR)
                {
                    obj = null;
                }
                else
                {
                    if (Util.IsDataSerializable(type))
                    {
                        if (obj == null)
                        {
                            obj = System.Activator.CreateInstance(type);
                        }

                        Util.EnumFields(root, obj, instance, bindingAttr);
                    }
                    else
                    {
#if UNITY_EDITOR
                        throw new System.ArgumentException(
                            string.Format("JsonReader.LoadObject - not serializable (t={0}, g={1})",
                                type, type.IsGenericType),
                            type.ToString());
#else
                        return null;
#endif
                    }
                }   
			}
			
			return obj;
		}
		
		static JSONNode Get(JSONNode root, string name) {
			var i = name.IndexOf ('/');
			if (i > 0) {
				var n1 = name.Substring(0, i);
				var n2 = name.Substring(i+1);
				return Get (root[n1], n2);
			} else {
				return root[name];
			}
		}
		
		static float ToFloat(JSONNode root) {
			var s = root.Value.TrimEnd ('%').Replace(",", "");
			if (string.IsNullOrEmpty (s))
				return 0;
			
			float v;
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) {
				return v;
			} else {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToFloat", root.Value);
#else
                return 0;
#endif
            }
        }
        static ulong ToUlong(SimpleJSON.JSONNode root)
        {
            var s = root.Value.TrimEnd('%').Replace(",", "");
            if (string.IsNullOrEmpty(s))
                return 0;

            ulong v;
            if (ulong.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            else
            {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToUlong", root.Value);
#else
                return 0;
#endif
            }
        }
        static long ToLong(JSONNode root)
        {
            var s = root.Value.TrimEnd('%').Replace(",", "");
            if (string.IsNullOrEmpty(s))
                return 0;

            long v;
            if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            else
            {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToLong", root.Value);
#else
                return 0;
#endif
            }
        }
		static double ToDouble(JSONNode root) {
			var s = root.Value.TrimEnd ('%').Replace(",", "");
			if (string.IsNullOrEmpty (s))
				return 0;

			double v;
			if (double.TryParse (s, NumberStyles.Any, CultureInfo.InvariantCulture, out v)) {
				return v;
			} else {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToDouble", root.Value);
#else
                return 0;
#endif
            }
        }

		static bool ToBool(JSONNode root) {
			var s = root.Value;
			if (string.IsNullOrEmpty (s))
				return false;

			return s.ToLower () == "true";
		}

		static int ToInt(JSONNode root)
        {
			var s = root.Value.TrimEnd ('%').Replace(",", "");
			if (string.IsNullOrEmpty (s))
				return 0;
			
			int v;
			if (int.TryParse (s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) {
				return v;
			}
            
            else {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToInt", root.Value);
#else
                return 0;
#endif
            }
        }
       
        static uint ToUInt(JSONNode root)
        {
            var s = root.Value.TrimEnd('%').Replace(",", "");
            if (string.IsNullOrEmpty(s))
                return 0;

            uint v;
            if (uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture,out v))
            {
                return v;
            }
            else
            {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToInt", root.Value);
#else
                return 0;
#endif
            }
        }

        static short Toshort(SimpleJSON.JSONNode root)
        {
            var s = root.Value.TrimEnd('%').Replace(",", "");
            if (string.IsNullOrEmpty(s))
                return 0;

            short v;
            if (short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            else
            {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToUlong", root.Value);
#else
                return 0;
#endif
            }
        }
        static SCC.Math.Int2 ToInt2(JSONNode root) {
			var s = root.Value;
			if (string.IsNullOrEmpty (s)) {
				return SCC.Math.Int2.zero;
			}

			var i = s.IndexOf (',');
			if (i > 0){
				return new SCC.Math.Int2(int.Parse(s.Substring(0, i), NumberStyles.Integer, CultureInfo.InvariantCulture),
                    int.Parse(s.Substring(i+1), NumberStyles.Integer, CultureInfo.InvariantCulture));
			}
            else {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToInt2", s);
#else
                return new SCC.Math.Int2();
#endif
            }
        }
		
		static SCC.Math.IntRange ToIntRange(JSONNode root) {
			var s = root.Value;
			var i = s.IndexOf ('~');
			if (i > 0) {
				return SCC.Math.IntRange.Make(int.Parse(s.Substring(0, i), NumberStyles.Integer, CultureInfo.InvariantCulture),
                    int.Parse(s.Substring(i+1), NumberStyles.Integer, CultureInfo.InvariantCulture));
			} else {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToIntRange", s);
#else
                return new SCC.Math.IntRange();
#endif
            }
        }
		
		static SCC.Math.FloatRange ToFloatRange(JSONNode root)
        {
            var s = root.Value;
            var i = s.IndexOf('~');
            return SCC.Math.FloatRange.Make (float.Parse(s.Substring(0, i), NumberStyles.Float, CultureInfo.InvariantCulture), 
                float.Parse(s.Substring(i+1), NumberStyles.Float, CultureInfo.InvariantCulture));
		}
        static SCC.Math.LongRange ToLongRange(JSONNode root)
        {
            var s = root.Value;
            var i = s.IndexOf('~');
            if (i > 0)
            {
                return SCC.Math.LongRange.Make(long.Parse(s.Substring(0, i), NumberStyles.Any, CultureInfo.InvariantCulture), 
                    long.Parse(s.Substring(i+1), NumberStyles.Any, CultureInfo.InvariantCulture));
            }
            else
            {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToLongRange", s);
#else
                return new SCC.Math.LongRange();
#endif
            }
        }
        static float PercentToFloat(JSONNode root) {
			var f = ToFloat(root) / 100.0f;
			
			if (f < 0 || f > 1) {
#if UNITY_EDITOR
                throw new System.ArgumentException ("PercentToFloat", root.Value);
#else
                return 0;
#endif
            }

            return f;
		}
		static object ToEnum(System.Type type, string s)
        {
            if (type.IsDefined(typeof(FlagsAttribute), false))
            {
                var pattern = @"([\w]+)+";
                var group   = System.Text.RegularExpressions.Regex.Matches(s, pattern);
                ulong flag  = 0;
                foreach (var re in group)
                {
                    foreach (var v in System.Enum.GetValues(type))
                    {
                        var f = type.GetField(System.Enum.GetName(type, v));

                        if(string.Equals(f.Name, re.ToString(), System.StringComparison.OrdinalIgnoreCase))
                        {
                            flag |= Convert.ToUInt64(System.Enum.Parse(type,f.Name));
                        }
                        else
                        {
                            var attrs = f.GetCustomAttributes(typeof(DataNameAttribute), false);
                            foreach (DataNameAttribute a in attrs)
                            {
                                if (string.Equals(a.Name, re.ToString(), System.StringComparison.OrdinalIgnoreCase))
                                {
                                    flag |= Convert.ToUInt64(System.Enum.Parse(type, f.Name));
                                }
                            }
                        }
                    }
                }

                return System.Enum.ToObject(type, flag);
            }
            else
            {
                foreach (var v in System.Enum.GetValues(type))
                {
                    var f = type.GetField(System.Enum.GetName(type, v));
                    var attrs = f.GetCustomAttributes(typeof(DataNameAttribute), false);

                    foreach (DataNameAttribute a in attrs)
                    {
                        if (string.Equals(a.Name, s, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return System.Enum.ToObject(type, v);
                        }
                    }
                }
            }
			UnityEngine.Debug.LogWarning(string.Format("ToEnum: {0}.Parse('{1}')", type.Name, s));
			if (string.IsNullOrEmpty (s))
				return null;

			return System.Enum.Parse (type, s);
		}
		
		static System.DateTime ToDateTime(JSONNode root)
        {
			if (string.IsNullOrEmpty (root.Value)) {
				return new System.DateTime();
			}

            //"2008-09-15 21:30";
            if (System.DateTime.TryParseExact(root.Value, "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime value) == false)
            {
                if (System.DateTime.TryParse(root.Value,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out value) == false)
                {
#if UNITY_EDITOR
                    throw new System.ArgumentException("DateTime", root.Value);
#else
                    value = System.DateTime.MinValue;
                    UnityEngine.Debug.LogError($"ToDateTime ERROR={root.Value}");
#endif
                }
            }
            return value;
		}
        static System.TimeSpan ToTimeSpan(JSONNode root)
        {
            if (string.IsNullOrEmpty(root.Value)){
                return new System.TimeSpan();
            }

            return System.TimeSpan.Parse(root.Value);
        }

        static UnityEngine.Vector2 ToVector2(JSONNode root)
        {
            if (string.IsNullOrEmpty(root.Value))
            {
                return new UnityEngine.Vector2();
            }

            var s = root.Value;
            var i = s.IndexOf(',');

            if (float.TryParse(s.Substring(0,i), NumberStyles.Float, CultureInfo.InvariantCulture,out float x) == false || 
                float.TryParse(s.Substring(i+1), NumberStyles.Any, CultureInfo.InvariantCulture, out float  y) == false)
            {
#if UNITY_EDITOR
                throw new System.ArgumentException("ToVector2", root.Value);
#else
                return new UnityEngine.Vector2();
#endif

            }

            return new UnityEngine.Vector2(x, y);
        }
    }
}


