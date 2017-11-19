using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class Test
    {
        public Test()
        {
            TestQuestion = new HashSet<TestQuestion>();
            UserDataArNavigation = new HashSet<UserData>();
            UserDataHrNavigation = new HashSet<UserData>();
            UserDataSrNavigation = new HashSet<UserData>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }

        public Users User { get; set; }
        public ICollection<TestQuestion> TestQuestion { get; set; }
        public ICollection<UserData> UserDataArNavigation { get; set; }
        public ICollection<UserData> UserDataHrNavigation { get; set; }
        public ICollection<UserData> UserDataSrNavigation { get; set; }
    }
}
