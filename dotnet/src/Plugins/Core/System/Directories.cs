

using System.Text.Json;

namespace Agience.Plugins.Core.System
{
    public class Directories
    {        

        public List<string> ListFiles(string directory)
        {
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).ToList();
            string jsonOutput = JsonSerializer.Serialize(files, new JsonSerializerOptions { WriteIndented = true });
            
            return files;
        }
    }
}
