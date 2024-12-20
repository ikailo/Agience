using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agience.Plugins.Core.Code
{
    /*
    internal class Analysis
    {
        private readonly Files.Files _files;

        public Analysis(string workingDirectory)
        {
            _files = new Files.Files(workingDirectory);
        }
        

        // Review: Find TODO comments in code files
        public List<string> FindTodos(string directory)
        {
            var todos = new List<string>();
            var codeFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(file => new[] { ".cs", ".js", ".java", ".py", ".cpp", ".ts", ".go" }.Contains(Path.GetExtension(file)))
                .ToList();

            foreach (var file in codeFiles)
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("TODO"))
                    {
                        todos.Add($"{file}:{i + 1} {lines[i].Trim()}");
                    }
                }
            }

            _files.WriteToFile("todos.txt", string.Join(Environment.NewLine, todos));
            return todos;
        }

        // Architecture: Analyze folder structure
        public string AnalyzeArchitecture(string directory)
        {
            var folders = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);
            var folderTree = folders.Select(folder => folder.Replace(directory, "").TrimStart(Path.DirectorySeparatorChar));
            var structure = string.Join(Environment.NewLine, folderTree);
            _files.WriteToFile("architecture.txt", structure);
            return structure;
        }


        // Summary: Summarize a file
        public string SummarizeFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var summary = $"File: {filePath}\nLines: {lines.Length}\nPreview:\n{string.Join(Environment.NewLine, lines.Take(10))}";
            _files.WriteToFile($"{Path.GetFileName(filePath)}_summary.txt", summary);
            return summary;
        }
    }*/
}
