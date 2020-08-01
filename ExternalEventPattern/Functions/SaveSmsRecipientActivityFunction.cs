using System;
using System.Threading.Tasks;
using ExternalEventPattern.DataAccess;
using ExternalEventPattern.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace ExternalEventPattern.Functions
{
    public class SaveSmsRecipientActivityFunction
    {
        private readonly ILogger<SaveSmsRecipientActivityFunction> _logger;

        public SaveSmsRecipientActivityFunction(ILogger<SaveSmsRecipientActivityFunction> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(SaveSmsRecipientActivityFunction))]
        public async Task<bool> SaveRecipientAsync([ActivityTrigger] IDurableActivityContext context,
            [Table("%SmsRecipientsTable%")] CloudTable smsRecipients)
        {
            var recipient = context.GetInput<SmsRecipient>();
            if (recipient == null)
            {
                return false;
            }

            var status = await SaveAsync(smsRecipients, recipient);

            return status;
        }

        private async Task<bool> SaveAsync(CloudTable table, SmsRecipient recipient)
        {
            try
            {
                var dataModel = new SmsRecipientDataModel
                {
                    PartitionKey = RecipientStatus.ToBeSent.ToString().ToUpper(),
                    RowKey = recipient.Id,
                    Message = recipient.Message
                };

                var tableOperation = TableOperation.InsertOrReplace(dataModel);
                await table.ExecuteAsync(tableOperation);

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error when saving recipient {recipientId}", recipient.Id);
            }

            return false;
        }
    }
}