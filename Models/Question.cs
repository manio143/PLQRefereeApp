using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PLQRefereeApp
{
    public enum QuestionType
    {
        AR, SR, HR
    }

    [Table("Question")]
    public partial class Question
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Information { get; set; }
        public RulebookVersion Rulebook { get; set; }
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
    }
}
