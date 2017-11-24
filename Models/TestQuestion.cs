using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLQRefereeApp
{
    [Table("TestQuestion")]
    public partial class TestQuestion
    {
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }

    }
}
