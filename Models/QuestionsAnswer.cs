using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLQRefereeApp
{
    [Table("QuestionsAnswer")]
    public partial class QuestionsAnswer
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }
}
