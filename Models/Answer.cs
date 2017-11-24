using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PLQRefereeApp
{
    [Table("Answer")]
    public partial class Answer
    {
        public int Id { get; set; }
        public bool Correct { get; set; }
        public string Value { get; set; }
    }
}
