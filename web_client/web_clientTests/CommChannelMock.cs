using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;

namespace web_client.Tests
{
    class CommChannelMock : CommChannelnterface
    {
        public RandomMover randomMover { get; }

        public CommChannelMock()
        {
            randomMover = new RandomMover();
            Point point = new Point();
            point.x = 12;
            point.y = 34;

            randomMover.id = 123;
        }
        //we modifiy the returned RandomMover for testing purposes
        public string SendData(string data)
        {
            CommunicatorJSON cjson = new CommunicatorJSON();            

            string repr = cjson.GetRepresentation(randomMover);

            return repr;

        }
        
    }
    class CommChannelMockNullResponse: CommChannelnterface
    {
        public string SendData(string data)
        {
            return null;
        }
    }
    class CommChannelMockInvalidResponse : CommChannelnterface
    {
        public string SendData(string data)
        {
            return "To be or not to be!";
        }
    }
}
