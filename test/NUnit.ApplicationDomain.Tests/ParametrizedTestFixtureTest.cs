using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
    [TestFixture(1, "ABC")]
    [TestFixture(4, "DEF")]
    [RunInApplicationDomain]
    public class ParametrizedTestFixtureTest
    {
        private readonly int _val;
        private readonly string _str;

        public ParametrizedTestFixtureTest(int val, string str)
        {
            _val = val;
            _str = str;
        }

        [Test]
        public void ParametrizedTestFixtureTest_SimpleTest()
        {
            if (_val == 1)
            {
                Assert.That(_str, Is.EqualTo("ABC"));
            }
            else if (_val == 4)
            {
                Assert.That(_str, Is.EqualTo("DEF"));
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestCase(9)]
        [TestCase(10)]
        public void ParametrizedTestFixtureTest_VaseTest(int caseValue)
        {
            if (_val == 1)
            {
                Assert.That(_str, Is.EqualTo("ABC"));
            }
            else if (_val == 4)
            {
                Assert.That(_str, Is.EqualTo("DEF"));
            }
            else
            {
                Assert.Fail();
            }

            Assert.That(caseValue, Is.GreaterThanOrEqualTo(9).And.LessThanOrEqualTo(10));
        }
    }
}
