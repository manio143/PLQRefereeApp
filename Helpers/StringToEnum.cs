using System;

namespace PLQRefereeApp
{
    public static class StringToEnum
    {
        public static QuestionType ToQuestionType(this string s)
        {
            return Enum.Parse<QuestionType>(s, ignoreCase: true);
        }
    }
}