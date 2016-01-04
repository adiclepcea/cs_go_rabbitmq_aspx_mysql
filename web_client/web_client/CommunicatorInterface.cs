using System;

namespace web_client
{
	public interface CommunicatorInterface
	{
		string GetRepresentation(RandomMover rm);
		RandomMover SendRepresentation(string representation);

	}
}

