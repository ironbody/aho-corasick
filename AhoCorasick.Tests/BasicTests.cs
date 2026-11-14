using AhoCorasick.Lib;

namespace AhoCorasick.Tests;

public class TrieTests
{
    [Test]
    public async Task NewVertex()
    {
        Trie.Vertex vertex = new();

        await Assert.That(vertex.Next.Length).EqualTo(Trie.AlphabetSize);
        await Assert.That(vertex.Next).ContainsOnly(x => x == -1);
    }
}
