using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackyStaty03.Models
{
    public class Season
    {
        public Guid GuidId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OWHAId { get; set; }
        public ObservableCollection<League> Children { get; set; } = new ObservableCollection<League>();
    }
}
