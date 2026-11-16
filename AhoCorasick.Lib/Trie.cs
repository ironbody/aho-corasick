using System;

namespace AhoCorasick.Lib;

public class Trie
{
    public const int AlphabetSize = 26;

    public List<Vertex> Vertices = [new Vertex()];

    public void AddString(string str)
    {
        str = str.ToLowerInvariant();

        int current = 0;

        foreach (char ch in str)
        {
            int c = ch - 'a';

            var linkedEdge = Vertices[current].Next[c];

            if (linkedEdge != -1)
            {
                current = linkedEdge;
                continue;
            }

            Vertices.Add(new Vertex());

            var justAdded = Vertices.Count-1;

            Vertices[current].Next[c] = justAdded;
            current = justAdded;
        }

        var outputVertex = Vertices[current];
        outputVertex.Output = true;
        Vertices[current] = outputVertex;
    }

    public struct Vertex
    {
        public int[] Next = new int[AlphabetSize];
        public bool Output = false;

        public Vertex()
        {
            Array.Fill(Next, -1);
        }
    }
}
