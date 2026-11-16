using AhoCorasick.Lib;

namespace AhoCorasick.Tests;

public class TrieTests
{
    Trie trie = new();

    [Test]
    public async Task NewVertex()
    {
        Trie.Vertex vertex = new();

        await Assert.That(vertex.Next.Length).EqualTo(Trie.AlphabetSize);
        await Assert.That(vertex.Next).ContainsOnly(x => x == -1);
    }

    [Test]
    [Arguments("a", 0)]
    [Arguments("b", 1)]
    [Arguments("z", 25)]
    public async Task AddString_OneCharacter_ConnectsEdgeCorrect(string input, int expectedIndex)
    {
        trie.AddString(input);

        // Adds another vertex
        await Assert.That(trie.Vertices.Count).IsEqualTo(2);

        // and connects the node
        await Assert.That(trie.Vertices[0].Next[expectedIndex]).IsEqualTo(1);
    }

    [Test]
    public async Task AddString_MultiCharacter_AAA()
    {
        trie.AddString("AAA");

        await Assert.That(trie.Vertices.Count).IsEqualTo(4);
        await Assert.That(trie.Vertices[0].Next[0]).IsEqualTo(1);
        await Assert.That(trie.Vertices[1].Next[0]).IsEqualTo(2);
        await Assert.That(trie.Vertices[2].Next[0]).IsEqualTo(3);
    }

    [Test]
    public async Task AddString_MultiCharacter_ABC()
    {
        trie.AddString("ABC");

        await Assert.That(trie.Vertices.Count).IsEqualTo(4);
        await Assert.That(trie.Vertices[0].Next[0]).IsEqualTo(1);
        await Assert.That(trie.Vertices[1].Next[1]).IsEqualTo(2);
        await Assert.That(trie.Vertices[2].Next[2]).IsEqualTo(3);
    }

    [Test]
    public async Task AddString_AddTwoString_ABC_DEF()
    {
        trie.AddString("ABC");
        trie.AddString("DEF");

        await Assert.That(trie.Vertices.Count).IsEqualTo(7);
        await Assert.That(trie.Vertices[0].Next[0]).IsEqualTo(1);
        await Assert.That(trie.Vertices[1].Next[1]).IsEqualTo(2);
        await Assert.That(trie.Vertices[2].Next[2]).IsEqualTo(3);

        await Assert.That(trie.Vertices[0].Next[3]).IsEqualTo(4);
        await Assert.That(trie.Vertices[4].Next[4]).IsEqualTo(5);
        await Assert.That(trie.Vertices[5].Next[5]).IsEqualTo(6);
    }

    [Test]
    public async Task AddString_AddTwoString_ABC_AEF()
    {
        trie.AddString("ABC");
        trie.AddString("AEF");

        await Assert.That(trie.Vertices.Count).IsEqualTo(6);
        await Assert.That(trie.Vertices[0].Next[0]).IsEqualTo(1);
        await Assert.That(trie.Vertices[1].Next[1]).IsEqualTo(2);
        await Assert.That(trie.Vertices[2].Next[2]).IsEqualTo(3);

        await Assert.That(trie.Vertices[0].Next[0]).IsEqualTo(1);
        await Assert.That(trie.Vertices[1].Next[4]).IsEqualTo(4);
        await Assert.That(trie.Vertices[4].Next[5]).IsEqualTo(5);
    }

}
