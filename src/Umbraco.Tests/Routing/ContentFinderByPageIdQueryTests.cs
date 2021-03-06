﻿using Moq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByPageIdQueryTests : BaseWebTest
    {
        [TestCase("/?umbPageId=1046", 1046)]
        [TestCase("/?UMBPAGEID=1046", 1046)]
        [TestCase("/default.aspx?umbPageId=1046", 1046)] // TODO: Should this match??
        [TestCase("/some/other/page?umbPageId=1046", 1046)] // TODO: Should this match??
        [TestCase("/some/other/page.aspx?umbPageId=1046", 1046)] // TODO: Should this match??
        public void Lookup_By_Page_Id(string urlAsString, int nodeMatch)
        {
            var umbracoContext = GetUmbracoContext(urlAsString);
            var httpContext = GetHttpContextFactory(urlAsString).HttpContext;
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var mockRequestAccessor = new Mock<IRequestAccessor>();
            mockRequestAccessor.Setup(x => x.GetRequestValue("umbPageID")).Returns(httpContext.Request.QueryString["umbPageID"]);

            var lookup = new ContentFinderByPageIdQuery(mockRequestAccessor.Object);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
        }
    }
}
