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

            //TODO create an interface to give the direction
            //Implement the interface in a class that moves 
            //in the desired direction. Test if 99 + 1 overflows to 0 
            //and 0 - 1 goes to 99
            rm.MoveOnePos();
        }
    }
}