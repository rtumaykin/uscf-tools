using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uscf_tools.player_search.dto
{
    public class LocalNameResolutionResponse
    {
        public int UscfId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
    }
}
