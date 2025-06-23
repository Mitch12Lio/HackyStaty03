using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackyStaty03.Models
{
    public class League
    {
        public Season? ParentSeason { get; set; } = null;
        public Guid GuidId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OWHAId { get; set; }
        public ObservableCollection<Division> Children { get; set; } = new ObservableCollection<Division>();
    }
}
