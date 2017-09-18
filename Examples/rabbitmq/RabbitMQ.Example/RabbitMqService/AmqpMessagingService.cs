﻿using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public class AmqpMessagingService
    {

        //private string _hostName = "localhost";
        private string _hostName = "172.17.0.2";
        private string _userName = "guest";
        private string _password = "guest";
        private string _exchangeName = "";
        private bool _durable = true;

        private string _oneWayMessageQueueName = "OneWayMessageQueue";
        private string _workerQueueDemoQueueName = "WorkerQueueDemoQueue";
        private string _publishSubscribeExchangeName = "PublishSubscribeExchange";
        private string _publishSubscribeQueueOne = "PublishSubscribeQueueOne";
        private string _publishSubscribeQueueTwo = "PublishSubscribeQueueTwo";

        private string _serialisationQueueName = "SerialisationDemoQueue";

        public AmqpMessagingService()
        {

        }

        public IConnection GetRabbitMqConnection()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = _hostName;
            connectionFactory.UserName = _userName;
            connectionFactory.Password = _password;
            
            return connectionFactory.CreateConnection();
        }

        #region "OneWayMessageQueue"
        public void SetUpQueueForOneWayMessageDemo(IModel model)
        {
            //QueueDeclaration is idempotent operation 
            //"denoting an element of a set that is unchanged in value when multiplied or otherwise operated on by itself."
            model.QueueDeclare(_oneWayMessageQueueName, _durable, false, false, null);
        }
      
        public void SendOneWayMessage(string message, IModel model)
        {
            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.Persistent = _durable;
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            model.BasicPublish(_exchangeName, _oneWayMessageQueueName, basicProperties, messageBytes);
        }

        public void ReceiveOneWayMessages(IModel model)
        {
            model.BasicQos(0, 1, false); //basic quality of service
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(model);
            model.BasicConsume(_oneWayMessageQueueName, false, consumer);
            while (true)
            {
                BasicDeliverEventArgs deliveryArguments = consumer.Queue.Dequeue() as BasicDeliverEventArgs;
                String message = Encoding.UTF8.GetString(deliveryArguments.Body);
                Console.WriteLine("Message received: {0}", message);
                model.BasicAck(deliveryArguments.DeliveryTag, false);
            }
        }
        #endregion

        #region "WorkerQueue"

        public void SetUpQueueForWorkerQueueDemo(IModel model)
        {
            model.QueueDeclare(_workerQueueDemoQueueName, _durable, false, false, null);
        }

        public void SendMessageToWorkerQueue(string message, IModel model)
        {
            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.Persistent = _durable;
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            model.BasicPublish(_exchangeName, _workerQueueDemoQueueName, basicProperties, messageBytes);
        }

        public void ReceiveWorkerQueueMessages(IModel model)
        {
            //Pulls message off queue, message no longer available

            model.BasicQos(0, 1, false); //basic quality of service
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(model);
            model.BasicConsume(_workerQueueDemoQueueName, false, consumer);
            while (true)
            {
                BasicDeliverEventArgs deliveryArguments = consumer.Queue.Dequeue() as BasicDeliverEventArgs;
                String message = Encoding.UTF8.GetString(deliveryArguments.Body);
                Console.WriteLine("Message received: {0}", message);
                model.BasicAck(deliveryArguments.DeliveryTag, false);
            }
        }
        #endregion

        #region "Publish/Subscriber"

        public void SetUpExchangeAndQueuesForDemo(IModel model)
        {
            model.ExchangeDeclare(_publishSubscribeExchangeName, ExchangeType.Fanout, true);
            model.QueueDeclare(_publishSubscribeQueueOne, true, false, false, null);
            model.QueueDeclare(_publishSubscribeQueueTwo, true, false, false, null);
            model.QueueBind(_publishSubscribeQueueOne, _publishSubscribeExchangeName, "");
            model.QueueBind(_publishSubscribeQueueTwo, _publishSubscribeExchangeName, "");
        }

        public void SendMessageToPublishSubscribeQueues(string message, IModel model)
        {
            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.Persistent = _durable;
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            model.BasicPublish(_publishSubscribeExchangeName, "", basicProperties, messageBytes);
        }

        public void ReceivePublishSubscribeMessageReceiverOne(IModel model)
        {
            model.BasicQos(0, 1, false);
            Subscription subscription = new Subscription(model, _publishSubscribeQueueOne, false);
            while (true)
            {
                BasicDeliverEventArgs deliveryArguments = subscription.Next();
                String message = Encoding.UTF8.GetString(deliveryArguments.Body);
                Console.WriteLine("Message from queue: {0}", message);
                subscription.Ack(deliveryArguments);
            }
        }

        public void ReceivePublishSubscribeMessageReceiverTwo(IModel model)
        {
            model.BasicQos(0, 1, false);
            Subscription subscription = new Subscription(model, _publishSubscribeQueueTwo, false);
            while (true)
            {
                BasicDeliverEventArgs deliveryArguments = subscription.Next();
                String message = Encoding.UTF8.GetString(deliveryArguments.Body);
                Console.WriteLine("Message from queue: {0}", message);
                subscription.Ack(deliveryArguments);
            }
        }
        #endregion

        #region "Serialize Object"

        public void SetUpQueueForSerialization(IModel model)
        {
            model.QueueDeclare(_serialisationQueueName, _durable, false, false, null);
        }

        public void SendObjectToQueue(string message, IModel model)
        {
            Customer customer = new Customer() { CustomerID = DateTime.Now.Ticks, FullName = message };

            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.SetPersistent(true);
            String jsonified = JsonConvert.SerializeObject(customer);
            byte[] customerBuffer = Encoding.UTF8.GetBytes(jsonified);
            model.BasicPublish("", _serialisationQueueName, basicProperties, customerBuffer);

        }

        public void ReceiveObjectFromQueue(IModel model)
        {
            model.BasicQos(0, 1, false);
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(model);
            model.BasicConsume(_serialisationQueueName, false, consumer);
            while (true)
            {
                BasicDeliverEventArgs deliveryArguments = consumer.Queue.Dequeue() as BasicDeliverEventArgs;
                String jsonified = Encoding.UTF8.GetString(deliveryArguments.Body);
                Customer customer = JsonConvert.DeserializeObject<Customer>(jsonified);
                Console.WriteLine("Pure json: {0}", jsonified);
                Console.WriteLine("Customer name: {0}", customer.FullName);
                Console.WriteLine("Customer Id: {0}", customer.CustomerID);
                model.BasicAck(deliveryArguments.DeliveryTag, false);
            }
        }

        #endregion
    }
}
