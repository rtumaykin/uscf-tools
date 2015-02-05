namespace uscf_tools.player_search.models
{
    /// <summary>
    /// Provides an enumeration for various rating statuses
    /// </summary>
    public enum RatingStatus
    {
        /// <summary>
        /// Unrated
        /// </summary>
        Unrated = 0,

        /// <summary>
        /// Provisional. This status is considered unstable, and is based on the amount of rated games played.
        /// </summary>
        Provisional = 1,

        /// <summary>
        /// Established. After 25 games the player is assigned an established rating
        /// </summary>
        Established = 2
    }
}
