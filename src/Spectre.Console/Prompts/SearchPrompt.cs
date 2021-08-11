using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Rendering;

namespace Spectre.Console
{
    public class SearchPrompt<T> : IPrompt<T>
    {
        private readonly IList<T> _choices = new List<T>();
        public Func<T, string>? Converter { get; set; } = choice => choice?.ToString();
        public Func<T, string, bool> Comparer { get; set; }
        public int PageSize { get; set; } = 10;

        public SearchPrompt()
        {
            Comparer = (choice, input) => Converter(choice).StartsWith(input);
        }

        public void AddChoice(T item)
        {
            _choices.Add(item);
        }

        public T Show(IAnsiConsole console)
        {
            var input = string.Empty;
            var index = 0;
            var reRender = false;
            ConsoleKeyInfo? key = null;
            AnsiConsole.Clear();

            while (true)
            {
                var toRender = _choices.Where(x => Comparer(x, input)).Select(Converter).Take(PageSize).ToArray();

                if (key == null || reRender == true || (key.Value.Key != ConsoleKey.DownArrow &&
                                                        key.Value.Key != ConsoleKey.UpArrow &&
                                                        key.Value.Key != ConsoleKey.Backspace &&
                                                        key.Value.Key != ConsoleKey.Enter))
                {
                    reRender = false;
                    var newText = new Text($"Searching for: {input}");
                    AnsiConsole.Render(newText);
                    AnsiConsole.Render(Text.NewLine);
                    var list = new ListRenderable(toRender, index);
                    AnsiConsole.Render(list);
                }

                key = AnsiConsole.Console.Input.ReadKey(true);

                if (key.Value.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    index = 0;
                    input = input.Substring(0, input.Length - 1);
                    reRender = true;
                    AnsiConsole.Clear();
                    continue;
                }

                if (key.Value.Key == ConsoleKey.DownArrow && index < toRender.Length - 1)
                {
                    index += 1;
                    AnsiConsole.Render(new ClearRowRederable(index, toRender.Length + 1));
                    AnsiConsole.Render(new Text(toRender.ElementAt(index - 1)));
                    AnsiConsole.Render(new ClearRowRederable(index + 1, toRender.Length + 1));
                    AnsiConsole.Render(new SelectedItemRenderable(toRender.ElementAt(index)));
                    continue;
                }

                if (key.Value.Key == ConsoleKey.UpArrow && index > 0)
                {
                    index -= 1;
                    AnsiConsole.Render(new ClearRowRederable(index + 2, toRender.Length + 1));
                    AnsiConsole.Render(new Text(toRender.ElementAt(index + 1)));
                    AnsiConsole.Render(new ClearRowRederable(index + 1, toRender.Length + 1));
                    AnsiConsole.Render(new SelectedItemRenderable(toRender.ElementAt(index)));
                    continue;
                }

                if (key.Value.Key == ConsoleKey.Enter)
                {
                    if (toRender.Length == 0)
                    {
                        continue;
                    }

                    break;
                }

                if (char.IsLetterOrDigit(key.Value.KeyChar)
                    || char.IsWhiteSpace(key.Value.KeyChar)
                    || char.IsSymbol(key.Value.KeyChar)
                    || char.IsPunctuation(key.Value.KeyChar))
                {
                    index = 0;
                    input += key.Value.KeyChar;
                    AnsiConsole.Clear();
                    continue;
                }
            }

            return _choices.Where(x => Comparer(x, input)).Take(PageSize).ElementAt(index);
        }

        public Task<T> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    
    internal class ClearRowRederable : IRenderable
    {
        private readonly int _row;
        private readonly int _currentIndex;
        private readonly int _length;

        public ClearRowRederable(int row, int currentIndex)
        {
            _row = row;
            _currentIndex = currentIndex;
        }

        public Measurement Measure(RenderContext context, int maxWidth)
        {
            return new Measurement(0, 0);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            var newIndex = _row + 1;

            yield return Segment.Control(AnsiSequences.CUP(newIndex, 0));
            yield return Segment.Control(AnsiSequences.EL(0));
        }
    }

    internal class SelectedItemRenderable : IRenderable
    {
        private readonly string _text;

        public SelectedItemRenderable(string text)
        {
            _text = text;
        }

        public Measurement Measure(RenderContext context, int maxWidth)
        {
            return new Measurement(0, 0);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            yield return new Segment(_text, new Style(foreground: Color.Yellow2));
        }
    }

    internal class ListRenderable : IRenderable
    {
        private readonly IEnumerable<string> _elems;
        private readonly int _index;

        public ListRenderable(IEnumerable<string> elems, int index)
        {
            _elems = elems;
            _index = index;
        }

        public Measurement Measure(RenderContext context, int maxWidth)
        {
            return new Measurement(0, 0);
        }

        public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            for (int i = 0; i < _elems.Count(); i++)
            {
                var elem = _elems.ElementAt(i);
                var hasStyle = i == _index;

                if (hasStyle)
                {
                    yield return new Segment(elem, new Style(foreground: Color.Yellow2));
                }
                else
                {
                    yield return new Segment(elem);
                }


                yield return Segment.LineBreak;
            }
        }
    }
}