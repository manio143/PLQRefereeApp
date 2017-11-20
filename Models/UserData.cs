using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool? ArIrdp { get; set; }
        public bool? SrIrdp { get; set; }
        public bool? HrIrdp { get; set; }
        public bool? HrPayment { get; set; }

        public User User { get; set; }

        public bool IsArCertified => Ar != null || ArIrdp == true;
        public bool IsSrCertified => Sr != null || SrIrdp == true;
        public bool IsHrCertified => Hr != null || HrIrdp == true;
        public bool IsCertified => IsArCertified || IsSrCertified || IsHrCertified;
    }
}
