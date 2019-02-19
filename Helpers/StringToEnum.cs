using System;
using System.Collections.Generic;

namespace PLQRefereeApp
{
    public static class StringToEnum
    {
        public static QuestionType ToQuestionType(this string s)
        {
            return Enum.Parse<QuestionType>(s, ignoreCase: true);
        }

        public static string String<T>(this IEnumerable<T> source) {
            return System.String.Join(", ", source);
        }
    }
}