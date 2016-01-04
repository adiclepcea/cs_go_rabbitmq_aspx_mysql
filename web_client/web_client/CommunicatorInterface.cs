using System;

namespace web_client
{
	public interface CommunicatorInterface
	{
		string GetRepresentation(ref RandomMover rm);
		bool SendRepresentation(string representation);
	}
}

