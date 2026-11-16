using System;

namespace AhoCorasick.Lib;

public class StringMatcher(Trie trie)
{
    public Match[] GetMatches(string text)
    {
        List<Match> found = [];

        var state = trie.Root;
        int i = 0;
        while (i < text.Length)
        {
            var character = text[i];
            if (state.HasChild(character))
            {
                state = state.Children[character];
                i++;
                if (state.Outputs.Count > 0)
                {
                    var outputs = state.Outputs.Select(s => new Match()
                    {
                        Position = i - s.Length,
                        Value = s,
                    });
                    found.AddRange(outputs);
                }
            }
            else if(state == trie.Root)
            {
                i++;
            }
            else
            {
                state = state.Fail;
            }
        }

        return found.ToArray();
    }
}

public struct Match
{
    public int Position;
    public string Value;
}
