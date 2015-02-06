using uscf_tools.player_search.models;

namespace uscf_tools.player_search.dto
{
    public class LocalNameResolutionUpdateRequest 
    {
        public int UscfId { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Suffix { get; set; }
        
        public string StateOrCountry { get; set; }
        
        public string UscfFullName { get; set; }
    }
}
