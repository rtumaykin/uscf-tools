using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using uscf_tools.player_search.dto;
using uscf_tools.player_search.models;

[assembly: InternalsVisibleTo("uscf-tools.tests")]
namespace uscf_tools.player_search
{
    /// <summary>
    /// Encapsulates methods used for searching for a player
    /// </summary>
    public class Api
    {

        internal class FullName
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Suffix { get; set; }
        }

        // the logic here should be like this:
        // Go to the mobile api and run search eigther by the name/state, or by id.
        // if the name has more than 2 parts, then use desktop search using the id in question.
        // Desktop site returns the names in the form last name, first name, so it is easy to reconstruct the original first and last name
        public IEnumerable<UscfSearchRecord> SearchByName(SearchByNameRequest searchByNameRequest)
        {
            var mobileApiResult = MobileApi.SearchByName(searchByNameRequest).ToList();

            return ProcessSearchResults(mobileApiResult);
        }

        public UscfMember LookupById(LookupByIdRequest lookupByIdRequest)
        {
            var lookupResult = MobileApi.LookupById(lookupByIdRequest);

            var mobileApiResult = new List<MobileApiResult>(new[] {lookupResult});
            
            var searchResult = ProcessSearchResults(mobileApiResult).FirstOrDefault();

            return searchResult == null
                ? null
                : new UscfMember
                {
                    FirstName = searchResult.FirstName,
                    LastName = searchResult.LastName,
                    Suffix = searchResult.Suffix,
                    StateOrCountry = searchResult.StateOrCountry,
                    UscfId = lookupResult.UscfId,
                    MembershipExpirationDate = lookupResult.MembershipExpirationDate,
                    MembershipStatus = lookupResult.MembershipStatus,
                    Comments = lookupResult.Comments,
                    BlitzRating = lookupResult.BlitzRating,
                    RegularRating = lookupResult.RegularRating,
                    QuickRating = lookupResult.QuickRating
                };
        }

        protected virtual IEnumerable<LocalNameResolutionResponse> ResolveNamesLocally(
            LocalNameResolutionRequest[] localNameResolutionRequests)
        {
            return null;
        }

        protected virtual void UpdateLocalNameResolutionData(LocalNameResolutionUpdateRequest[] data)
        {
            
        }


