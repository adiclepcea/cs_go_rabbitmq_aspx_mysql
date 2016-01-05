using System;

namespace web_client
{
	public interface CommunicatorInterface
	{
		string GetRepresentation(RandomMover rm);
		string SendRepresentation(ref RandomMover rm,string server);
	}
}

