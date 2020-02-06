using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace ToDoApp.RabbitMQCommunicator
{
    public class RabbitCommunicator 
    {
        public static string queueName, message;

        // creates a connection, channel, and returns it as a result
        public static IModel StartRabbitCommunicator()
        {
            return CreateChannel(InitConnection());
        }

        //method for queue creation
        public static QueueDeclareOk CreateQueue(IModel channel, string queueName)
        {
            QueueDeclareOk queue = channel.QueueDeclare(queueName, true, false, false, null);

            return queue;
        }

        //method for publishing a message to a queue
        public static void PublishMessage(string queue, string message, IModel channel)
        {
            byte[] MessageBody = Encoding.UTF8.GetBytes(message);
            RabbitCommunicator.CreateQueue(channel, queue);
            channel.BasicPublish(string.Empty, queue, false, null, MessageBody);
        }

        //connection is not closed, only the channel
        public static void Disconnect(IModel channel)
        {
            channel.Close();
        }

        //method for creating a rabbitmq connection
        private static IConnection InitConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "localhost";

            IConnection conn = factory.CreateConnection();

            return conn;
        }

        private static IModel CreateChannel(IConnection conn)
        {
            IModel channel = conn.CreateModel();   

            return channel;
        }

   
     

     
    }
}
