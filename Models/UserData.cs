using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Team { get; set; }
        public int? Ar { get; set; }
        public int? Sr { get; set; }
        public int? Hr { get; set; }
        public DateTime? Arcooldown { get; set; }
        public DateTime? Srcooldown { get; set; }
        public DateTime? Hrcooldown { get; set; }
        public sbyte ArIrdp { get; set; }
        public sbyte SrIrdp { get; set; }
        public sbyte HrIrdp { get; set; }
        public sbyte HrPayment { get; set; }

        public Test ArNavigation { get; set; }
        public Test HrNavigation { get; set; }
        public Users IdNavigation { get; set; }
        public Test SrNavigation { get; set; }
    }
}
