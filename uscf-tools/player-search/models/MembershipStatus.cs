namespace uscf_tools.player_search.models
{
    /// <summary>
    /// Return status of the membership
    /// </summary>
    public enum MembershipStatus : byte
    {
        /// <summary>
        /// Membership is current
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Life time membership. In mobile lookup it appears as 2099-12-31
        /// </summary>
        Life = 2,

        /// <summary>
        /// Only saw this in the desktop search where it says "Non-Member"
        /// </summary>
        NonMember = 3,

        /// <summary>
        /// In mobile site it appears in the comments
        /// </summary>
        Inactive = 4,

        /// <summary>
        /// In mobile site it appears in the comments
        /// </summary>
        Deceased = 5,
        
        /// <summary>
        /// In mobile it appears in comments as "Duplicate, use ID xxxxx" 
        /// In desktop it appears as "Dupl. ID" 
        /// </summary>
        Duplicate = 6
    }
}
