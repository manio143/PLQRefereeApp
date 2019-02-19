using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PLQRefereeApp
{
    [Table("Test")]
    public partial class Test
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }
        public RulebookVersion Rulebook { get; set; }
        public int? Mark { get; set; }

        public User User { get; set; }

        public IEnumerable<Question> Questions { get; set; }
    }

    public static class TestDuration
    {
        public static TimeSpan Duration(this QuestionType @this) {
            switch (@this)
            {
                case QuestionType.AR: return new TimeSpan(0, 20, 0);
                case QuestionType.SR: return new TimeSpan(0, 20, 0);
                case QuestionType.HR: return new TimeSpan(0, 35, 0);
                default: throw new ArgumentException("Invalid value");
            }
        }
        public static int QuestionCount(this QuestionType @this) {
            switch (@this)
            {
                case QuestionType.AR: return 25;
                case QuestionType.SR: return 25;
                case QuestionType.HR: return 50;
                default: throw new ArgumentException("Invalid value");
            }
        }
    }
}
