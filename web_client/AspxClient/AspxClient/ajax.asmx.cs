using System;
using System.Collections.Generic;
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
            public Point pos { get; set; }

            [DataMember]
            public int id { get; set; }

        }

        public void ConnectToRabbit()
        {
            ConnectionFactory cf = new ConnectionFactory();
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
                        Console.WriteLine(" [x] Received {0}", message);
                    };
                    channel.BasicConsume(queue: queueName, noAck: true, consumer: consumer);
                    //not nice, but ... it does wait
                    while (true) {
                        Thread.Sleep(50);
                    }
                }
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
            RemoteMover mucu = new RemoteMover();
            mucu.id = 10;
            Point p = new Point();
            p.x = 2;

            mucu.pos = new Point(2, 10);
            string rez = "";
            HttpContext.Current.Response.ContentType = "application/json";

            using (System.IO.MemoryStream sw = new System.IO.MemoryStream())
            {

                dcjs.WriteObject(sw, mucu);

                rez = System.Text.UTF8Encoding.UTF8.GetString(sw.ToArray());
            }
            HttpContext.Current.Response.Write(rez);
            
        }
    }
}
