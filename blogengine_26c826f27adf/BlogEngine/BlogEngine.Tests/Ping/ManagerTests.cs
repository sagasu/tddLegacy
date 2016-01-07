using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlogEngine.Core;
using BlogEngine.Core.Ping;
using Moq;
using NUnit.Framework;

namespace BlogEngine.Tests.Ping
{
    [TestFixture]
    class ManagerTests
    {
        [Test]
        public void Send_PingBackOnAndTrackBackOff_SendPingback()
        {
            var fm = new TestableManager(new List<string>() { "http://example.com/1" }, "any blog name")
                {
                    WillPingbackOn = true, WillTrackbackOn = false
                };

            fm.InstanceSend(new StubPublishable());

            Assert.IsTrue(fm.IsPingbackSend);
        }
        
        [Test]
        public void Send_WhenUrlsInContentAndTrackbackOn_SendsMessage()
        {
            var fm = new TestableManager(new List<string>() {"http://example.com/1"}, "any blog name")
                {
                    WillPingbackOn = false,
                    WillTrackbackOn = true,
                };
            
            fm.InstanceSend(new StubPublishable());

            StringAssert.Contains(@"title=&url=http://example.com/1&excerpt=&blog_name=any blog name", fm.Message.First());
        }

        [Test]
        public void Send_WhenMultipleUrlsAndTrackbackIsCalled_SendsMessageFromGivenUrl()
        {
            var fm = new TestableManager(new List<string>() { "http://example1.com/", "http://example2.com/" }, "any blog name")
                {
                    WillPingbackOn = false, WillTrackbackOn = true
                };

            fm.InstanceSend(new StubPublishable());

            StringAssert.Contains(@"title=&url=http://example1.com/&excerpt=&blog_name=any blog name", fm.Message.First());
            StringAssert.Contains(@"title=&url=http://example2.com/&excerpt=&blog_name=any blog name", fm.Message.Skip(1).First());
        }
    }
}