        private IEnumerable<UscfSearchRecord> ProcessSearchResults(List<MobileApiResult> mobileApiResult)
        {
            var searchResult = (from apiResult in mobileApiResult
                let parsedName = ParseName_FirstPass(apiResult.FullName)
                where parsedName != null
                select new UscfSearchRecord
                {
                    UscfId = apiResult.UscfId,
                    FirstName = parsedName.FirstName,
                    LastName = parsedName.LastName,
                    Suffix = parsedName.Suffix,
                    StateOrCountry = apiResult.StateOrCountry,
                    MembershipStatus = apiResult.MembershipStatus,
                    MembershipExpirationDate = apiResult.MembershipExpirationDate,
                    RatingStatus =
                        apiResult.RegularRating == null ? RatingStatus.Unrated : apiResult.RegularRating.Status,
                    Rating = apiResult.RegularRating == null ? 0 : apiResult.RegularRating.Value,
                    ProvisionalRatingGamesCount =
                        apiResult.RegularRating == null ? 0 : apiResult.RegularRating.GamesCount ?? 0,
                    Comments = apiResult.Comments
                }).ToList();

            var resolvedIds = searchResult.Select(result => result.UscfId).ToArray();

            var unresolvedNames = (from item in mobileApiResult
                where !resolvedIds.Contains(item.UscfId)
                select new LocalNameResolutionRequest
                {
                    UscfId = item.UscfId, UscfFullName = item.FullName
                }).ToArray();

            var localNameResolutionResults = ResolveNamesLocally(unresolvedNames);

            if (localNameResolutionResults != null)
            {
                searchResult.AddRange(from row in localNameResolutionResults
                    let unresolvedRecord = mobileApiResult.FirstOrDefault(item => item.UscfId == row.UscfId)
                    select new UscfSearchRecord
                    {
                        UscfId = row.UscfId,
                        FirstName = row.FirstName,
                        LastName = row.LastName,
                        Suffix = row.Suffix,
                        StateOrCountry = unresolvedRecord.StateOrCountry,
                        MembershipStatus = unresolvedRecord.MembershipStatus,
                        MembershipExpirationDate = unresolvedRecord.MembershipExpirationDate,
                        RatingStatus =
                            unresolvedRecord.RegularRating == null
                                ? RatingStatus.Unrated
                                : unresolvedRecord.RegularRating.Status,
                        Rating = unresolvedRecord.RegularRating == null ? 0 : unresolvedRecord.RegularRating.Value,
                        ProvisionalRatingGamesCount =
                            unresolvedRecord.RegularRating == null
                                ? 0
                                : unresolvedRecord.RegularRating.GamesCount ?? 0,
                        Comments = unresolvedRecord.Comments
                    });

                resolvedIds = searchResult.Select(result => result.UscfId).ToArray();
            }

            var unparsedNamePlayerIds = (from item in mobileApiResult
                                   where !resolvedIds.Contains(item.UscfId)
                                         select new LookupByIdRequest
                                   {
                                       UscfId = item.UscfId
                                   }).ToArray();

            if (unparsedNamePlayerIds.Length > 0)
            {
                var desktopResult = DesktopApi.LookupByIds(unparsedNamePlayerIds);

                searchResult.AddRange(from resultItem in desktopResult
                    let unresolvedRecord =
                        mobileApiResult.FirstOrDefault(item => item.UscfId == resultItem.MemberNumber)
                    where unresolvedRecord != null
                    let parsedName = ParseName_UsingReverseName(unresolvedRecord.FullName, resultItem.FullName)
                    select new UscfSearchRecord
                    {
                        UscfId = resultItem.MemberNumber,
                        FirstName = parsedName.FirstName,
                        LastName = parsedName.LastName,
                        Suffix = parsedName.Suffix,
                        StateOrCountry = unresolvedRecord.StateOrCountry,
                        MembershipStatus = unresolvedRecord.MembershipStatus,
                        MembershipExpirationDate = unresolvedRecord.MembershipExpirationDate,
                        RatingStatus =
                            unresolvedRecord.RegularRating == null
                                ? RatingStatus.Unrated
                                : unresolvedRecord.RegularRating.Status,
                        Rating =
                            unresolvedRecord.RegularRating == null ? 0 : unresolvedRecord.RegularRating.Value,
                        ProvisionalRatingGamesCount =
                            unresolvedRecord.RegularRating == null
                                ? 0
                                : unresolvedRecord.RegularRating.GamesCount ?? 0,
                        Comments = unresolvedRecord.Comments
                    });
            }

            var recordsToSave = new List<LocalNameResolutionUpdateRequest>();
            recordsToSave.AddRange(from searchResultItem in searchResult
                                   let originalItem = mobileApiResult.FirstOrDefault(item => item.UscfId == searchResultItem.UscfId)
                                   select new LocalNameResolutionUpdateRequest
                                   {
                                       FirstName = searchResultItem.FirstName,
                                       LastName = searchResultItem.LastName,
                                       StateOrCountry = searchResultItem.StateOrCountry,
                                       Suffix = searchResultItem.Suffix,
                                       UscfId = searchResultItem.UscfId,
                                       UscfFullName = originalItem.FullName
                                   });

            // save all resolved resords to cache
            UpdateLocalNameResolutionData(recordsToSave.ToArray());
            
            return searchResult;
        }


