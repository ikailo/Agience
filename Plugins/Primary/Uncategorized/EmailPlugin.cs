using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Primary.Uncategorized
{
    public class EmailPlugin
    {
        [KernelFunction]
        [Description("Sends an email to a recipient.")]
        public Task SendEmailAsync(
            
            [Description("Semicolon delimitated list of emails of the recipients")] string recipientEmails,
            string subject,
            string body
        )
        {
            // Add logic to send an email using the recipientEmails, subject, and body
            // For now, we'll just print out a success message to the console
            //console.WriteLineAsync($"* Sent Email *\r\nTo: {recipientEmails}\r\nSubject: {subject}\r\nBody: {body}\r\n* * *");

            return Task.CompletedTask;
        }
    }
}
