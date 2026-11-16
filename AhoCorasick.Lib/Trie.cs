using System;
using System.Net;

namespace AhoCorasick.Lib;

public class Trie
{
    public const int AlphabetSize = 26;

    public List<Vertex> Vertices = [new Vertex()];

    public void AddString(string str)
    {
        str = str.ToLowerInvariant();

        int current = 0;

        foreach (char character in str)
        {
            int letterIdx = character - 'a';

            var linkedEdge = Vertices[current].Children[letterIdx];

            if (linkedEdge != -1)
            {
                current = linkedEdge;
                continue;
            }

            Vertices.Add(new Vertex(current, character));

            var justAdded = Vertices.Count - 1;

            Vertices[current].Children[letterIdx] = justAdded;
            current = justAdded;
        }

        var outputVertex = Vertices[current];
        outputVertex.Output = true;
        Vertices[current] = outputVertex;
    }

    public void Build()
    {
        Vertices[0] = Vertices[0] with {Fail = 0};
        var root = Vertices[0];

        Queue<int> queue = new();

        foreach (var child in root.Children.Where(c => c != -1))
        {
            Vertices[child] = Vertices[child] with { Fail = 0 };
            queue.Enqueue(child);
        }

        while (queue.TryDequeue(out int node))
        {
            var vert = Vertices[node];
            foreach (var (idx, child) in vert.Children.Index().Where((c) => c.Item != -1))
            {
                queue.Enqueue(child);
                var link = vert.Fail;
                while (!Vertices[link].HasChild(idx) && link != 0)
                {
                    link = Vertices[link].Fail;
                }
                var childFailLink =
                    Vertices[link].Children[idx] != -1 ? Vertices[link].Children[idx] : 0;
                var childOutput = Vertices[childFailLink].Output;
                Vertices[child] = Vertices[child] with
                {
                    Fail = childFailLink,
                    Output = childOutput,
                };
            }
        }
    }

    public struct Vertex
    {
        public int[] Children = new int[AlphabetSize];
        public bool Output = false;
        public int Fail = -1;
        public int Parent = -1;
        public char Character = '$';

        public Vertex()
            : this(-1, '$') { }

        public Vertex(int parent, char character)
        {
            Parent = parent;
            Character = character;
            Array.Fill(Children, -1);
        }

        public readonly bool HasChild(int idx) => Children[idx] != -1;
    }
}