        internal static FullName ParseName_FirstPass(string fullName)
        {
            var namesArray = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // if this array has only 2 entries - it is simple - first is the first name, last is the last name
            if (namesArray.Length == 2)
                return new FullName
                {
                    FirstName = namesArray[0],
                    LastName = namesArray[1]
                };

            // if the above fails - we will be calling the Desktop Api and search by the Id. The result will contain name in the form of 
            // LastName comma FirstName.
            // since I don't want to call that web site for each player, I will return the full name in the FirstName field and this will indicate 
            // that an additional call to the desktop site needs to be made
            return null;
        }
        internal static FullName ParseName_UsingReverseName(string fullName, string reverseName)
        {
            // trim these values
            fullName = fullName.Trim();
            reverseName = reverseName.Trim();

            // quick way to see how many commas are in each name version
            var fullNameCommaCount = fullName.Length - fullName.Replace(",", "").Length;
            var reverseNameCommaCount = reverseName.Length - reverseName.Replace(",", "").Length;

            // simple case - the "Last Name, First Name" notation contains only one comma more than a "First Name Last Name" notation.
            // This means that the name consists only from 2 parts - and the question is only how to find these parts.
            if (fullNameCommaCount == reverseNameCommaCount - 1)
            {
                var commaPos = reverseName.IndexOf(',');
                while (commaPos >= 0 && commaPos < reverseName.Length)
                {
                    var trialSplitFullName = new FullName
                    {
                        LastName = reverseName.Substring(0, commaPos).Trim(),
                        FirstName =
                            commaPos < reverseName.Length - 1
                                ? reverseName.Substring(commaPos + 1).Trim()
                                : String.Empty
                    };

                    // if the 2 parts match then we're OK
                    if (trialSplitFullName.FirstName == fullName.Substring(0, trialSplitFullName.FirstName.Length)
                        &&
                        trialSplitFullName.LastName ==
                        fullName.Substring(fullName.Length - trialSplitFullName.LastName.Length))
                    {
                        // do a cleanup
                        trialSplitFullName.FirstName = trialSplitFullName.FirstName.Replace(",", " ").ReplaceAll("  ", " ");
                        trialSplitFullName.LastName = trialSplitFullName.LastName.Replace(",", " ").ReplaceAll("  ", " ");

                        return trialSplitFullName;
                    }

                    // find next before continue loop
                    commaPos = commaPos >= reverseName.Length - 1 ? -1 : reverseName.IndexOf(',', commaPos + 1);
                }
            }

            // this would be the case when the suffix is also used and in the reverse name it is returned as 
            // lastName,firstName,suffix. And in the normal name it would be firstName lastName suffix.
            if (fullNameCommaCount == reverseNameCommaCount - 2)
            {
                // here we will try to walk forward through the commas to get the last name, and at the same time walk backwards to get the suffix
                // the combination of last name space suffix should match the last part of the fullName.
                var lastNameCommaPos = reverseName.IndexOf(',');
                while (lastNameCommaPos > 0 && lastNameCommaPos < reverseName.Length)
                {
                    var suffixCommaPos = reverseName.LastIndexOf(',');

                    // do not allow overlapping
                    while (suffixCommaPos != -1 && suffixCommaPos > lastNameCommaPos)
                    {
                        // at most we need to trim around the names and then replace double spaces with single spaces to find a match
                        var trialFullName = new FullName
                        {
                            LastName = reverseName.Substring(0, lastNameCommaPos).Trim(),
                            Suffix = reverseName.Substring(suffixCommaPos + 1).Trim(),
                            FirstName =
                                reverseName.Substring(lastNameCommaPos + 1, suffixCommaPos - lastNameCommaPos - 1).Trim()
                        };

                        if (
                            (trialFullName.FirstName + " " + trialFullName.LastName + " " + trialFullName.Suffix)
                                .ReplaceAll("  ", " ") == fullName.ReplaceAll("  ", " "))
                        {
                            // do a cleanup
                            trialFullName.FirstName = trialFullName.FirstName.Replace(",", " ").ReplaceAll("  ", " ");
                            trialFullName.LastName = trialFullName.LastName.Replace(",", " ").ReplaceAll("  ", " ");

                            return trialFullName;
                        }

                        suffixCommaPos = suffixCommaPos == 0 ? -1 : reverseName.LastIndexOf(',', suffixCommaPos - 1);
                    }
                    lastNameCommaPos = lastNameCommaPos >= reverseName.Length - 1
                        ? -1
                        : reverseName.IndexOf(',', lastNameCommaPos + 1);
                }
            }


            // we don't know how to parse this
            return new FullName
            {
                FirstName = "",
                LastName = fullName
            };
        }
    }
}
