using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public partial class Test
    {
        public Test()
        {
            TestQuestion = new HashSet<TestQuestion>();
            UserDataArNavigation = new HashSet<UserData>();
            UserDataHrNavigation = new HashSet<UserData>();
            UserDataSrNavigation = new HashSet<UserData>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }

        public User User { get; set; }

        private IEnumerable<Question> _questions = null;
        public IEnumerable<Question> Questions {
            get
            {
                if (_questions == null)
                    _questions = TestQuestion.Where(tq => tq.TestId == Id).Select(tq => tq.Question).ToList();
                return _questions;
            }
            set
            {
                _questions = value;
            }
        }

        public ICollection<TestQuestion> TestQuestion { get; set; }
    }
}
