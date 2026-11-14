using System;

namespace AhoCorasick.Lib;

public class Trie
{
    public const int AlphabetSize = 26;

    public List<Vertex> Vertices = [new Vertex()];

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
