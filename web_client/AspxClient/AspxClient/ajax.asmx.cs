using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace AspxClient
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        static Thread t = null;
        static ConcurrentDictionary<int,object> dictGrid = new ConcurrentDictionary<int, object>();
        object oLock = new object();
        public struct Point
        {
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x { get; set; }
            public int y {get; set; }
        }
        [DataContract]
        public class RemoteMover
        {
           
            [DataMember]
            public Point point { get; set; }

            [DataMember]
            public int id { get; set; }

        }

        public void ConnectToRabbit()
        {
            ConnectionFactory cf = new ConnectionFactory();
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(RemoteMover));
            cf.HostName = "localhost";
            cf.UserName = "guest";
            cf.Password = "guest";
            //connect to RabbitMQ
            using (IConnection conn = cf.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "miscari", type: "fanout", durable: true);
                    var queueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(queue: queueName ,
                              exchange: "miscari",
                              routingKey: "");
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = System.Text.UTF8Encoding.UTF8.GetString(body);

                        RemoteMover rm = JsonToRemoteMover(message);

                        if (rm != null)
                        {
                            WriteToDb(rm);
                            dictGrid[rm.id] = rm;
                        }
                        
                    };
                    channel.BasicConsume(queue: queueName, noAck: true, consumer: consumer);
                    //not nice, but ... it does wait
                    while (true) {
                        Thread.Sleep(50);
                    }
                }
            }
        }

        private void WriteToDb(RemoteMover rm)
        {

        }

        public RemoteMover JsonToRemoteMover(string json)
        {
            try {
                DataContractJsonSerializer jdcs = new DataContractJsonSerializer(typeof(RemoteMover));
                System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(json));
                RemoteMover rm = (RemoteMover)jdcs.ReadObject(ms);
                return rm;
            }
            catch (InvalidCastException )
            {
                return null;
            }
            catch (SerializationException)
            {
                return null;
            }
        }

        
        [WebMethod]
        public void GetName()
        {

            lock(oLock)
            {
                if (t == null)
                {
                    t = new Thread(() =>
                    {
                        ConnectToRabbit();
                    });
                    t.IsBackground = true;
                    t.Start();
                }
            }
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(RemoteMover));

            
            string rez = "";
            HttpContext.Current.Response.ContentType = "application/json";

            using (System.IO.MemoryStream sw = new System.IO.MemoryStream())
            {

                if (dictGrid.Count > 0)
                {
                    RemoteMover rmLast = (RemoteMover)dictGrid.Last().Value;
                    sw.Write(System.Text.UTF8Encoding.UTF8.GetBytes("[\r\n"), 0, 3);
                    foreach (RemoteMover rm in dictGrid.Values)
                    {

                        dcjs.WriteObject(sw, rm);
                        if (rmLast != rm)
                        {
                            sw.Write(System.Text.UTF8Encoding.UTF8.GetBytes(",\r\n"), 0, 3);
                        }
                    }
                    sw.Write(System.Text.UTF8Encoding.UTF8.GetBytes("]\r\n"), 0, 3);

                    rez = System.Text.UTF8Encoding.UTF8.GetString(sw.ToArray());
                }
            }
            HttpContext.Current.Response.Write(rez);
            
        }
    }
}
