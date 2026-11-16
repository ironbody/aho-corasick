using System;
using System.Text;

namespace AhoCorasick.Lib;

/// <summary>Renders a trie as a D2 diagram (https://d2lang.com).</summary>
public static class TriePrinter
{
    public static string Print(this Trie trie, bool includeFailLinks = true)
    {
        // Nodes don't carry an id, so use each node's position in Vertices.
        var ids = new Dictionary<Node, int>(ReferenceEqualityComparer.Instance);
        foreach (var (idx, node) in trie.Vertices.Index())
        {
            ids[node] = idx;
        }

        string Ref(Node? node) => node is null ? "null" : $"#{ids[node]}";

        var sb = new StringBuilder();

        // Node declarations: each node is a D2 "class" shape, one row per property.
        foreach (var (idx, node) in trie.Vertices.Index())
        {
            var title = node == trie.Root ? $"#{idx} root" : $"#{idx} '{node.Character}'";
            var outputs = node.Outputs.Count > 0 ? string.Join(", ", node.Outputs) : "∅";
            var children = node.Children.Count > 0
                ? string.Join(" ", node.Children.Keys)
                : "∅";

            sb.AppendLine($"{idx}: {{");
            sb.AppendLine("  shape: class");
            sb.AppendLine($"  label: {Quote(title)}");
            sb.AppendLine($"  character: {Quote(node.Character.ToString())}");
            sb.AppendLine($"  parent: {Quote(Ref(node.Parent))}");
            sb.AppendLine($"  fail: {Quote(Ref(node.Fail))}");
            sb.AppendLine($"  children: {Quote(children)}");
            sb.AppendLine($"  outputs: {Quote(outputs)}");
            if (node.Outputs.Count > 0)
            {
                // Highlight nodes that report a match (fill = header colour for class shapes)
                sb.AppendLine("  style.fill: '#1b7a3a'");
            }
            sb.AppendLine("}");
        }

        // Trie edges
        foreach (var (idx, node) in trie.Vertices.Index())
        {
            foreach (var (letter, child) in node.Children)
            {
                sb.AppendLine($"{idx} -> {ids[child]}: {Quote(letter.ToString())}");
            }
        }

        // Failure links (only set after Build); skip links to root to keep the
        // diagram readable. Every node's fail target is still listed in its table.
        if (includeFailLinks)
        {
            foreach (var (idx, node) in trie.Vertices.Index())
            {
                if (node.Fail is null || node.Fail == trie.Root)
                {
                    continue;
                }

                sb.AppendLine(
                    $"{idx} -> {ids[node.Fail]}: {{style.stroke-dash: 3; style.stroke: red}}"
                );
            }
        }

        return sb.ToString();
    }

    // Single-quoted D2 strings are literal: no $-substitutions. A quote is escaped by doubling it.
    private static string Quote(string s) => "'" + s.Replace("'", "''") + "'";
}
