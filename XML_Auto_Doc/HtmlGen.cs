using ShellProgressBar;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XML_Auto_Doc
{
    public class HtmlGen
    {
        private string MakeLinkName(string name)
        {
            return name;
        }

        private string MakeTooltip(IEnumerable<Source> sources)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("Examples:").AppendLine();
            foreach(var src in sources)
            {
                string filePath = src.File;
                filePath = filePath.Substring(filePath.IndexOf("Defs") + 5);
                str.Append("Def: ").Append(src.DefName ?? "???").Append(", File: ").AppendLine(filePath);
            }
            return str.ToString().TrimEnd();
        }

        private HashSet<string> alreadyLinked = new HashSet<string>();

        private string MakeSee(IEnumerable<Source> sources, bool links)
        {
            alreadyLinked.Clear();
            StringBuilder str = new StringBuilder();            
            str.Append("(see: ");

            bool first = true;
            int i = 0;
            foreach(var src in sources)
            {
                if (!alreadyLinked.Add(src.File))
                    continue;

                if(links)
                    str.Append("<a href=\"file:///").Append(src.File.Replace('/', '\\')).Append("\" target=\"popup\">");
                else
                    str.Append("<a>");
                if (!first)
                    str.Append(", ");
                str.Append(new FileInfo(src.File).Name).Append("</a>");

                first = false;

                if(i++ > 4)
                {
                    str.Append(" ...");
                    break;
                }
            }
            str.Append(")");
            return str.ToString();
        }

        public string Generate(XmlParser parser, string version, bool localMode)
        {
            StringBuilder str = new StringBuilder(1024 * 1024);

            string header = ResourceLoader.TryReadAsString("XML_Auto_Doc.HtmlHeader.html");

            str.Append(header.Replace("[[TITLE]]", $"Rimworld Auto Documentation <span>for {version}</span>").Replace("[[SUB_TITLE]]", "Made by Epicguru, based on milon's work."));

            // Side links.
            using (var bar = new ProgressBar(parser.ElementCount, "Make side bar", new ProgressBarOptions() { ProgressBarOnBottom = true }))
            {
                foreach (var value in parser.AllElementsSorted)
                {
                    str.AppendLine($"<a id=\"Side_Link\" href=\"#{MakeLinkName(value.Name)}\"> > {value.Name}</a>");
                    bar.Tick();
                }
            }            

            str.AppendLine("</div>");
            str.AppendLine("<div id=\"HTML_Info\">");

            // Main pages
            using (var bar = new ProgressBar(parser.ElementCount, "Make main content", new ProgressBarOptions() { ProgressBarOnBottom = true }))
            {
                foreach (var value in parser.AllElementsSorted)
                {
                    str.AppendLine($"<h2 id=\"{MakeLinkName(value.Name)}\">{value.Name}</h2>");

                    // Parents.
                    if (value.Parents.Count > 0)
                    {
                        str.AppendLine($"<h3>Parent{(value.Parents.Count > 1 ? "s" : "")}:</h3>");
                        bool first = true;
                        foreach (var parent in value.Parents)
                        {
                            str.Append($"<a href=\"#{MakeLinkName(parent.Name)}\">{(first ? "" : ",  ")}{parent.Name}</a>");
                            first = false;
                        }
                    }

                    // Children:
                    if (value.Children.Count > 0)
                    {
                        bool first = true;
                        str.AppendLine($"<h3>Children:</h3>");
                        foreach (var child in value.Children)
                        {
                            str.Append($"<a href=\"#{MakeLinkName(child.Name)}\">{(first ? "" : ",  ")}{child.Name}</a>");
                            first = false;
                        }
                    }

                    // Values:
                    if (value.Values.Count > 0)
                    {
                        str.Append("<h3>Values:</h3>");
                        bool group = value.GroupedValues?.Count > 1;

                        if (group)
                        {
                            foreach (var pair in value.GroupedValues)
                            {
                                str.Append($"<h4>When used in {pair.Key.Name}:</h4>");
                                foreach (var grouped in value.SortValues(pair.Value))
                                {
                                    //str.AppendLine($"<li title=\"{MakeTooltip(grouped.sources)}\">{grouped.value}</li>");
                                    str.AppendLine($"<li title=\"{MakeTooltip(grouped.sources)}\">");
                                    str.Append($"<span style=\"text-align: left; width:250px; display: inline-block;\">{grouped.value}</span>");
                                    str.Append($"<code style=\"text-align: left; width:max-content - 350;  display: inline-block;\">{MakeSee(grouped.sources, localMode)}</code></li>");
                                }
                            }
                        }
                        else
                        {
                            foreach (var key in value.ValuesKeysSorted)
                            {
                                var v = value.Values[key];
                                str.AppendLine($"<li title=\"{MakeTooltip(v)}\">");
                                str.Append($"<span style=\"text-align: left; width:250px; display: inline-block;\">{key}</span>");
                                str.Append($"<code style=\"text-align: left; width:max-content - 350;  display: inline-block;\">{MakeSee(v, localMode)}</code></li>");
                            }
                        }
                    }

                    str.AppendLine("<hr>");
                    bar.Tick();
                }
            }

            str.AppendLine("</div>");

            str.Append("</body>");

            return str.ToString();
        }
    }
}
