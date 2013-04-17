using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqChat.Contracts
{
    public class Joined
    {
        public DateTime JoinedOn { get; set; }
        public string User { get; set; }
    }
}
