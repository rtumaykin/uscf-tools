using System.Linq;
using SelfValidatingModel;

namespace uscf_tools.player_search.models
{
    /// <summary>
    /// Model for sending the Search By Name request to the api
    /// </summary>
    public class SearchByNameRequest : SelfValidatingModelBase
    {
        private static readonly string[] UsStates =
        {
            "AK", "AL", "AR", "AS", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "FM", "GA", "GU", "HI", "IA", "ID", "IL",
            "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MH", "MI", "MN", "MO", "MP", "MS", "MT", "NC", "ND", "NE", "NH",
            "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "PR", "PW", "RI", "SC", "SD", "TN", "TX", "UM", "UT", "VA",
            "VI", "VT", "WA", "WI", "WV", "WY"
        };

        private string _lastName;
        /// <summary>
        /// Last Name for use in the search query. Minimum of 2 characters are required. 
        /// On the USCF site the search is performed by matching this value to the first characters of the player's last name.
        /// </summary>
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value.ToUpper();
            }
        }

        private string _firstName;

        /// <summary>
        /// First Name. Optional. If used, then on the USCF site search is performed anywhere within the player's first name
        /// </summary>
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value.ToUpper();
            }
        }

        private string _state;

        /// <summary>
        /// State. Optional. If provided must match an abbreviation of one of the US states, otherwise won't find any matches.
        /// </summary>
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value.ToUpper();
            }
        }

        protected override void CreateValidationRules()
        {
            AddValidationRule("LastName", () => string.IsNullOrWhiteSpace(LastName), () => "Last Name is required");
            AddValidationRule("LastName", () => string.IsNullOrWhiteSpace(LastName) || LastName.Length <= 2, () => "Last Name requires a minimum of 3 characters.");
            AddValidationRule("State", () => !string.IsNullOrWhiteSpace(State) && !UsStates.Contains(State),
                () => "If provided, State must be a 2 letter abbreviation of one of the US states");
        }
    }
}
