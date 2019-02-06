using InfluxDB.Client.Domain;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class PageLinkTest
    {
        private class PageLinks : AbstractPageLinks
        {
        }

        [Test]
        public void IsNotFindOptions()
        {
            var pageLinks = new PageLinks();
            pageLinks.Links.Add("next", "/api/v2/buckets?descendin=true\u0026limi=20\u0026offse=10\u0026sortB=ID");

            Assert.IsNull(pageLinks.GetPrevPage());
            Assert.IsNull(pageLinks.GetSelfPage());
            Assert.IsNull(pageLinks.GetNextPage());
        }

        [Test]
        public void MappingLinks()
        {
            var pageLinks = new PageLinks();
            pageLinks.Links.Add("self", "/api/v2/buckets?descending=false\u0026limit=20\u0026offset=0");

            Assert.IsNull(pageLinks.GetPrevPage());
            Assert.IsNull(pageLinks.GetNextPage());
            Assert.IsNotNull(pageLinks.GetSelfPage());
            Assert.AreEqual(20, pageLinks.GetSelfPage().Limit);
            Assert.AreEqual(false, pageLinks.GetSelfPage().Descending);
            Assert.AreEqual(0, pageLinks.GetSelfPage().Offset);
            Assert.AreEqual(null, pageLinks.GetSelfPage().SortBy);
        }

        [Test]
        public void SortBy()
        {
            var pageLinks = new PageLinks();
            pageLinks.Links.Add("next", "/api/v2/buckets?descending=true\u0026limit=20\u0026offset=10\u0026sortBy=ID");

            Assert.IsNull(pageLinks.GetPrevPage());
            Assert.IsNull(pageLinks.GetSelfPage());
            Assert.IsNotNull(pageLinks.GetNextPage());
            Assert.AreEqual(20, pageLinks.GetNextPage().Limit);
            Assert.AreEqual(true, pageLinks.GetNextPage().Descending);
            Assert.AreEqual(10, pageLinks.GetNextPage().Offset);
            Assert.AreEqual("ID", pageLinks.GetNextPage().SortBy);
        }
    }
}