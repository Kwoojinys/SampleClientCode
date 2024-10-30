using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;


namespace SCC.JsonIO {
	
	public class JsonWriter : IEnumFieldsWithJson
    {
		static JsonWriter instance = new JsonWriter ();
		
		public void OnField(JSONNode root, object obj, FieldProp f, BindingFlags bindingAttr)
        {
			var valObj = f.GetValue (obj);
			
			if (string.IsNullOrEmpty (f.attr.Name))
            {
				ToJson(root, valObj, bindingAttr);
			}
            else
            {
                if (valObj == null)
                {
                    //!< 저장데이터최적화를 위해 NULL 데이터는 기입하지 말도록 하자

                    //if (Util.IsGenericList(f.FieldType) || Util.IsGenericListDicKeyValue(f.FieldType) ||
                    //    f.FieldType.IsClass == true || Util.IsNullable(f.FieldType))
                    //{
                    //    Set(root, f.attr.Name, new JSONNull());
                    //}
                    //else
                    //{
                    //    Set(root, f.attr.Name, new JSONClass());
                    //}
                }
                else
                {
                    if(string.IsNullOrEmpty(f.attr.Group) == false)
                    {
                        Set(root,$"{f.attr.Group}/{f.attr.Name}", ToJson(null, valObj, bindingAttr));
                    }
                    else
                    {
                        Set(root, f.attr.Name, ToJson(null, valObj, bindingAttr));
                    }
                }
			}
		}

		public static string WriteToString(object obj, BindingFlags bindingAttr)
        {
			var root = ToJson (null, obj, bindingAttr);
			var txt = root.ToString ();
			return txt;
		}

		public static void WriteToFile(string localPath, object obj, BindingFlags bindingAttr)
        {
			var root = ToJson (null, obj, bindingAttr);
			var txt = root.ToString ();
			Util.WriteTextFile (localPath, txt);
		}

		public static string ToJsonString(object obj, BindingFlags bindingAttr)
        {
			var root = ToJson (null, obj, bindingAttr);
			return root.ToString ();
		}

		public static JSONNode ToJson(JSONNode root, object obj, BindingFlags bindingAttr)
        {
			if (obj == null) 
            {
				return new JSONObject();
			}
			
			var type = obj.GetType ();
			
			if (type.IsEnum)
            {
               return ToJsonEnum (type, obj);
			}
            else if(type == typeof(bool))
            {
                return new JSONBool(obj.ToString().ToLower());
            }
            else if (type == typeof(short))
            {
                var v = (short)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(int))
            {
                var v = (int)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(uint))
            {
                var v = (uint)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(float))
            {
                var v = (float)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(double))
            {
                var v = (double)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(long))
            {
                var v = (long)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(ulong))
            {
                var v = (ulong)obj;
                return new JSONNumber(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (type == typeof(string))
            {
                return new JSONString(obj.ToString());
            }
            else if (type == typeof(System.DateTime))
            {
				var dt = (System.DateTime)obj;
                return dt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
			}
            else if (type == typeof(SCC.Math.Int2))
            {
				var v = (SCC.Math.Int2)obj;
				return new JSONString (string.Format ("{0},{1}", v.x, v.y));
			}
            else if (type == typeof(SCC.Math.IntRange))
            {
				var v = (SCC.Math.IntRange)obj;
				return new JSONString(string.Format ("{0}~{1}", v.min, v.max));
			}
            else if (type == typeof(SCC.Math.FloatRange))
            {
				var v = (SCC.Math.FloatRange)obj;
				return new JSONString(string.Format ("{0}~{1}", v.min, v.max));
			}
            else if(type == typeof(SCC.Math.LongRange))
            {
                var v = (SCC.Math.LongRange)obj;
                return new JSONString(string.Format("{0}~{1}", v.min, v.max));
            }
            else if (type == typeof(SCC.Math.ULongRange))
            {
                var v = (SCC.Math.ULongRange)obj;
                return new JSONString(string.Format("{0}~{1}", v.min, v.max));
            }
            else if (type.IsArray && type.GetElementType () == typeof(byte))
            {
				var v = (byte[])obj;
				return new JSONString(System.Convert.ToBase64String (v));
			}
            else if (Util.IsGenericList(type))
            {
				//var argType = type.GetGenericArguments()[0];
				var list = (IList)obj;
				root = new JSONArray();
				for(var i = 0 ; i < list.Count ; ++i) {
					root.Add(ToJson(null, list[i], bindingAttr));
				}
				return root;
			}
            else if (Util.IsArray(type) == true)
            {
                var list = (IList)obj;
                root = new JSONArray();
                for (var i = 0; i < list.Count; ++i)
                {
                    root.Add(ToJson(null, list[i], bindingAttr));
                }
                return root;
            }
            else if (Util.IsGenericListDicKeyValue(type))
            {
                var dic = (IDictionary)obj;
                root = new JSONObject();

                foreach (var entry in dic.Keys){
                    root.Add(ToJson(null, entry, bindingAttr), ToJson(null, dic[entry], bindingAttr));
                }
                return root;
            }
            else if (Util.IsCustomCollection(type) == true)
            {
                var collection = ((ICustomSerializableCollection)obj).BeforeSerializeCollection();
                root = new JSONArray();
                for (var i = 0; i < collection.Count; ++i)
                {
                    root.Add(ToJson(null, collection[i], bindingAttr));
                }
                return root;
            }
            else
            {
				
				if (root == null) 
                {
					root = new JSONObject();
				}
				
				Util.EnumFields (root, obj, instance, bindingAttr);
				return root;
			}
		}
		
		static void Set(JSONNode root, string name, JSONNode v)
        {
			var i = name.IndexOf ('/');
			if (i > 0) 
            {
				var n1 = name.Substring(0, i);
				var n2 = name.Substring(i+1);
				var subNode = root[n1];
				if (subNode == null) 
                {
					subNode = new JSONObject();
					root[n1] = subNode;
				}
				
				Set (subNode, n2, v);
			} else 
            {
				root[name] = v;
			}
		}
		
		static JSONString ToJsonEnum(System.Type type, object obj)
        {
			var s   = "";
            var str = obj.ToString();
            if (type.IsDefined(typeof(FlagsAttribute), false))
            {
                var pattern = @"([\w]+)+";
                var group = System.Text.RegularExpressions.Regex.Matches(str, pattern);
                foreach (var es in group)
                {
                    var memberInfo = type.GetMember(es.ToString());
                    var attrs = memberInfo[0].GetCustomAttributes(typeof(DataNameAttribute), false);
                    if (attrs == null || attrs.Length == 0)
                    {
                        if(s.Length > 0)
                        {
                            s += string.Format($@"|{es.ToString()}");
                        }
                        else
                        {
                            s += es.ToString();
                        }
                    }
                    else
                    {
                        if (s.Length > 0)
                        {
                            s += string.Format($@"|{(attrs[0] as DataNameAttribute).Name}");
                        }
                        else
                        {
                            s += (attrs[0] as DataNameAttribute).Name;
                        }
                    }
                }
            }
            else
            {
                var memberInfo = type.GetMember(str);
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DataNameAttribute), false);
                if (attrs == null || attrs.Length == 0)
                {
                    s = str;
                }
                else
                {
                    s = (attrs[0] as DataNameAttribute).Name;
                }
            }
			return new JSONString(s);
		}
	}
}


