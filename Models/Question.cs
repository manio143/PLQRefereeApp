using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public enum QuestionType
    {
        AR, SR, HR
    }

    public partial class Question
    {
        public Question()
        {
            QuestionsAnswer = new HashSet<QuestionsAnswer>();
            TestQuestion = new HashSet<TestQuestion>();
        }

        public int Id { get; set; }
        public string Value { get; set; }
        public string Information { get; set; }
        public string TypeData
        {
            get
            {
                return Type.ToString();
            }
            set
            {
                if (Enum.TryParse(typeof(QuestionType), value, true, out var result))
                    Type = (QuestionType)result;
                else
                    throw new ArgumentException("Invalid paramter value; not in {AR, SR, HR}");
            }
        }
        public QuestionType Type { get; set; }

        public IEnumerable<Answer> Answers => QuestionsAnswer.Where(qa => qa.QuestionId == Id).Select(qa => qa.Answer);

        public ICollection<QuestionsAnswer> QuestionsAnswer { get; set; }
        public ICollection<TestQuestion> TestQuestion { get; set; }
    }
}
