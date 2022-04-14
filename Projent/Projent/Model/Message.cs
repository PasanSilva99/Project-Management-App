using System;
using System.Collections.Generic;

namespace Projent.Model
{
    public class Message
    {
        public string MessageContent { get; set; }
        public bool isSticker { get; set; }
        public string sender { get; set; }
        public string receiver { get; set; }
        public DateTime Time { get; set; }
        public List<String> MentionedUsers { get; set; }
    }
}
