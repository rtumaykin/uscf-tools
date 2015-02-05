using NUnit.Framework;
using uscf_tools.player_search;
using uscf_tools.player_search.models;

namespace uscf_tools.tests
{
    [TestFixture]
    public class Tests
    {
        #region Integration tests
        [Ignore("This is an integration test and shouldn't run automatically")]
        [Test]
        public void Should_Search_UsChess_Site()
        {
            var searchByNameRequest = new SearchByNameRequest()
            {
                LastName = "ivanov"
            };

            var resultApi = Api.SearchByName(searchByNameRequest);
            //var resultDesktop = rtumaykin.uscf_api.player_search.DesktopApi.SearchByName(searchByNameRequest);
        }

        [Test]
        [Ignore("This is an integration test and shouldn't run automatically")]
        public void Should_LookupById_Success()
        {
            var lookupByIdRequest = new LookupByIdRequest()
            {
                UscfId = 15007412
            };

            //            var resultApi = UsChessApi.MobileApi.SearchByName(searchByNameRequest);
            var result = Api.LookupById(lookupByIdRequest);
        }
        #endregion
    }
}
