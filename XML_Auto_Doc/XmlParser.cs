using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace XML_Auto_Doc
{
    public class XmlParser
    {
        public int TotalFiles { get; }
        public readonly string[] DirectoryPaths;

        public int ElementCount => elements.Count;
        public IEnumerable<AnalysisElement> AllElementsSorted
        {
            get
            {
                var arr = elements.Values.ToArray();
                Array.Sort(arr, (a, b) => a.Name.CompareTo(b.Name));
                return arr;
            }
        }

        public HashSet<string> IgnoreTags = new HashSet<string>()
        {
            "description",
            "label"
        };

        private Dictionary<string, AnalysisElement> elements = new Dictionary<string, AnalysisElement>();
        private List<XmlDocument> docs;
        private List<string> errors = new List<string>();

        public XmlParser(params string[] dirs)
        {
            DirectoryPaths = dirs;
            Console.WriteLine("Finding xml files...");
            foreach (var dir in dirs)            
                foreach (var _ in Directory.EnumerateFiles(dir, "*.xml", SearchOption.AllDirectories))
                    TotalFiles++;
            Console.WriteLine($"Found {TotalFiles} xml files.");
        }

        public void LoadAll()
        {
            var bar = new ProgressBar(2, "Analyze defs", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true,
            });
            var linkBar = bar.Spawn(1, "Link defs", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true,
            });
            var parseBar = bar.Spawn(TotalFiles, "Parse defs", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true,
            });            

            errors.Clear();
            docs = new List<XmlDocument>(TotalFiles);

            foreach (var dir in DirectoryPaths)
            {
                foreach (var filePath in Directory.EnumerateFiles(dir, "*.xml", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(filePath);
                    parseBar.Tick($"Parse defs: {info.Name}");
                    Parse(info);
                }
            }            
            bar.Tick();

            linkBar.MaxTicks = elements.Count * 2;
            foreach(var thing in elements)
            {
                linkBar.Tick($"Link defs: {thing.Key}");
                Link(thing.Value);
            }
            foreach (var thing in elements)
            {
                linkBar.Tick($"Group values: {thing.Key}");
                thing.Value.GroupValues();
            }
            bar.Tick();

            bar.Dispose();
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var error in errors)
            {
                Console.WriteLine(error.TrimEnd());
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Analyze(string name, bool grouped = true)
        {
            var elem = TryGet(name);
            if(elem == null)
            {
                Console.WriteLine("Not found!");
                return;
            }

            Console.WriteLine($"{elem.Name}");

            if (grouped && elem.GroupedValues?.Count > 1)
            {
                if (elem.GroupedValues.Count > 0)
                {
                    Console.WriteLine("Values:");
                    foreach (var tupple in elem.GroupedValues)
                    {
                        Console.WriteLine($"When part of '{tupple.Key.Name}':");

                        foreach (var pair in tupple.Value)
                        {
                            Console.WriteLine($" - '{pair.value}'");
                            foreach (var src in pair.sources)
                                Console.WriteLine($"    in {src.DefName ?? "???"} ({new FileInfo(src.File).Name})");
                        }
                    }
                }
            }
            else
            {
                if(elem.Values.Count > 0)
                {
                    Console.WriteLine("Values:");
                    foreach(var pair in elem.Values)
                    {
                        Console.WriteLine($" - '{pair.Key}'");
                        foreach (var src in pair.Value)
                            Console.WriteLine($"    in {src.DefName ?? "???"} ({new FileInfo(src.File).Name}, owner {src.Owner})");
                    }
                }
            }
            

            if(elem.Children.Count > 0)
            {
                Console.WriteLine("Children:");
                foreach (var pair in elem.Children)
                {
                    Console.WriteLine($" - {pair.Name}");                    
                }
            }

            if (elem.Parents.Count > 0)
            {
                Console.WriteLine("Parent(s):");
                foreach (var pair in elem.Parents)
                {
                    Console.WriteLine($" - {pair.Name}");
                }
            }
        }

        private void Parse(FileInfo file)
        {
            string rawText = File.ReadAllText(file.FullName);

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(rawText);
            }
            catch(Exception e)
            {
                errors.Add($"[{e.GetType().Name}] exception when trying to parse '{file.FullName}'");
                errors.Add(e.Message);
                return;
            }

            if (doc.DocumentElement.Name != "Defs")
                return;

            docs.Add(doc);

            Explore(doc.DocumentElement, null, file, null);
        }

        public static string MakeName(XmlElement e)
        {
            if (e == null)
                return null;

            if(e.Name == "li")
            {
                return $"{e.ParentNode.Name} [List]";
            }

            return e.Name;
        }

        private void Explore(XmlElement e, string defName, FileInfo file, string parentName)
        {
            if (e == null)
                return;

            if (IgnoreTags.Contains(e.Name))
                return;

            if(defName == null && e.HasChildNodes)
            {
                foreach(var child in e.ChildNodes)
                {
                    if(child is XmlElement ce && ce.Name == "defName")
                    {
                        defName = ce.InnerText;
                        break;
                    }
                }

                if (e.HasAttribute("Name"))
                {
                    defName = e.GetAttribute("Name");
                }
            }

            bool isValue = e.HasChildNodes && e.ChildNodes.Count == 1 && !e.ChildNodes[0].HasChildNodes;

            var self = GetOrCreate(e);
            self.AddParentRaw(parentName);

            if (isValue)
            {
                self.AddValue(e.ChildNodes[0].InnerText, file, defName, MakeName(e.ParentNode as XmlElement));
                return;
            }

            if (e.HasChildNodes)
            {
                foreach(var child in e.ChildNodes)
                {
                    if(child is XmlElement childE)
                    {
                        Explore(childE, defName, file, self.Name);
                    }
                }
            }
        }

        private void Link(AnalysisElement e)
        {
            if (e == null)
                return;           

            foreach(var value in e.ParentsRaw)
            {
                var found = TryGet(value);
                found.Children.Add(e);
                e.Parents.Add(found);
            }            
        }

        private AnalysisElement GetOrCreate(XmlElement e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (elements.TryGetValue(MakeName(e), out var found))
            {
                return found;
            }

            found = new AnalysisElement(e);
            elements.Add(found.Name, found);
            return found;
        }

        public AnalysisElement TryGet(string name)
        {
            if (elements.TryGetValue(name, out var found))
                return found;
            return null;
        }

        public AnalysisElement TryGet(XmlElement e)
        {
            string name = MakeName(e);
            foreach(var pair in elements)
            {
                if (pair.Value.Name == name)
                    return pair.Value;
            }
            return null;
        }
    }
}
