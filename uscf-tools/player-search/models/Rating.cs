namespace uscf_tools.player_search.models
{
    /// <summary>
    /// Encapsulates the rating information for a player
    /// </summary>
    public class Rating
    {
        /// <summary>
        /// Rating status - Unrated, Provisional or Established
        /// </summary>
        public RatingStatus Status { get; set; }

        /// <summary>
        /// Rating score itself
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// For provisional ratings this should contain the number of rated games the player has played 
        /// </summary>
        public int? GamesCount { get; set; }
    }
}
