using System;

namespace CrossSetaWeb.Models
{
    public class UserActivityLog
    {
        public int LogID { get; set; }
        public string UserName { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public string IPAddress { get; set; }
        public string Details { get; set; }
    }
}
