namespace ExternalEventPattern.Models
{
    public class SendSmsRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }
}