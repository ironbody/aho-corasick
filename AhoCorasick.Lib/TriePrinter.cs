using System;
using System.Text;

namespace AhoCorasick.Lib;

public static class TriePrinter
{
    public static string Print(this Trie trie)
    {
        var sb = new StringBuilder();

        sb.AppendLine("0:root");

        foreach (var (idx, vert) in trie.Vertices.Index())
        {
            for (var i = 0; i < vert.Next.Length; i++)
            {
                if (vert.Next[i] == -1)
                {
                    continue;
                }

                var letter = Convert.ToChar('a' + i);
                var vertex = vert.Next[i];
                sb.AppendLine($"{vertex}:{letter}");
                sb.AppendLine($"{idx}->{vertex}");
            }
        }

        return sb.ToString();
    }
}
