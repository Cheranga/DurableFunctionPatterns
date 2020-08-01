
using Microsoft.Azure.Cosmos.Table;

namespace ExternalEventPattern.DataAccess
{
    public class SmsRecipientDataModel : TableEntity
    {
        public string To { get; set; }
        public string Message { get; set; }
    }

    public enum RecipientStatus
    {
        ToBeSent,
        Sent,
        CouldNotSend,
        Delivered,
        Undelivered
    }
}