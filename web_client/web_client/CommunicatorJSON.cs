using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;

namespace web_client
{
	public class CommunicatorJSON : CommunicatorInterface
	{
		DataContractJsonSerializer dcjs;
		public CommunicatorJSON ()
		{
			dcjs = new DataContractJsonSerializer (typeof(RandomMover));
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

		public string SendRepresentation (ref RandomMover rm,string server)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (server);
			string representation = GetRepresentation (rm);
			RandomMover rmTemp;
			byte[] req = System.Text.UTF8Encoding.UTF8.GetBytes (representation);
			//we setup the request header and then we send it
			request.Method = "POST";
			request.Accept = "application/json";
			request.ContentType = "application/json; charset=utf-8";
			request.ContentLength = req.Length;
			request.GetRequestStream ().Write (req, 0, req.Length);

			//now see what goodies grandma sent us
			using (var response = (HttpWebResponse)request.GetResponse ()) {

				if (response.StatusCode != HttpStatusCode.OK) { //it means something bad happened, so we stop
					return null;
				}

				using (var sr = response.GetResponseStream()) {
					object resp = dcjs.ReadObject (sr); 			//convert the response to object
					try {
						rmTemp = resp as RandomMover;			//this object should be a representation of RandomMover
						rm.id = rmTemp.id;
						rm.pos = rmTemp.pos;
					} catch (InvalidCastException ex) {
						return null;
					}
				}
			}

			return rm.ToString();
		}

		#endregion
	}
}

