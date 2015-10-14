using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhitStream.Data;

namespace WhitStreamUnitTest
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void testListPatternMatch()
        {
            Punctuation.ListPattern lp = new Punctuation.ListPattern(2, 4, 6, 8, 10, 12, 14);
            Assert.IsTrue(lp.Match(2));
            Assert.IsTrue(lp.Match(4));
            Assert.IsTrue(lp.Match(6));
            Assert.IsTrue(lp.Match(8));
            Assert.IsTrue(lp.Match(10));
            Assert.IsTrue(lp.Match(12));
            Assert.IsTrue(lp.Match(14));
            Assert.IsFalse(lp.Match(1));
            Assert.IsFalse(lp.Match(3));
            Assert.IsFalse(lp.Match(5));
            Assert.IsFalse(lp.Match(7));
            Assert.IsFalse(lp.Match(9));
            Assert.IsFalse(lp.Match(11));
            Assert.IsFalse(lp.Match(13));
            Assert.IsFalse(lp.Match(15));
        }
    }
}
