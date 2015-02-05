using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;
using uscf_tools.player_search.models;

namespace uscf_tools.player_search
{
    #region Public Facing DTO Models

    /// <summary>
    /// This class encapsulates data that is being returned by mobile api.
    /// </summary>
    internal class MobileApiResult
    {
        /// <summary>
        /// USCF Id
        /// </summary>
        public int UscfId { get; set; }

        /// <summary>
        /// Player's full name in the form "FirstName LastName[ Suffix]"
        /// </summary>
        public string FullName { get; set; }
        
        /// <summary>
        /// Player's state or country
        /// </summary>
        public string StateOrCountry { get; set; }

        /// <summary>
        /// Expiration date of membership
        /// </summary>
        public DateTime MembershipExpirationDate { get; set; }
        
        /// <summary>
        /// Status of membership. For more information <see cref="models.MembershipStatus"/>
        /// </summary>
        public MembershipStatus MembershipStatus { get; set; }

        public Rating RegularRating { get; set; }

        public Rating QuickRating { get; set; }

        public Rating BlitzRating { get; set; }

        /// <summary>
        /// USCF Comments
        /// </summary>
        public string Comments { get; set; }
    }
    #endregion // internally public Facing DTO Models

    internal static class MobileApi
    {
        internal enum ParsingResult : byte
        {
            Ok = 0,
            NameNeedsAdditionalParsing = 1,
            NameUnparsable = 2,
            RatingUnparsable = 4,
            StatusUnparsable = 8,
            ExpirationDateUnparsable = 16
        }


        private const string BaseUrl = "http://www.uschess.org";
        private const string SearchByNameUrl = "msa/thin2.php";
        private const string LookupByIdUrl = "msa/thin3.php?{id};{date}";

        public static IEnumerable<MobileApiResult> SearchByName(SearchByNameRequest searchByNameRequest)
        {
            var result = new List<MobileApiResult>();

            if (!searchByNameRequest.IsValid)
                throw searchByNameRequest.Exception;

            var client = new RestClient(BaseUrl);
            var request = new RestRequest(SearchByNameUrl, Method.POST)
                .AddParameter("memln", searchByNameRequest.LastName.Substring(0, Math.Min(searchByNameRequest.LastName.Length, 20)))
                .AddParameter("mode", "Search");

            if (!string.IsNullOrWhiteSpace(searchByNameRequest.FirstName))
                request.AddParameter("memfn", searchByNameRequest.FirstName.Substring(0, Math.Min(searchByNameRequest.FirstName.Length, 20)));

            if (!string.IsNullOrWhiteSpace(searchByNameRequest.State))
                request.AddParameter("memstate", searchByNameRequest.State);

            var response = client.Execute(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                var htmlContent = response.Content;
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                // second table always exists
                var rows = htmlDocument.DocumentNode.SelectNodes("//table")[1].SelectNodes("tr");

                result = (from row in rows
                    select row.SelectNodes("td")
                    into cells
                    where cells != null && (cells.Count == 3 || cells.Count == 4)
                    let infocell = cells[2].InnerText.Trim('\n').Split('\n').Select(val => val.Trim()).ToArray()
                    let comments = cells.Count == 4 ? cells[3].InnerText : string.Empty
                    let rating = ParseRating(infocell[2])
                    let membershipExpirationDate = ParseExpirationDate(infocell[1])
                    select new MobileApiResult
                    {
                        UscfId = ParseMemberNumber(cells[0].InnerText.Trim('\n')),
                        FullName = cells[1].InnerText.Trim('\n'),
                        StateOrCountry = infocell[0],
                        MembershipExpirationDate = membershipExpirationDate,
                        MembershipStatus = ParseMembershipStatus(membershipExpirationDate, comments),
                        RegularRating = rating,
                        Comments = comments
                    }).Where(item => item.UscfId > 0).ToList();
            }

            return result;
        }

        internal static MembershipStatus ParseMembershipStatus(DateTime membershipExpirationDate, string comments)
        {
            // First look what is in the comments:
            // this does not come out of the mobile site, but let me still have it here.
            if (comments.ToLower().Contains("non-member"))
                return MembershipStatus.NonMember;
            
            if (comments.ToLower().Contains("inactive"))
                return MembershipStatus.Inactive;

            if (comments.ToLower().Contains("deceased"))
                return MembershipStatus.Deceased;

            if (comments.ToLower().Contains("duplicate"))
                return MembershipStatus.Duplicate;

            if (membershipExpirationDate == DateTime.Parse("2099-12-31"))
                return MembershipStatus.Life;

            return MembershipStatus.Regular;
        }

        internal static DateTime ParseExpirationDate(string expirationDateString)
        {
            DateTime result;
            
            return DateTime.TryParse(expirationDateString, out result) ? result : DateTime.MinValue;
        }

