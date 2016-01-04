using System;
using System.Runtime.Serialization.Json;

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

		public RandomMover SendRepresentation (string representation)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

