using System;


namespace web_client
{
	public interface CommunicatorInterface
	{
        void SetCommChannel(CommChannelnterface cci);
		string GetRepresentation(RandomMover rm);
		string SendRepresentation(ref RandomMover rm);
	}
}

