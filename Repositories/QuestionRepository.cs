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

        private IEnumerable<Question> GetQuestionsWhere(Func<Question, bool> p) => Context.Questions.Where(p).ToList();
        public IEnumerable<Question> GetAllQuestions() => GetQuestionsWhere(_ => true);
        public IEnumerable<Question> GetARQuestions() => GetQuestionsWhere(q => q.Type == QuestionType.AR);
        public IEnumerable<Question> GetSRQuestions() => GetQuestionsWhere(q => q.Type == QuestionType.SR);
        public IEnumerable<Question> GetHRQuestions() => GetQuestionsWhere(q => q.Type == QuestionType.HR);
        public IEnumerable<Question> GetARQuestions(RulebookVersion rulebookVersion) => GetQuestionsWhere(q => q.Type == QuestionType.AR && q.Rulebook == rulebookVersion);
        public IEnumerable<Question> GetSRQuestions(RulebookVersion rulebookVersion) => GetQuestionsWhere(q => q.Type == QuestionType.SR && q.Rulebook == rulebookVersion);
        public IEnumerable<Question> GetHRQuestions(RulebookVersion rulebookVersion) => GetQuestionsWhere(q => q.Type == QuestionType.HR && q.Rulebook == rulebookVersion);
        internal IEnumerable<Answer> GetAnswersFor(Question q)
        {
            return Context.QuestionsAnswers.Where(qa => qa.QuestionId == q.Id).ToList().Select(qa => qa.AnswerId).Select(ai => Context.Answers.First(a => a.Id == ai));
        }
    }
}