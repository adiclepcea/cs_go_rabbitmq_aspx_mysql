using System;
using System.Runtime.Serialization.Json;

namespace web_client
{
	public class CommunicatorJSON : CommunicatorInterface
	{
		public CommunicatorJSON ()
		{

		}

		#region CommunicatorInterface implementation

		public string GetRepresentation (RandomMover rm)
		{
			throw new NotImplementedException ();
		}

		public RandomMover SendRepresentation (string representation)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

