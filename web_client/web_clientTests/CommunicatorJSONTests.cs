using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization.Json;
using web_client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace web_client.Tests
{
    [TestClass()]
    public class CommunicatorJSONTests
    {
        CommunicatorJSON cjson;
        [TestInitialize()]
        public void InitTest()
        {
            cjson = new CommunicatorJSON();
        }

        [TestMethod()]
        public void GetRepresentationTest()
        {

            DataContractJsonSerializer dcs = new DataContractJsonSerializer(typeof(RandomMover));

            RandomMover rm = new RandomMover();
            rm.id = 1;
            Point p = new Point();
            p.x = 12;
            p.y = 33;
            rm.pos = p;

            string repr = cjson.GetRepresentation(rm);            

            //reconvert the stream to a random mover representation to see if all went well
            System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(repr));

            RandomMover rmout = (RandomMover)dcs.ReadObject(ms);

            Assert.AreEqual(rm.id, rmout.id);
            Assert.AreEqual(rm.pos.x, rmout.pos.x);
            Assert.AreEqual(rm.pos.y, rmout.pos.y);

            //we could also do this, but the method above also tests that we have a valid json
            //string rez = "{\"id\":1,\"point\":{\"x\":12,\"y\":33}}";
            //repr.Replace(" ", "");
            //repr.Replace("\r\n", "");

            //Assert.AreEqual(repr, rez);

        }


        /// <summary>
        /// If the server says that the RandomMover should be modified, 
        /// then so it should
        /// </summary>
        [TestMethod()]
        public void SendRepresentationTest()
        {
            DataContractJsonSerializer dcs = new DataContractJsonSerializer(typeof(RandomMover));
            CommChannelMock ccmock = new CommChannelMock();
            cjson.SetCommChannel(ccmock);
            //the random mover we are sending in
            RandomMover rmIn = new RandomMover();
            Point p = new Point();
            p.x = 2;
            p.y = 3;
            rmIn.id = 1;
            rmIn.pos = p;

            //the reference Random Mover
            RandomMover rmRef = ccmock.randomMover;
            

            string rez = cjson.SendRepresentation(ref rmIn);

            Assert.AreEqual(rmIn.id, rmRef.id);
            Assert.AreEqual(rmIn.pos.x, rmRef.pos.x);
            Assert.AreEqual(rmIn.pos.y, rmRef.pos.y);

        }

        [TestMethod]
        [ExpectedException(typeof(System.Runtime.Serialization.SerializationException))]
        public void SendRepresentationTest_ShouldCrashSerializationException()
        {
            cjson.SetCommChannel(new CommChannelMockInvalidResponse());

            //the random mover we are sending in
            RandomMover rmIn = new RandomMover();
            Point p = new Point();
            p.x = 2;
            p.y = 3;
            rmIn.id = 1;
            rmIn.pos = p;
            
            string rez = cjson.SendRepresentation(ref rmIn);
        }

        [TestMethod]
        public void SendRepresentationTest_ShouldCrashException()
        {            
            cjson.SetCommChannel(new CommChannelMockNullResponse());

            //the random mover we are sending in
            RandomMover rmIn = new RandomMover();
            Point p = new Point();
            p.x = 2;
            p.y = 3;
            rmIn.id = 1;
            rmIn.pos = p;
            try {
                string rez = cjson.SendRepresentation(ref rmIn);
                Assert.Fail();
            }
            catch (Exception) { }    

        }

    }
}