using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using RabbitMqChat.Contracts;

namespace RabbitMqChat
{
    class Program
    {
        private static IBus _bus;
        static void Main(string[] args)
        {
            var ip = Ask("Please enter server IP[:port]: ");
            var user = Ask("Please enter your nick name: ");

            using (_bus = RabbitHutch.CreateBus("host=" + ip, x => x.Register<IEasyNetQLogger>(_ => new EmptyLogger())))
            {
                SendJoinedMessage(user);
                Console.WriteLine("You will be joined to chat soon. If you will want to leave just enter message 'exit'");
                SubscribeToMessages(user);
                while (true)
                {
                    Console.Write("> ");
                    var msg = Console.ReadLine();

                    if (msg.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SendExitMessage(user);
                        break;
                    }

                    SendMessage(_bus, user, msg);
                }
            }
        }

        private static void SubscribeToMessages(string user)
        {
            _bus.Subscribe<Joined>(user, msg => Console.WriteLine("User {0} joined at {1}", msg.User, msg.JoinedOn));
            _bus.Subscribe<Leaved>(user, msg => Console.WriteLine("User {0} left at {1}", msg.User, msg.LeftOn));
            _bus.Subscribe<Message>(user, msg => Console.WriteLine("[{2}] {0}> {1}", msg.User, msg.Text, msg.PostedOn.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        private static string Ask(string prompt)
        {
            string result = null;
            while (string.IsNullOrEmpty(result))
            {
                Console.Write(prompt);
                result = Console.ReadLine();
            }
            return result;
        }

        private static void SendJoinedMessage(string user)
        {
            using (var publishChannel = _bus.OpenPublishChannel())
            {
                publishChannel.Publish(new Joined { JoinedOn = DateTime.Now, User = user });
            }
        }

        private static void SendExitMessage(string user)
        {
            using (var publishChannel = _bus.OpenPublishChannel())
            {
                publishChannel.Publish(new Leaved() { LeftOn = DateTime.Now, User = user });
            }
        }

        private static void SendMessage(IBus bus, string user, string msg)
        {
            using (var publishChannel = bus.OpenPublishChannel())
            {
                publishChannel.Publish(new Message {PostedOn = DateTime.Now, User = user, Text = msg});
            }
        }
    }

    public class EmptyLogger : IEasyNetQLogger
    {
        public void DebugWrite(string format, params object[] args)
        {
        }

        public void InfoWrite(string format, params object[] args)
        {
        }

        public void ErrorWrite(string format, params object[] args)
        {
        }

        public void ErrorWrite(Exception exception)
        {
        }
    }
}
