using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackyStaty03.Models
{
    public class OWRoot
    {
        public Guid GuidId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<Season> Children { get; set; } = [];
    }
}
