using System;
using SelfValidatingModel;

namespace uscf_tools.player_search.models
{
    /// <summary>
    /// Model for sending the Lookup by Id request to the api
    /// </summary>
    public class LookupByIdRequest : SelfValidatingModelBase
    {
        /// <summary>
        /// USCF Id
        /// </summary>
        public int UscfId { get; set; }
        /// <summary>
        /// Date as of which the ratings should be retrieved from the USCF web site. 
        /// This property is optional. If it is not set, then the search will retrieve the latest ratings 
        /// </summary>
        public DateTime? AsOfDate { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UscfId", () => UscfId < 10000001, () => string.Format("Invalid USCF Id {0}. Must be greater than 10000000.", UscfId));
        }
    }
}
