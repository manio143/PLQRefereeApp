using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLQRefereeApp
{
    [Table("Users")]
    public partial class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Passphrase { get; set; }
        public bool Administrator { get; set; }
        public string Reset { get; set; }
    }
}
