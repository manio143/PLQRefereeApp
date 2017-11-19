using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class QuestionsAnswer
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }

        public Answer Answer { get; set; }
        public Question Question { get; set; }
    }
}
