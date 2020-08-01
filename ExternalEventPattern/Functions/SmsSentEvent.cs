using ExternalEventPattern.DataAccess;

namespace ExternalEventPattern.Functions
{
    public class SmsEvent
    {
        public string Id { get; set; }
        public RecipientStatus Status { get; set; }
    }
}