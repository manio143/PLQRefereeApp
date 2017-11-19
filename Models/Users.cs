using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class Users
    {
        public Users()
        {
            Test = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string Passphrase { get; set; }
        public sbyte Administrator { get; set; }
        public string Reset { get; set; }

        public UserData UserData { get; set; }
        public ICollection<Test> Test { get; set; }
    }
}
