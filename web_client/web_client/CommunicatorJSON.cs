using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;

namespace web_client
{
	public class CommunicatorJSON : CommunicatorInterface
	{
		DataContractJsonSerializer dcjs;
        CommChannelnterface cci;

        //so that we can send this by http or whatever we decide later
        //and we can also test with a mock channel
        public void SetCommChannel(CommChannelnterface cci)
        {
            this.cci = cci;
        }

		public CommunicatorJSON ()
		{
			dcjs = new DataContractJsonSerializer (typeof(RandomMover));
            SetCommChannel(new CommChannelHttp());            
		}

        public void SetServer(string server)
        {
            ((CommChannelHttp)this.cci).Server = server;
        }

		#region CommunicatorInterface implementation

		public string GetRepresentation (RandomMover rm)
		{
			string rez = "";
			using (System.IO.MemoryStream sw = new System.IO.MemoryStream()) {

				dcjs.WriteObject (sw, rm);

				rez = System.Text.UTF8Encoding.UTF8.GetString (sw.ToArray ());
			}
			return rez;
		}

		public string SendRepresentation (ref RandomMover rm)
		{
            
            string representation = GetRepresentation (rm);
            RandomMover rmTemp;
            
            String response;
            if ((response = cci.SendData(representation))==null) {
                throw new Exception("No response from server!");                
            }
            Console.Out.WriteLine(response);
            using (MemoryStream ms = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(response)))
            {
                try
                {
                    object obj = dcjs.ReadObject(ms);
                    rmTemp = (RandomMover)obj;
                }
                catch (System.Runtime.Serialization.SerializationException serEx) //not a valid json representation of RandomMover
                {
                    throw serEx;
                }
            }
            rm.id = rmTemp.id;
            rm.pos = rmTemp.pos;

            return rm.ToString();
		}

		#endregion
	}
}

