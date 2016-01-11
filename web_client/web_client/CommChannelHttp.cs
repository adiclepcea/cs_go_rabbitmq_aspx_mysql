using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace web_client
{
    class CommChannelHttp : CommChannelnterface
    {
        public string Server { get; set; }
        private HttpWebRequest request;

        public CommChannelHttp()
        {
            
        }
        public string SendData(string data)
        {

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            try
            {
                request = (HttpWebRequest)WebRequest.Create(Server);
                byte[] req = System.Text.UTF8Encoding.UTF8.GetBytes(data);
                //we setup the request header and then we send it
                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = req.Length;
                request.GetRequestStream().Write(req, 0, req.Length);

                //now see what goodies grandma sent us
                using (var response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode != HttpStatusCode.OK)
                    { //it means something bad happened, so we stop
                        return null;
                    }

                    System.IO.Stream stream = response.GetResponseStream();

                    stream.CopyTo(ms);

                    return System.Text.UTF8Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (WebException)
            {
                Console.Out.WriteLine("The bloody server gives me the silence treatment!");
                return null;
            }
        }
    }
}
