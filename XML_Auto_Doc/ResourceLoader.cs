using System.IO;
using System.Reflection;

namespace XML_Auto_Doc
{
    public static class ResourceLoader
    {
        public static string TryReadAsString(string path, string defaultValue = null)
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
