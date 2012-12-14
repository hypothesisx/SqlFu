using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace SqlFu.Internals
{
    internal class TableInfo
    {
        public string Name { get;  private set; }
        public string PrimaryKey { get; private set; }
        public string[] Excludes { get; private set; }
        public string SelectSingleSql { get; set; }
        public string InsertSql { get; set; }
        public string[] ConvertToString { get; private set; }
        public bool AutoGenerated { get; private set; }

        public TableInfo(string name)
        {
            Name = name;
            PrimaryKey = "Id";
            Excludes = new string[0];
            ConvertToString=new string[0];
            AutoGenerated = true;
        }
        public TableInfo(Type t)
        {
            if (t.IsValueType || t==typeof(object)) throw new InvalidOperationException("A table can't be System.Object or just a value");
            var tab = t.GetSingleAttribute<TableAttribute>();
            if (tab!=null)
            {
                Name = tab.Name;
                PrimaryKey = tab.PrimaryKey;
                AutoGenerated = tab.AutoGenerated;
            }
            else
            {
                Name = t.Name;
                PrimaryKey = "Id";
                AutoGenerated = false;
            }

            var exclude = new List<string>();
            var tstring = new List<string>();
            if (t != typeof(ExpandoObject))
            {
                foreach (var p in t.GetProperties())
                {
                    var qr = p.GetSingleAttribute<QueryOnlyAttribute>();
                    if (qr != null)
                    {
                        exclude.Add(p.Name);
                    }
                    var tos = p.GetSingleAttribute<InsertAsStringAttribute>();
                    if (tos != null)
                    {
                        tstring.Add(p.Name);
                    }
                }
            }
            Excludes = exclude.ToArray();
            ConvertToString = tstring.ToArray();
        }

        static ConcurrentDictionary<Type,TableInfo> _cache=new ConcurrentDictionary<Type, TableInfo>();
        public static TableInfo ForType(Type t)
        {
            TableInfo ti = null;
            if (!_cache.TryGetValue(t,out ti))
            {
                ti= new TableInfo(t);
                _cache.TryAdd(t, ti);
            }
            return ti;
        }



    }
}