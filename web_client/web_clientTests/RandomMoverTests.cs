using Microsoft.VisualStudio.TestTools.UnitTesting;
using web_client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace web_client.Tests
{
    [TestClass()]
    public class RandomMoverTests
    {
        [TestMethod()]
        public void MoveOnePosTest()
        {
            RandomMover rm = new RandomMover();
            Point p = new Point();
            rm.id = 0;
            p.x = 99;
            p.y = 99;
            rm.pos = p;
            //0 = down
            //2 = up
            //1 = left
            //3 = right
            rm.MoveOnePos((byte)3);  
            Assert.AreEqual(0,rm.pos.x);
            rm.MoveOnePos((byte)0);
            Assert.AreEqual(0, rm.pos.y);

            rm.MoveOnePos((byte)1);
            Assert.AreEqual(99, rm.pos.x);
            rm.MoveOnePos((byte)2);
            Assert.AreEqual(99, rm.pos.y);
        }
    }
}