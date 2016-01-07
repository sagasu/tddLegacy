﻿namespace BlogEngine.Core.Web.Controls
{
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// All pages in the custom themes as well as pre-defined pages in the root
    ///     must inherit from this class.
    /// </summary>
    /// <remarks>
    /// The class is responsible for assigning the theme to all
    ///     derived pages as well as adding RSS, RSD, tracking script
    ///     and a whole lot more.
    /// </remarks>
    public abstract class BlogBasePage : Page
    {
        #region Public Methods

        /// <summary>
        /// Adds the generic link to the header.
        /// </summary>
        /// <param name="relation">
        /// The relation string.
        /// </param>
        /// <param name="title">
        /// The title string.
        /// </param>
        /// <param name="href">
        /// The href string.
        /// </param>
        public virtual void AddGenericLink(string relation, string title, string href)
        {
            AddGenericLink(null, relation, title, href);
        }

        /// <summary>
        /// Adds the generic link to the header.
        /// </summary>
        /// <param name="type">
        /// The type string.
        /// </param>
        /// <param name="relation">
        /// The relation string.
        /// </param>
        /// <param name="title">
        /// The title string.
        /// </param>
        /// <param name="href">
        /// The href string.
        /// </param>
        public virtual void AddGenericLink(string type, string relation, string title, string href)
        {
            Scripting.Helpers.AddGenericLink(this, type, relation, title, href);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the default stylesheet language
        /// </summary>
        protected virtual void AddDefaultLanguages()
        {
            Response.AppendHeader("Content-Style-Type", "text/css");
            Response.AppendHeader("Content-Script-Type", "text/javascript");
        }

        /// <summary>
        /// Adds the content-type meta tag to the header.
        /// </summary>
        protected virtual void AddMetaContentType()
        {
            var meta = new HtmlMeta
                {
                    HttpEquiv = "content-type",
                    Content =
                        string.Format(
                            "{0}; charset={1}", Response.ContentType, Response.ContentEncoding.HeaderName)
                };
            Page.Header.Controls.Add(meta);
        }

        /// <summary>
        /// Add a meta tag to the page's header.
        /// </summary>
        /// <param name="name">
        /// The tag name.
        /// </param>
        /// <param name="value">
        /// The tag value.
        /// </param>
        protected virtual void AddMetaTag(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return;

            const string tag = "\n<meta name=\"{0}\" content=\"{1}\" />";
            Header.Controls.Add(new LiteralControl(string.Format(tag, name, value)));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.TemplateControl.Error"></see> event.
        /// </summary>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs"></see> that contains the event data.
        /// </param>
        protected override void OnError(EventArgs e)
        {
            var ctx = HttpContext.Current;
            var exception = ctx.Server.GetLastError();

            if (exception != null && exception.Message.Contains("callback"))
            {
                // This is a robot spam attack so we send it a 404 status to make it go away.
                ctx.Response.StatusCode = 404;
                ctx.Server.ClearError();
                Comment.OnSpamAttack();
            }

            base.OnError(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// Adds links and javascript to the HTML header tag.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string relativeWebRoot = Utils.RelativeWebRoot;
            Uri absoluteWebRoot = Utils.AbsoluteWebRoot;
            string instanceName = BlogSettings.Instance.Name;

            if (!Page.IsCallback)
            {
                // Links
                AddGenericLink("contents", "Archive", string.Format("{0}archive.aspx", relativeWebRoot));
                AddGenericLink("start", instanceName, relativeWebRoot);
                AddGenericLink("application/rdf+xml", "meta", "SIOC", string.Format("{0}sioc.axd", absoluteWebRoot));
                AddGenericLink("application/apml+xml", "meta", "APML", string.Format("{0}apml.axd", absoluteWebRoot));
                AddGenericLink("application/rdf+xml", "meta", "FOAF", string.Format("{0}foaf.axd", absoluteWebRoot));

                if (string.IsNullOrEmpty(BlogSettings.Instance.AlternateFeedUrl))
                {
                    AddGenericLink(
                        "application/rss+xml",
                        "alternate",
                        string.Format("{0} (RSS)", instanceName),
                        string.Format("{0}?format=rss", Utils.FeedUrl));
                    AddGenericLink(
                        "application/atom+xml",
                        "alternate",
                        string.Format("{0} (ATOM)", instanceName),
                        string.Format("{0}?format=atom", Utils.FeedUrl));
                }
                else
                {
                    AddGenericLink("application/rss+xml", "alternate", instanceName, Utils.FeedUrl);
                }

                AddGenericLink("application/rsd+xml", "edituri", "RSD", string.Format("{0}rsd.axd", absoluteWebRoot));

                AddMetaContentType();

                AddDefaultLanguages();

                if (BlogSettings.Instance.EnableOptimization)
                {
                    Scripting.Helpers.AddBundledStylesAndScripts(this);
                }
                else
                {
                    Scripting.Helpers.AddStylesAndScripts(this);
                }

                if (BlogSettings.Instance.EnableOpenSearch)
                {
                    AddGenericLink(
                        "application/opensearchdescription+xml",
                        "search",
                        instanceName,
                        string.Format("{0}opensearch.axd", absoluteWebRoot));
                }

                Scripting.Helpers.AddCustomCodeToHead(this);

                Scripting.Helpers.AddTrackingScript(this);
            }       
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.PreInit"/> event at the beginning of page initialization.
        /// Assignes the selected theme to the pages.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnPreInit(EventArgs e)
        {
            bool allowViewing = false;

            // - To prevent authenticated users from accessing the site, you would assign
            //   that user to a role that does not have the right to ViewPublicPosts.
            // - To prevent unauthenticated users from accessing the site, remove
            //   the ViewPublicPosts from the Anonymous role.
            // - If the user is authenticated, but hasn't been assigned to any roles, allow
            //   them to access the site.
            // - Even though we allow authenticated users without any roles to access the
            //   site, the user will still usually not be able to view any published posts.
            //   It is ideal that all users are assigned to a role, even if that role has
            //   minimal rights such as ViewPublicPosts.

            if (Security.IsAuthorizedTo(Rights.ViewPublicPosts))
                allowViewing = true;
            else if (Security.IsAuthenticated && Security.GetCurrentUserRoles().Length == 0)
                allowViewing = true;

            if (!allowViewing)
                Response.Redirect(string.Format("{0}Account/login.aspx", Utils.RelativeWebRoot));

            MasterPageFile = string.Format("{0}themes/{1}/site.master", Utils.ApplicationRelativeWebRoot, BlogSettings.Instance.GetThemeWithAdjustments(null));

            base.OnPreInit(e);

            if (Page.IsPostBack || string.IsNullOrEmpty(Request.QueryString["deletepost"]))
                return;

            var post = Post.GetPost(new Guid(Request.QueryString["deletepost"]));
            if (post == null || !post.CanUserDelete)
                return;

            post.Delete();
            post.Save();
            Response.Redirect(Utils.RelativeWebRoot);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.PreRenderComplete"></see> event after 
        ///     the <see cref="M:System.Web.UI.Page.OnPreRenderComplete(System.EventArgs)"></see> event and before the page is rendered.
        /// </summary>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs"></see> that contains the event data.
        /// </param>
        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);
            if (BlogSettings.Instance.UseBlogNameInPageTitles)
            {
                Page.Title = string.Format("{0} | {1}", BlogSettings.Instance.Name, Page.Title);
            }
        }

        /// <summary>
        /// Initializes the <see cref="T:System.Web.UI.HtmlTextWriter"></see> object and calls on the child
        ///     controls of the <see cref="T:System.Web.UI.Page"></see> to render.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="T:System.Web.UI.HtmlTextWriter"></see> that receives the page content.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(new RewriteFormHtmlTextWriter(writer));
        }

        #endregion
    }
}