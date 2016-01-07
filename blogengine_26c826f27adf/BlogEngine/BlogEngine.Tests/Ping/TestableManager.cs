using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlogEngine.Core;
using BlogEngine.Core.Ping;

namespace BlogEngine.Tests.Ping
{
    public class TestableManager : Manager
    {
        private readonly string _blogName;
        public bool IsPingbackSend { get; set; }

        public List<string> Message = new List<string>();
        public List<string> UrlsInContent { get; set; }
        public bool WillPingbackOn { get; set; }
        public bool WillTrackbackOn { get; set; }

        public TestableManager(List<string> urls, string blogName)
        {
            _blogName = blogName;
            UrlsInContent = urls;
        }

        protected override void SendPingback(Uri itemUrl, Uri url)
        {
            IsPingbackSend = true;
        }

        protected override bool GetTrackbackSent(TrackbackMessage message)
        {
            Message.Add(message.ToString());
            return true;
        }

        protected override TrackbackMessage GetTrackbackMessage(IPublishable item, Uri itemUrl, Uri trackbackUrl)
        {
            var trackbackMessage = new TestableTrackbackMessage(item, itemUrl, trackbackUrl, _blogName);
            return trackbackMessage;
        }

        protected override Uri GetTrackbackUrl(Uri url)
        {
            return url;
        }

        protected override bool IsPingbackEnabled()
        {
            return WillPingbackOn;
        }

        protected override bool IsTrackbackSendEnabled()
        {
            return WillTrackbackOn;
        }

        protected override IEnumerable<Uri> GetUrlsFromContent(string content)
        {
            return UrlsInContent.Select(_ => new Uri(_));
        }
    }

    public class TestableTrackbackMessage : TrackbackMessage
    {
        public TestableTrackbackMessage(IPublishable item, Uri urlToNotifyTrackback, Uri itemUrl, string blogName)
            : base(item, urlToNotifyTrackback, itemUrl)
        {
            BlogName = blogName;
        }

        public TestableTrackbackMessage(IPublishable item, Uri urlToNotifyTrackback, Uri itemUrl) : base(item, urlToNotifyTrackback, itemUrl){}

        protected override string GetBlogName()
        {
            return "";
        }
    }
}
