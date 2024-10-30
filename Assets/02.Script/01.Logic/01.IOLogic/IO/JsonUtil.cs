using System;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;


namespace SCC.JsonIO 
{
    //!<========================================================================================
    //FieldProp
    //!<========================================================================================
    public struct FieldProp 
	{
		public FieldInfo			field;
		public PropertyInfo			prop;
		public DataNameAttribute	attr;

		public static FieldProp Create(FieldInfo field, DataNameAttribute attr) 
		{
			return new FieldProp { field = field, attr = attr };
		}

        public static FieldProp Create(PropertyInfo prop, DataNameAttribute attr) 
            => new FieldProp { prop = prop, attr = attr };

        public Type FieldType
            => this.field != null ? field.FieldType : this.prop != null ? prop.PropertyType : null;

        public readonly object GetValue(object obj) => this.field != null ? field.GetValue(obj) : this.prop != null ? this.prop.GetValue(obj, null) : null;

        public readonly void SetValue(object obj, object v)
        {
            if (this.field != null)
            {
				field.SetValue(obj, v);
			} 
			else if (this.prop != null) 
			{
                prop.SetValue(obj, v, null);
			}
		}
	}

    //!<========================================================================================
    //IEnumFieldsWithJson
    //!<========================================================================================
    public interface IEnumFieldsWithJson 
	{
		void OnField (JSONNode root, object obj, FieldProp field, BindingFlags bindingAttr);
    }

    //!<========================================================================================
    //ICustomSerializableCollection
    //!<========================================================================================
    public interface ICustomSerializableCollection
    {
		IReadOnlyList<object> BeforeSerializeCollection();
		bool AfterDeserializeCollection(IReadOnlyList<object> collection);
    }

    //!<========================================================================================
    //CustomSerializableCollection
    //!<========================================================================================
    public abstract class CustomSerializableCollection<T1,T2> : ICustomSerializableCollection
    {
        public abstract IReadOnlyList<object> BeforeSerializeCollection();
        public abstract bool AfterDeserializeCollection(IReadOnlyList<object> collection);
    }
    //!<========================================================================================
    //Util
    //!<========================================================================================
    public class Util 
	{
		
		public static string AppDataFilePath(string localPath) 
		{
			return System.IO.Path.Combine (UnityEngine.Application.persistentDataPath, localPath + ".appdata");
		}
        public static string FilePath(string localPath)
        {
            return System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, localPath);
        }
        public static string ReadTextFile(string localPath) 
		{
			var filePath = AppDataFilePath(localPath);
			if (System.IO.File.Exists (filePath) == false) 
			{
				return "";
			}
			
			var txt = System.IO.File.ReadAllText (filePath);
			return txt;
		}
		
		public static void WriteTextFile(string localPath, string txt) 
		{
			var filePath = AppDataFilePath(localPath);
			System.IO.File.WriteAllText (filePath, txt);
		}
		
		public static void EnumFields(
			JSONNode root, object obj, IEnumFieldsWithJson cb, BindingFlags bindingAttr)
        {
			var type	= obj.GetType ();
			var fields	= type.GetFields(bindingAttr);
			if(fields?.Length > 0)
			{
				var count = fields.Length;
                for (var i = 0; i < count; ++i)
                {
					var field = fields[i];
                    var attrs = field.GetCustomAttributes(typeof(DataNameAttribute), false);
                    if (attrs.Length > 0)
                    {
                        if (attrs[0] is DataNameAttribute a)
                        {
                            cb.OnField(root, obj, FieldProp.Create(field, a), bindingAttr);
                        }
                    }
                }
			}

			var properties = type.GetProperties();
			if(properties?.Length > 0)
			{
				var count = properties.Length;
				for(var i = 0; i < count; ++i)
                {
					var prop	= properties[i];
                    var attrs	= prop.GetCustomAttributes(typeof(DataNameAttribute), false);
                    if (attrs.Length > 0)
                    {
                        if (attrs[0] is DataNameAttribute a)
                        {
                            cb.OnField(root, obj, FieldProp.Create(prop, a), bindingAttr);
                        }
                    }
                }
			}
		}
		
		public static void EnumFields(Type type, System.Action<FieldProp> cb, BindingFlags bindingAttr)
        {
            var fields = type.GetFields(bindingAttr);
            if (fields?.Length > 0)
            {
                var count   = fields.Length;
                for (var i  = 0; i < count; ++i)
                {
                    var field = fields[i];
                    var attrs = field.GetCustomAttributes(typeof(DataNameAttribute), false);
                    if (attrs.Length > 0)
                    {
                        if (attrs[0] is DataNameAttribute a)
                        {
                            cb(FieldProp.Create(field, a));
                        }
                    }
                }
            }
            var properties = type.GetProperties();
            if (properties?.Length > 0)
            {
                var count   = properties.Length;
                for (var i  = 0; i < count; ++i)
                {
                    var prop    = properties[i];
                    var attrs   = prop.GetCustomAttributes(typeof(DataNameAttribute), false);
                    if (attrs.Length > 0)
                    {
                        if (attrs[0] is DataNameAttribute a)
                        {
                            cb(FieldProp.Create(prop, a));
                        }
                    }

                }
            }
		}

		public static bool IsDataSerializable(Type type) 
		{
			return type.GetCustomAttributes (typeof(DataSerializableAttribute), false).Length > 0;
		}
		
		public static bool IsGenericList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition () == typeof(List<>);
		}
        public static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        public static bool IsGenericListDicKeyValue(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public static bool IsNullable(Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition () == typeof(Nullable<>);
		}
		public static bool IsCustomCollection(Type type)
		{
			if(type.IsGenericType == true && type.BaseType != null)
			{
                if (type.BaseType.GetGenericTypeDefinition() == typeof(CustomSerializableCollection<,>) == true)
				{
					return true;
				}
            }
			return false;
		}

    }
	
}

