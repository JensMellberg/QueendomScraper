using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace QueendomScraper
{
    public enum MoveResult
    {
        Found,
        Notified,
        Nonexistant,
    }

    public class StringTraverser
    {
        private int position;
        private string source;

        public StringTraverser(string source)
        {
            position = 0;
            this.source = source;
        }

        public MoveResult MoveUntil(string stop, bool movePast, string notifier = null)
        {
            while (true)
            {
                var next = source.Substring(position, stop.Length);
                if (source.Substring(position, stop.Length) == stop)
                {
                    position += movePast ? stop.Length : 0;
                    return MoveResult.Found;
                } 
                else if (notifier != null && source.Substring(position, notifier.Length) == notifier)
                {
                    return MoveResult.Notified;
                }
                position += 1;
                if (position + Math.Max(stop.Length, notifier?.Length ?? 0) >= source.Length)
                {
                    return MoveResult.Nonexistant;
                }
            }
        }

        public void Move(int steps)
        {
            position += steps;
        }

        public string ReadUntil(string stop)
        {
            var result = new StringBuilder();
            while (source.Substring(position, stop.Length) != stop)
            {
                result.Append(source[position]);
                position += 1;
                if (position + stop.Length >= source.Length)
                {
                    return null;
                }
            }

            return result.ToString();
        }
    }
}
