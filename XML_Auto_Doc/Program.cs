using System;
using System.IO;

namespace XML_Auto_Doc
{
    internal class Program
    {
        public const bool LOCAL_MODE = false;

        public static void FatalError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
            Environment.Exit(-1);
        }

        static int Main(string[] args)
        {
            string output = "./Output.html";
            string title = ResourceLoader.TryReadAsString("XML_Auto_Doc.Title.txt") ?? "<title error>";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Created by Epicguru. Based on original by milon.\n");

            if (args.Length < 2)
            {
                Console.WriteLine($"Missing arguments: Argument #0 must be version, argument #1 must be def folder. Additional folders can be added after argument 1.");
                return -1;
            }

            string version = args[0].Trim();
            string[] paths = new string[args.Length - 1];
            Array.Copy(args, 1, paths, 0, paths.Length);

            var parser = new XmlParser(paths);
            parser.LoadAll();

            var gen = new HtmlGen();
            
            File.WriteAllText(output, gen.Generate(parser, version, LOCAL_MODE));
            Console.ReadLine();
            return 0;
        }
    }
}
