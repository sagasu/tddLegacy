using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlogEngine.Core;

namespace BlogEngine.Tests.Ping
{
    public class StubPublishable : IPublishable
    {
        public Uri AbsoluteLink { get; private set; }
        public string Author { get; private set; }
        public StateList<Category> Categories { get; private set; }
        public string Content { get; private set; }
        public DateTime DateCreated { get; private set; }
        public DateTime DateModified { get; private set; }
        public string Description { get; private set; }
        public Guid Id { get; private set; }
        public Guid BlogId { get; private set; }
        public Blog Blog { get; private set; }
        public bool IsPublished { get; private set; }
        public string RelativeLink { get; private set; }
        public string RelativeOrAbsoluteLink { get; private set; }
        public string Title { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsVisibleToPublic { get; private set; }
        public void OnServing(ServingEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
