using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class Answer
    {
        public Answer()
        {
            QuestionsAnswer = new HashSet<QuestionsAnswer>();
            TestQuestion = new HashSet<TestQuestion>();
        }

        public int Id { get; set; }
        public sbyte Correct { get; set; }
        public string Answer1 { get; set; }

        public ICollection<QuestionsAnswer> QuestionsAnswer { get; set; }
        public ICollection<TestQuestion> TestQuestion { get; set; }
    }
}
