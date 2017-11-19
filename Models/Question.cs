using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public partial class Question
    {
        public Question()
        {
            QuestionsAnswer = new HashSet<QuestionsAnswer>();
            TestQuestion = new HashSet<TestQuestion>();
        }

        public int Id { get; set; }
        public string Question1 { get; set; }
        public string Information { get; set; }
        public string Type { get; set; }

        public ICollection<QuestionsAnswer> QuestionsAnswer { get; set; }
        public ICollection<TestQuestion> TestQuestion { get; set; }
    }
}
