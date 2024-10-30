using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;

namespace SCC.JsonIO
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class DataSerializableAttribute : Attribute
    {
		public DataSerializableAttribute()
        {

		}
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
	public class DataNameAttribute : Attribute
    {
		public string Name		{ get; set; }
		public string Group		{ get; set; }

		public DataNameAttribute(string name) {
			this.Name	= name;
			this.Group	= null;
		}
        public DataNameAttribute(string group,string name)
        {
            this.Group	= group;
            this.Name	= name;
        }
    }

}
