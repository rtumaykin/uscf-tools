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

            var resultApi = new Api().SearchByName(searchByNameRequest);
            //var resultDesktop = DesktopApi.SearchByName(searchByNameRequest);
            var ss = resultApi;
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
            var result = new Api().LookupById(lookupByIdRequest);
            var ss = result;
        }
        #endregion

        [Test]
        public void Should_Correctly_Parse_2_part_name()
        {
            const string twoPartName = "FIRSTNAME LASTNAME";
            var parsedName = Api.ParseName_FirstPass(twoPartName);
            Assert.IsNotNull(parsedName, "Parsed name should not be null");
            Assert.That(parsedName.FirstName == "FIRSTNAME", "First name remained unparsed");
            Assert.That(parsedName.LastName == "LASTNAME", "Last name remained unparsed");
        }

        [Test]
        public void Should_Correctly_Parse_3_part_name_with_aid_of_full_site_result()
        {
            const string firstNameLastName = "FIRSTNAME MIDDLENAME LASTNAME";
            const string lastNameFirstName = "LASTNAME,FIRSTNAME MIDDLENAME";

            var parsedName = Api.ParseName_UsingReverseName(firstNameLastName, lastNameFirstName);
            Assert.IsNotNull(parsedName, "Parsed name should not be null");
            Assert.That(parsedName.FirstName == "FIRSTNAME MIDDLENAME", "First name remained unparsed");
            Assert.That(parsedName.LastName == "LASTNAME", "Last name remained unparsed");
        }

        [Test]
        public void Should_Correctly_Parse_3_part_name_with_aid_of_full_site_result_simple()
        {
            const string firstNameLastName = "FIRSTNAME     MIDDLENAME LASTNAME";
            const string lastNameFirstName = "LASTNAME,FIRSTNAME     MIDDLENAME";

            var parsedName = Api.ParseName_UsingReverseName(firstNameLastName, lastNameFirstName);
            Assert.IsNotNull(parsedName, "Parsed name should not be null");
            Assert.That(parsedName.FirstName == "FIRSTNAME MIDDLENAME", "First name remained unparsed");
            Assert.That(parsedName.LastName == "LASTNAME", "Last name remained unparsed");
        }

        [Test]
        public void Should_Correctly_Parse_3_part_name_with_aid_of_full_site_result_commas_inside()
        {
            const string firstNameLastName = "FIRST, NAME MIDDLENAME LAST , NAME";
            const string lastNameFirstName = "LAST , NAME,FIRST, NAME MIDDLENAME";

            var parsedName = Api.ParseName_UsingReverseName(firstNameLastName, lastNameFirstName);
            Assert.IsNotNull(parsedName, "Parsed name should not be null");
            Assert.That(parsedName.FirstName == "FIRST NAME MIDDLENAME", "First name remained unparsed");
            Assert.That(parsedName.LastName == "LAST NAME", "Last name remained unparsed");
        }

        [Test]
        public void Should_Correctly_Parse_3_part_name_with_aid_of_full_site_result_suffix()
        {
            const string firstNameLastName = "FIRST, NAME MIDDLENAME LAST , NAME JR";
            const string lastNameFirstName = "LAST , NAME,FIRST, NAME MIDDLENAME, JR";

            var parsedName = Api.ParseName_UsingReverseName(firstNameLastName, lastNameFirstName);
            Assert.IsNotNull(parsedName, "Parsed name should not be null");
            Assert.That(parsedName.FirstName == "FIRST NAME MIDDLENAME", "First name remained unparsed");
            Assert.That(parsedName.LastName == "LAST NAME", "Last name remained unparsed");
            Assert.That(parsedName.Suffix == "JR", "Suffix is unparsed");
        }
    }
}
