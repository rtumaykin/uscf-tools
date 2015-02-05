using System;

namespace uscf_tools.player_search.models
{
    public class UscfSearchRecord
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
        /// Type of the rating - unrated, regular, provisional
        /// </summary>
        public RatingStatus RatingStatus { get; set; }
        
        /// <summary>
        /// Rating score
        /// </summary>
        public int Rating { get; set; }
        
        /// <summary>
        /// Value, indicating the number of games this player has his or her rating based on
        /// </summary>
        public int ProvisionalRatingGamesCount { get; set; }
        
        /// <summary>
        /// Various comments from the web site
        /// </summary>
        public string Comments { get; set; }
    }
}
