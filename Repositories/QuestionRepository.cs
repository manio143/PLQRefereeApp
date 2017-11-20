using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public class QuestionRepository
    {
        public QuestionRepository(MainContext mainCtx)
        {
            Context = mainCtx;
        }

        private MainContext Context { get; }

        private IEnumerable<Question> GetQuestionsWhere(Func<Question, bool> p) => Context.Questions.Where(p);
        public IEnumerable<Question> GetAllQuestions() => GetQuestionsWhere(_ => true);
        public IEnumerable<Question> GetARQuestions() => GetQuestionsWhere(q => q.Type == QuestionType.AR);
        public IEnumerable<Question> GetSRQuestions() => GetQuestionsWhere(q => q.Type == QuestionType.SR);
        public IEnumerable<Question> GetHRQuestions() => GetQuestionsWhere(q => q.Type == QuestionType.HR);
    }
}