        internal static int ParseMemberNumber(string memberNumberString)
        {
            int result;
            return int.TryParse(memberNumberString, out result) ? result : 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookupByIdRequest"></param>
        /// <returns></returns>
        public static MobileApiResult LookupById(LookupByIdRequest lookupByIdRequest)
        {
            if (!lookupByIdRequest.IsValid)
                throw lookupByIdRequest.Exception;
            var client = new RestClient(BaseUrl);
            var request = new RestRequest(LookupByIdUrl, Method.GET)
                .AddParameter("id", lookupByIdRequest.UscfId, ParameterType.UrlSegment)
                .AddParameter("date",
                    lookupByIdRequest.AsOfDate.HasValue ? lookupByIdRequest.AsOfDate.Value.ToString("yyyy-MM-dd") : "",
                    ParameterType.UrlSegment);

            var response = client.Execute(request);

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                var htmlContent = response.Content;
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                var rows = htmlDocument.DocumentNode.SelectNodes("//input");

                // get as much as we can from the input elements. All of them are required except for a comment. 
                // If any of them are missing just return null
                var fullNameNode = rows.FirstOrDefault(row => row.Attributes["name"].Value == "memname");
                if (fullNameNode == null)
                    return null;

                var fullNameString = fullNameNode.Attributes["value"].Value;
                if (string.IsNullOrWhiteSpace(fullNameString))
                    return null;

                var stateNode = rows.FirstOrDefault(row => row.Attributes["name"].Value == "state_country");
                if (stateNode == null)
                    return null;

                // state is optional.
                var state = stateNode.Attributes["value"].Value;

                var expDateNode = rows.FirstOrDefault(row => row.Attributes["name"].Value == "memexpdt");
                if (expDateNode == null)
                    return null;

                DateTime expirationDate;
                if (!DateTime.TryParse(expDateNode.Attributes["value"].Value, out expirationDate))
                    return null;

                var ratingNode = rows.FirstOrDefault(row => row.Attributes["name"].Value == "rating1");
                if (ratingNode == null)
                    return null;

                var ratingString = ratingNode.Attributes["value"].Value;
                if (string.IsNullOrWhiteSpace(ratingString))
                    return null;

                var rating = ParseRating(ratingString);

                var quickRatingNode = rows.FirstOrDefault(row => row.Attributes["name"].Value == "rating2");
                if (quickRatingNode == null)
                    return null;

                var quickRatingString = quickRatingNode.Attributes["value"].Value;
                if (string.IsNullOrWhiteSpace(quickRatingString))
                    return null;

                var quickRating = ParseRating(quickRatingString);

                var blitzRatingNode = rows.FirstOrDefault(row => row.Attributes["name"].Value == "rating3");
                if (blitzRatingNode == null)
                    return null;

                var blitzRatingString = blitzRatingNode.Attributes["value"].Value;
                if (string.IsNullOrWhiteSpace(blitzRatingString))
                    return null;

                var blitzRating = ParseRating(blitzRatingString);


                // get the comments
                var commentNode = htmlDocument.DocumentNode
                    .SelectSingleNode("//form")
                    .SelectNodes("//tr")[1]
                    .SelectSingleNode("//font[@color='red']");

                var comment = commentNode != null ? commentNode.InnerText.Trim() : string.Empty;


                return new MobileApiResult
                {
                    FullName = fullNameString,
                    UscfId = lookupByIdRequest.UscfId,
                    StateOrCountry = state,
                    MembershipExpirationDate = expirationDate,
                    RegularRating = rating,
                    QuickRating = quickRating,
                    BlitzRating = blitzRating,
                    Comments = comment,
                    MembershipStatus = ParseMembershipStatus(expirationDate, comment)
                };
            }

            return null;
        }

        internal static Rating ParseRating(string ratingRawString)
        {
            if (string.IsNullOrWhiteSpace(ratingRawString) || ratingRawString == "Unrated")
                return
                    new Rating
                    {
                        Status = RatingStatus.Unrated,
                        Value = 0
                    };

            // There should be only 2 possibilities - a number ending with * which means Established rating
            // and a number / number which means Unestablished rating / based on the games number
            var r = new Regex("(\\d+)[^\\d]");


            var regexMatches = r.Match(ratingRawString);

            // found matches
            if (regexMatches.Success && regexMatches.Groups.Count > 1)
            {
                var ratingValue = regexMatches.Groups[1].Value;
                var ratingTypeChar = ratingRawString.Substring(ratingValue.Length, 1);
                var value = int.Parse(ratingValue);

                switch (ratingTypeChar)
                {
                    case "*":
                        return new Rating
                        {
                            Status = RatingStatus.Established,
                            Value = value
                        };
                    case "/":
                        regexMatches = r.Match(ratingRawString.Substring(ratingValue.Length + 1));
                        return new Rating
                        {
                            Status = RatingStatus.Provisional,
                            Value = value,
                            GamesCount = int.Parse(regexMatches.Success && regexMatches.Groups.Count > 1
                                ? regexMatches.Groups[1].Value
                                : ratingRawString.Substring(ratingValue.Length + 1))
                        };
                    default:
                        throw new Exception(string.Format("Unable to parse rating string \"{0}\"", ratingRawString));
                }
            }
            return null;
        }
    }
}
