using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqChat.Contracts
{
    public class Message
    {
        public DateTime PostedOn { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
    }
}
