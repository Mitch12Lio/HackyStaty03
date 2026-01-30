using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackyStaty03.Models
{
    public class Team
    {
        public Division? ParentDivision { get; set; } = null;
        public Guid GuidId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OWHAId { get; set; }
        //public List<Division>? Children { get; set; } = null;

        public string DisplayName => $"{Name} - {OWHAId}";

    }
}
