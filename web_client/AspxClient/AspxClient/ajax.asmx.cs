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
using System.Data;
using MySql.Data.MySqlClient;

namespace AspxClient
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        static Thread t = null;
        static ConcurrentDictionary<int,object> dictGrid = new ConcurrentDictionary<int, object>();
        static string connString = "server=127.0.0.1;uid=user_timeline;pwd=UserPass123!;database=utimeline;";        
        static string sqlLastInInterval = "select "+
                                "t.timepoint, t.id_agent, t.x, t.y from timeline t "+
                                "inner join("+
                                "select "+
                                "max(timepoint) as tpoint, "+
                                "id_agent as agent "+
                                "from "+
                                "timeline where "+
                                "timepoint "+ 
                                "between @start_date and @end_date " + 
	                            "group by id_agent ) t2 " +
                                "on t.id_agent = t2.agent and t.timepoint = t2.tpoint";

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

        public WebService1():base()
        {
            //I should put here the Connect to Rabbit function, but then I woud lose 
            //the oportunity to use the double-checked locking in GetReadings and show off :)
        }

        //this function starts the first time the GetReadings function called
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
            using(MySqlConnection conn = new MySqlConnection(WebService1.connString))
            {
                try {
                    conn.Open();

                }
                catch (Exception)
                {
                    return;
                }
                using (MySqlCommand mysqlComm = new MySqlCommand("insert into timeline(id_agent,x,y) values(@id_agent,@x,@y)", conn))
                {
                    mysqlComm.Parameters.AddWithValue("@id_agent", rm.id);
                    mysqlComm.Parameters.AddWithValue("@x", rm.point.x);
                    mysqlComm.Parameters.AddWithValue("@y", rm.point.y);
                    mysqlComm.ExecuteNonQuery();
                    //only show what was written
                    dictGrid[rm.id] = rm;
                }
            }            

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
        public void GetHistoryReadings(string data1,string data2)
        {
            HttpContext.Current.Response.ContentType = "application/json";

            string rez = "";

            using (MySqlConnection conn = new MySqlConnection(WebService1.connString))
            {
                try
                {
                    conn.Open();

                }
                catch (Exception)
                {
                    return;
                }
                System.Text.StringBuilder sb = new System.Text.StringBuilder("[");
                // ajax will ask for data in an 499 interval but that does not mean that the 
                //messages will arrive in the exact timeline and get written
                using (MySqlCommand mysqlComm = new MySqlCommand(sqlLastInInterval, conn))
                {                    
                    mysqlComm.Parameters.AddWithValue("@start_date", data1);
                    mysqlComm.Parameters.AddWithValue("@end_date", data2);
                    using (MySqlDataReader dr = mysqlComm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            //now we could create a RemoteMover and do the same as before
                            //but lets do it otherwise. We will use a StringBuilder 
                            //since it is faster than string concatenation. This is not a good solution for production as it is error prone,
                            //but I will play if allowed
                            sb.Append("{\"id\":");
                            sb.Append(dr.GetInt64("id_agent").ToString());
                            sb.Append(", \"point\":{\"x\":");
                            sb.Append(dr.GetInt32("x").ToString());
                            sb.Append(",\"y\":");
                            sb.Append(dr.GetInt32("y").ToString());
                            sb.Append("}}");
                            sb.Append(","); 
                        }
                        if (sb.Length > 5)
                        {
                            sb.Remove(sb.Length - 1, 1);
                        }
                        sb.Append("]");
                        rez = sb.ToString();
                    }
                }
            }


            HttpContext.Current.Response.Write(rez);
        }

        [WebMethod]
        public void GetReadings()
        {

            //we initialize the thread that will read from RabbitMQ
            if (t == null)
            {
                lock (oLock)
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
            }

            //put the available data to json - java script will love it
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
