using System;
using System.Collections.Generic;
using System.Text;

namespace ExternalEventPattern.Models
{
    public class SmsRecipient
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }
}
