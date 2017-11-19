using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class TestQuestion
    {
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }

        public Answer Answer { get; set; }
        public Question Question { get; set; }
        public Test Test { get; set; }
    }
}
