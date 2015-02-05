using System;

namespace uscf_tools.player_search.models
{
    public class UscfMember
    {
        /// <summary>
        /// An integer number representing the USCF Id.
        /// </summary>
        public int UscfId { get; set; }

        /// <summary>
        /// First Name. Also may contain middle initial or whole middle name, as USCF does not store it separately.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Member Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Name suffix
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Member State or Country
        /// </summary>
        public string StateOrCountry { get; set; }

        /// <summary>
        /// Status of the member
        /// </summary>
        public MembershipStatus MembershipStatus { get; set; }

        /// <summary>
        /// Membership expiration date. Usually all of them have it
        /// </summary>
        public DateTime MembershipExpirationDate { get; set; }

        /// <summary>
        /// Member's regular rating
        /// </summary>
        public Rating RegularRating { get; set; }
        
        /// <summary>
        /// Member's quick rating
        /// </summary>
        public Rating QuickRating { get; set; }
        
        /// <summary>
        /// Member's blitz rating
        /// </summary>
        public Rating BlitzRating { get; set; }
        
        /// <summary>
        /// Various comments from the web site
        /// </summary>
        public string Comments { get; set; }

    }
}
