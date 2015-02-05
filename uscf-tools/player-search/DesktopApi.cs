using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;
using uscf_tools.player_search.models;

namespace uscf_tools.player_search
{
    internal class DesktopApiResult
    {
        public int MemberNumber { get; set; }
        public string FullName { get; set; }
        public string StateOrCountry { get; set; }
        public string MembershipExpirationDate { get; set; }
        public Rating Rating { get; set; }
        public string RatingString { get; set; }
        public string Comments { get; set; }
    }

    internal static class DesktopApi
    {
        private const string BaseUrl = "http://www.uschess.org";
        private const string SearchByNameUrl = "datapage/player-search.php";

        public static IEnumerable<DesktopApiResult> LookupByIds(LookupByIdRequest[] lookupByIdRequests)
        {
            var playerIds = String.Join(", ", (from lookupByIdRequest in lookupByIdRequests
                where lookupByIdRequest.IsValid
                select lookupByIdRequest.UscfId.ToString(CultureInfo.InvariantCulture)));
            


            var client = new RestClient(BaseUrl);
            var request = new RestRequest(SearchByNameUrl, Method.GET)
                .AddParameter("name", playerIds);

            var response = client.Execute(request);

            var result = new List<DesktopApiResult>();

            if (response.ResponseStatus != ResponseStatus.Completed) 
                return result;
            
            // remove all of these &nbsp that are polluting data
            var htmlContent = response.Content.Replace("&nbsp;", "");
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            int memberNumber;

            // second table always exists
            var rows = htmlDocument.DocumentNode.SelectSingleNode("//form[@action='./player-search.php']").ParentNode.SelectSingleNode("table").SelectNodes("tr");
            result.AddRange(from row in rows
                select row.SelectNodes("td")
                into cells
                where cells != null && cells.Count == 7 && int.TryParse(cells[0].InnerText.Trim(), out memberNumber)
                let rating = ParseRating(cells[1].InnerText.Trim())
                select new DesktopApiResult
                {
                    MemberNumber = int.Parse(cells[0].InnerText.Trim()), // should work since there was a condition above
                    FullName = cells[6].InnerText.Trim(),
                    MembershipExpirationDate = cells[5].InnerText.Trim(), // this may contain not a date but a comment
                    Rating = rating,
                    StateOrCountry = cells[4].InnerText.Trim(),
                    RatingString = cells[1].InnerText.Trim(),
                    Comments = string.Empty
                });

            return result;
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
