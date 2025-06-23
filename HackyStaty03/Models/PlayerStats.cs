using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackyStaty03.Models
{
    public class PlayerStats
    {
        public int rank { get; set; }
        public int PtsPG { get; set; }
        public int GPG { get; set; }
        public int APG { get; set; }
        public int yellowCards { get; set; }
        public int redCards { get; set; }
        public int A { get; set; }
        public int AID { get; set; }
        public bool AP { get; set; }
        public int CATID { get; set; }
        public bool committed { get; set; }
        public int DID { get; set; }
        public string fname { get; set; } = string.Empty;
        public int G { get; set; }
        public int GP { get; set; }
        public int GTID { get; set; }
        public int GWG { get; set; }
        public bool import { get; set; }
        public bool injured { get; set; }
        public int jersey { get; set; }
        public string lname { get; set; } = string.Empty;
        public int pagecount { get; set; }
        public int PID { get; set; }
        public int PIM { get; set; }
        public float PIMx { get; set; }
        public string PIMs { get; set; } = string.Empty;
        public double PIMd { get; set; }
        public int PitchCount { get; set; }
        public int PPG { get; set; }
        public int PTS { get; set; }
        public bool reserve { get; set; }
        public bool rookie { get; set; }
        public int SHG { get; set; }
        public int SID { get; set; }
        public bool suspended { get; set; }
        public string seasonName { get; set; } = string.Empty;
        public string leagueName { get; set; } = string.Empty;
        public string divisionName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string createdTeamName { get; set; } = string.Empty;
        public int TID { get; set; }
        public int totalrecords { get; set; }
        public string Position { get; set; } = string.Empty;
        public int Touchdowns { get; set; }
        public int Sacks { get; set; }
        public int Interceptions { get; set; }
    }
}
