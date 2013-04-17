RabbitMqChat
============

Warning! This code provided for messaging Demo purposes and shouldn't be used in Production like environments.

Requirements:
* Up and running RabbitMQ server (http://www.rabbitmq.com/)
* Visual Studio to compile code

This is demo chat application using RabbitMQ as server and C# + EasyNetQ (http://easynetq.com/) as client.

Pull it, compile and run. When you will be asked for server IP, enter your RabbitMQ server IP. If it's hosted on non standard port (not 5672) then also provide port.
When you will asked for nick name, enter your nickname. You can run a few applications and enter different nick names, then you will be able to publish message
from one console and you will get it in all other consoles.

## How it works?

It's realy easy to understand how it works if you EasyNetQ or at least RabbitMQ.

At first I create object named `bus` to communicate with RabbitMQ

`
using (_bus = RabbitHutch.CreateBus("host=" + ip...
{
`

Also I subscribe to chat messages calling method `SubscribeToMessages` which inside looks like

`
	_bus.Subscribe<Joined>(user, msg => Console.WriteLine("User {0} joined at {1}", msg.User, msg.JoinedOn));
	_bus.Subscribe<Leaved>(user, msg => Console.WriteLine("User {0} left at {1}", msg.User, msg.LeftOn));
	_bus.Subscribe<Message>(user, msg => Console.WriteLine("[{2}] {0}> {1}", msg.User, msg.Text, msg.PostedOn.ToString("yyyy-MM-dd HH:mm:ss")));
`

Here I just say that I'm interested of getting notifications when user joins or leaves the chat and when users sends some text to chat.

When I'm subscribed to messages, let's publish one using method `SendMessage` which inside is just simply

`
	using (var publishChannel = bus.OpenPublishChannel())
	{
		publishChannel.Publish(new Message {PostedOn = DateTime.Now, User = user, Text = msg});
	}
`

So it just creates object called `Message` and puts into it some useful information. If you wander what is `Message` class take a look into a project
added to this solution called `RabbitMqChat.Contracts`. It contains three classes called `Joined`, `Leaved`, `Message` which accordingly will be sent from
publisher and received by subscribers when user will join or leave a chat or will write text message. Just simply like that we have chat!

## How it works inside of RabbitMQ?

Thanks to EasyNetQ it's easy to implement such chat console. So, how it looks inside of RabbitMQ. EasynetQ creates for every class an Exchange (http://www.rabbitmq.com/tutorials/tutorial-three-python.html)
which is named by class name and is responsible to send a message to each subscribers queue. So, in our case EasyNetQ will create 3 Exchanges:

* RabbitMqChat_Contracts_Joined:RabbitMqChat_Contracts
* RabbitMqChat_Contracts_Leaved:RabbitMqChat_Contracts
* RabbitMqChat_Contracts_Message:RabbitMqChat_Contracts

It's easy to see a pattern in naming, that's `<namespace>_<class>:<assemply name>`.

From other side there are subscriber queues which are named by pattern `<namespace>_<class>:<assembly name>_<subscription id>`. Here subscription id is unique application
identificator. If it will not be used so then all application would subscribe to single queue and just one application would get message. In this case as subscription id
I have used nick name and so queues are separated. And probably you understood already that if you will open two consoles with same nickname you will get single message
only in one console. So, if will start chat with nick name John, then three queues will be created (one queue per subscription id, per class):

* RabbitMqChat_Contracts_Joined:RabbitMqChat_Contracts_John
* RabbitMqChat_Contracts_Leaved:RabbitMqChat_Contracts_John
* RabbitMqChat_Contracts_Message:RabbitMqChat_Contracts_John

Below is diagram visualising what happens when John publishes message:

![pub/sub](https://raw.github.com/gediminasgu/RabbitMqChat/master/pubsub.png)

Note: as John is also subscribed to the same Exchange, he will also will receive his own messages. It could be easily solved just filtering messages if its author is the same user.

## Message persistence

As you probably know, RabbitMQ store messages in queue once queue is created even no one listening to it. So try to open two chat consoles with different nicknames,
enter some message, then exit from one console, from another enter more message and connect to chat with first console again. You will get all the messages which
was written by another user while this user was offline.

That's all. Happy messaging!