using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace XML_Auto_Doc
{
    public class AnalysisElement
    {
        public string Name;
        public bool IsList;

        public IEnumerable<string> ParentsRaw => parentsTemp;

        public HashSet<AnalysisElement> Children = new HashSet<AnalysisElement>();
        public HashSet<AnalysisElement> Parents = new HashSet<AnalysisElement>();

        private HashSet<string> parentsTemp = new HashSet<string>();

        public IEnumerable<string> ValuesKeysSorted
        {
            get
            {
                bool areAllFloats = true;
                foreach(var key in Values.Keys)
                {
                    if(!float.TryParse(key, out _))
                    {
                        areAllFloats = false;
                        break;
                    }
                }

                var arr = Values.Keys.ToArray();
                if (areAllFloats)
                {
                    Array.Sort(arr, (a, b) =>
                    {
                        float af = float.Parse(a);
                        float bf = float.Parse(b);
                        return af.CompareTo(bf);
                    });
                }
                else                
                    Array.Sort(arr);

                return arr;
            }
        }

        public Dictionary<string, HashSet<Source>> Values = new Dictionary<string, HashSet<Source>>();
        public Dictionary<AnalysisElement, List<(string value, List<Source> sources)>> GroupedValues;

        public AnalysisElement(XmlElement e)
        {
            Name = XmlParser.MakeName(e);
            IsList = e.Name == "li";
        }

        public IEnumerable<(string value, List<Source> sources)> SortValues(List<(string value, List<Source> sources)> list)
        {
            string[] raw = new string[list.Count];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = list[i].value;
            }

            bool areAllFloats = true;
            foreach (var key in raw)
            {
                if (!float.TryParse(key, out _))
                {
                    areAllFloats = false;
                    break;
                }
            }

            var real = list.ToArray();
            Array rawArr = raw;
            if (areAllFloats)
            {
                float[] floatArr = new float[raw.Length];
                for (int i = 0; i < floatArr.Length; i++)                
                    floatArr[i] = float.Parse(raw[i]);
                rawArr = floatArr;
            }

            Array.Sort(rawArr, real);
            return real;
        }

        public void AddParentRaw(string parentName)
        {
            if(parentName != null)
                parentsTemp.Add(parentName);
        }

        public void AddValue(string value, FileInfo file, string defName, string owner)
        {
            if(!Values.TryGetValue(value, out var found))
            {
                found = new HashSet<Source>(8);
                Values.Add(value, found);
            }

            Source src = new Source()
            {
                File = file.FullName,
                DefName = defName,
                Owner = owner
            };

            found.Add(src);
        }

        public void GroupValues()
        {
            GroupedValues = new Dictionary<AnalysisElement, List<(string value, List<Source> sources)>>();
            foreach (var pair in GetGroupedValues())
                GroupedValues.Add(pair.parent, pair.values);
        }

        private IEnumerable<(AnalysisElement parent, List<(string value, List<Source> sources)> values)> GetGroupedValues()
        {
            // To be called only after linking.

            foreach(var parent in Parents)
            {
                List<(string, List<Source>)> tempAll = new List<(string, List<Source>)>();
                foreach(var pair in Values)
                {
                    List<Source> tempSources = new List<Source>();
                    string value = pair.Key;
                    foreach(var src in pair.Value)
                    {
                        if(src.Owner == parent.Name)
                        {
                            tempSources.Add(src);
                        }
                    }

                    if (tempSources.Count > 0)
                        tempAll.Add((value, tempSources));
                }

                if (tempAll.Count != 0)
                    yield return (parent, tempAll);
            }
        }
    }

    public class Source
    {
        public string File;
        public string DefName;
        public string Owner;

        public override bool Equals(object obj)
        {
            return obj is Source src && File == src.File && DefName == src.DefName && Owner == src.Owner;
        }

        public override int GetHashCode()
        {
            return (File + DefName + Owner).GetHashCode();
        }
    }
}
