using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool Correct { get; set; }
        public string Value { get; set; }

        public Question Question => QuestionsAnswer.First(qa => qa.AnswerId == Id).Question;

        public ICollection<QuestionsAnswer> QuestionsAnswer { get; set; }
        public ICollection<TestQuestion> TestQuestion { get; set; }
    }
}
