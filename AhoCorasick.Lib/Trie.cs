using System;
using System.Net;

namespace AhoCorasick.Lib;

public class Trie
{
    public List<Node> Vertices;

    internal Trie(List<Node> vertices)
    {
        Vertices = vertices;
    }

    public Node Root => Vertices[0];
};

public class TrieBuilder
{
    public List<Node> Vertices = [new()];
    public Node Root => Vertices[0];
    private bool alreadyConstructed = false;

    public void AddString(string str)
    {
        if (alreadyConstructed)
        {
            throw new InvalidOperationException("Can't add strings to already built triebuilder");
        }
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        Node current = Root;

        foreach (char character in str)
        {
            if (current.Children.TryGetValue(character, out var linkedEdge))
            {
                current = linkedEdge;
                continue;
            }

            var justAdded = new Node(current, character);
            Vertices.Add(justAdded);

            current.Children[character] = justAdded;
            current = justAdded;
        }

        current.Outputs.Add(str);
    }

    public Trie Build()
    {
        Root.Fail = Root;

        Queue<Node> queue = new();

        foreach (var (_, child) in Root.Children)
        {
            child.Fail = Root;
            queue.Enqueue(child);
        }

        while (queue.TryDequeue(out var vert))
        {
            foreach (var (key, child) in vert.Children)
            {
                queue.Enqueue(child);
                var link = vert.Fail!;
                while (!link.HasChild(key) && link != Root)
                {
                    link = link.Fail!;
                }

                var childFailLink = link.Children.GetValueOrDefault(key) ?? Root;
                child.Fail = childFailLink;
                child.CopyOutputs(childFailLink);
            }
        }

        return new Trie(Vertices);
    }
}

public class Node
{
    public Dictionary<char, Node> Children = [];
    public HashSet<string> Outputs = [];
    public Node? Fail = null;
    public Node? Parent = null;
    public char Character = '$';

    public Node()
        : this(null, '$') { }

    public Node(Node? parent, char character)
    {
        Parent = parent;
        Character = character;
    }

    public bool HasChild(char key) => Children.ContainsKey(key);

    public void CopyOutputs(Node other)
    {
        foreach (var o in other.Outputs)
        {
            Outputs.Add(o);
        }
    }
}
