using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackyStaty03.Models
{
    public class Division
    {
        public Season? ParentLeague { get; set; } = null;
        public Guid GuidId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OWHAId { get; set; }
        public ObservableCollection<Team> Children { get; set; } = new ObservableCollection<Team>();
    }
}
