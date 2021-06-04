using System;
using System.Collections.Generic;

namespace Spectre.Console
{
    public static class SearchPromptExtensions
    {
        public static SearchPrompt<T> AddChoices<T>(this SearchPrompt<T> prompt, IEnumerable<T> choices)
            where T : notnull
        {
            foreach (var choice in choices)
            {
                prompt.AddChoice(choice);
            }

            return prompt;
        }

        public static SearchPrompt<T> UseConverter<T>(this SearchPrompt<T> prompt, Func<T, string>? displaySelector)
            where T : notnull
        {
            prompt.Converter = displaySelector;
            return prompt;
        }

        public static SearchPrompt<T> UseComparer<T>(this SearchPrompt<T> prompt, Func<T, string, bool> comparer)
            where T : notnull
        {
            prompt.Comparer = comparer;
            return prompt;
        }

        public static SearchPrompt<T> WithPageSize<T>(this SearchPrompt<T> prompt, int pageSize)
            where T : notnull
        {
            if (pageSize <= 0)
            {
                throw new ArgumentException(null, nameof(pageSize));
            }
            
            prompt.PageSize = pageSize;
            return prompt;
        }
    }
}