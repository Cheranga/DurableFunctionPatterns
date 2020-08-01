using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExternalEventPattern.DataAccess;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace ExternalEventPattern.Functions
{
    public class UpdateSmsRecipientActivityFunction
    {
        [FunctionName(nameof(UpdateSmsRecipientActivityFunction))]
        public async Task UpdateRecipientAsync([ActivityTrigger] IDurableActivityContext context,
            [Table("%SmsRecipientsTable%")]CloudTable table)
        {
            var smsEvent = context.GetInput<SmsEvent>();

            switch (smsEvent.Status)
            {
                case RecipientStatus.Sent:
                    await HandleSentAsync(table, smsEvent);
                    break;

                case RecipientStatus.CouldNotSend:
                    await HandleCouldNotSendAsync(table, smsEvent);
                    break;
            }
        }

        private async Task HandleSentAsync(CloudTable table, SmsEvent smsEvent)
        {
            var record = await GetAsync(table, RecipientStatus.ToBeSent.ToString().ToUpper(), smsEvent.Id);
            if (record != null)
            {
                var newRecord = new SmsRecipientDataModel
                {
                    PartitionKey = RecipientStatus.Sent.ToString().ToUpper(),
                    RowKey = smsEvent.Id,
                    Message = record.Message,
                    To = record.To
                };

                await CreateAsync(table, newRecord);

                await DeleteAsync(table, RecipientStatus.ToBeSent.ToString().ToUpper(), smsEvent.Id);
            }
        }

        private async Task HandleCouldNotSendAsync(CloudTable table, SmsEvent smsEvent)
        {
            var record = await GetAsync(table, RecipientStatus.ToBeSent.ToString().ToUpper(), smsEvent.Id);
            if (record != null)
            {
                var newRecord = new SmsRecipientDataModel
                {
                    PartitionKey = RecipientStatus.CouldNotSend.ToString().ToUpper(),
                    RowKey = smsEvent.Id,
                    Message = record.Message,
                    To = record.To
                };

                await CreateAsync(table, newRecord);

                await DeleteAsync(table, RecipientStatus.ToBeSent.ToString().ToUpper(), smsEvent.Id);
            }
        }

        private async Task<bool> DeleteAsync(CloudTable table, string partitionKey, string rowKey)
        {
            var deleteOperation = TableOperation.Delete(new SmsRecipientDataModel
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                ETag = "*"
            });

            var tableResult = await table.ExecuteAsync(deleteOperation);

            return true;
        }

        private async Task<SmsRecipientDataModel> GetAsync(CloudTable table, string partitionKey, string rowKey)
        {
            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));

            var query = new TableQuery<SmsRecipientDataModel>().Where(filter);

            var result = await table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            var record = result.Results?.FirstOrDefault();

            return record;
        }

        private async Task CreateAsync(CloudTable table, SmsRecipientDataModel model)
        {
            var tableOperation = TableOperation.InsertOrReplace(model);

            await table.ExecuteAsync(tableOperation);
        }
    }
}