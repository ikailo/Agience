using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.Text
{
    internal class Text //: IAgiencePlugin
    {
        private const int DEFAULT_LENGTH = 4000;

        [KernelFunction, Description("Split text into chunks.")]
        public static string[] SplitText(

            [Description("The text to split.")]
            string text,

            [Description("The maximum size of each chunk.")]
            int maxLength = DEFAULT_LENGTH
         )
        {
            List<string> result = new List<string>();
            int start = 0;
            while (start < text.Length)
            {
                int length = Math.Min(maxLength, text.Length - start);
                string substr = text.Substring(start, length);

                // if the substring ends in the middle of a sentence, adjust the length accordingly
                if (substr.LastIndexOfAny(new char[] { '.', '!', '?' }) != substr.Length - 1)
                {
                    int lastPeriod = substr.LastIndexOf('.');
                    int lastExclamation = substr.LastIndexOf('!');
                    int lastQuestion = substr.LastIndexOf('?');
                    int lastEnd = Math.Max(lastPeriod, Math.Max(lastExclamation, lastQuestion));
                    if (lastEnd != -1)
                    {
                        length = lastEnd + 1;
                        substr = text.Substring(start, length);
                    }
                }

                result.Add(substr);
                start += length;
            }
            return result.ToArray();
        }
    }
}
