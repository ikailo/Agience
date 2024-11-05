using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.Uncategorized
{
    public class Debug
    {
        //public override string[] InputKeys => new string[] { "template_id", "input_string" };
        /*

        [KernelFunction, Description("Debug a template with input data.")]
        public Task<Data?> Process(Runner runner, Data? input = null)
        {
            Console.WriteLine("Default Debug template has been called. //TODO: Implement.");

            return Task.FromResult<Data?>(null);
        }*/
    }
}

/*
       internal override async Task<Data?> Process(Data? data = null)
       {

#if DEBUG

           // Parse the input for the template id and user data

           // Expected Input: debug <templateId> <userData>

           int firstSpace = data?.Raw?.IndexOf(' ') ?? -1;

           if (firstSpace > 6)
           {
               var templateId = data?.Raw?.Substring(6, firstSpace - 6);
               var userData = data?.Raw?.Substring(firstSpace + 1);

               if (string.IsNullOrEmpty(templateId) || !Agent.Agency.Catalog.ContainsKey(templateId) || string.IsNullOrEmpty(userData))
               {
                   return null;
               }

               Model.Template? template = Agent.Agency.Catalog[templateId];

               Data? inputData = null;

               if (template?.InputKeys != null && template.InputKeys.Length > 0)
               {
                   inputData = new Data(userData, DataFormat.STRUCTURED);
               }
               else
               {
                   inputData = new Data(userData, DataFormat.RAW);
               }

               return await Agent.Dispatch(templateId, data);
           }
           else
           {
               // TODO: Allow parameterless debug with no data
               return "Not Supported";
           }

#endif

           return "Debug not enabled";
       }*/