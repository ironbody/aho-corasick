using AhoCorasick.Lib;

namespace AhoCorasick.Tests;

public class TrieBuilderTests
{
    private readonly TrieBuilder builder = new();

    /// <summary>Follows the trie edges for <paramref name="path"/> starting at <paramref name="root"/>.</summary>
    private static Node Walk(Node root, string path)
    {
        var node = root;
        foreach (var c in path)
        {
            node = node.Children[c];
        }
        return node;
    }

    // ---------- AddString ----------

    [Test]
    public async Task NewBuilder_HasOnlyRoot()
    {
        await Assert.That(builder.Vertices.Count).IsEqualTo(1);
        await Assert.That(builder.Root.Children.Count).IsEqualTo(0);
        await Assert.That(builder.Root.Outputs.Count).IsEqualTo(0);
        await Assert.That(builder.Root.Parent).IsNull();
    }

    [Test]
    [Arguments("a")]
    [Arguments("z")]
    [Arguments("A")]
    [Arguments(" ")]
    public async Task AddString_OneCharacter_AddsChildOfRoot(string input)
    {
        builder.AddString(input);

        await Assert.That(builder.Vertices.Count).IsEqualTo(2);

        var child = builder.Root.Children[input[0]];
        await Assert.That(child).IsSameReferenceAs(builder.Vertices[1]);
        await Assert.That(child.Parent).IsSameReferenceAs(builder.Root);
        await Assert.That(child.Character).IsEqualTo(input[0]);
    }

    [Test]
    public async Task AddString_RepeatedLetters_BuildsChain()
    {
        builder.AddString("aaa");

        await Assert.That(builder.Vertices.Count).IsEqualTo(4);
        await Assert.That(builder.Root.Children['a']).IsSameReferenceAs(builder.Vertices[1]);
        await Assert.That(builder.Vertices[1].Children['a']).IsSameReferenceAs(builder.Vertices[2]);
        await Assert.That(builder.Vertices[2].Children['a']).IsSameReferenceAs(builder.Vertices[3]);
    }

    [Test]
    public async Task AddString_DisjointWords_CreateSeparateBranches()
    {
        builder.AddString("abc");
        builder.AddString("def");

        await Assert.That(builder.Vertices.Count).IsEqualTo(7);
        await Assert.That(builder.Root.Children.Keys).IsEquivalentTo(['a', 'd']);
    }

    [Test]
    public async Task AddString_SharedPrefix_ReusesNodes()
    {
        builder.AddString("abc");
        builder.AddString("aef");

        await Assert.That(builder.Vertices.Count).IsEqualTo(6);
        await Assert.That(builder.Root.Children.Keys).IsEquivalentTo(['a']);
        await Assert.That(Walk(builder.Root, "a").Children.Keys).IsEquivalentTo(['b', 'e']);
    }

    [Test]
    public async Task AddString_SameWordTwice_AddsNothingNew()
    {
        builder.AddString("abc");
        builder.AddString("abc");

        await Assert.That(builder.Vertices.Count).IsEqualTo(4);
        await Assert.That(Walk(builder.Root, "abc").Outputs).IsEquivalentTo(["abc"]);
    }

    [Test]
    public async Task AddString_PrefixOfExistingWord_MarksInnerNode()
    {
        builder.AddString("hers");
        builder.AddString("he");

        await Assert.That(builder.Vertices.Count).IsEqualTo(5);
        await Assert.That(Walk(builder.Root, "h").Outputs.Count).IsEqualTo(0);
        await Assert.That(Walk(builder.Root, "he").Outputs).IsEquivalentTo(["he"]);
        await Assert.That(Walk(builder.Root, "her").Outputs.Count).IsEqualTo(0);
        await Assert.That(Walk(builder.Root, "hers").Outputs).IsEquivalentTo(["hers"]);
    }

    [Test]
    public async Task AddString_IsCaseSensitive()
    {
        builder.AddString("a");
        builder.AddString("A");

        await Assert.That(builder.Vertices.Count).IsEqualTo(3);
        await Assert.That(builder.Root.Children.Keys).IsEquivalentTo(['a', 'A']);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task AddString_EmptyOrNull_IsIgnored(string? input)
    {
        builder.AddString(input!);

        await Assert.That(builder.Vertices.Count).IsEqualTo(1);
        await Assert.That(builder.Root.Outputs.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddString_AfterBuild_Throws()
    {
        builder.AddString("abc");
        builder.Build();

        await Assert.That(() => builder.AddString("def")).Throws<InvalidOperationException>();
    }

    // ---------- Build ----------

    [Test]
    public async Task Build_EmptyBuilder_ReturnsTrieWithOnlyRoot()
    {
        var trie = builder.Build();

        await Assert.That(trie.Vertices.Count).IsEqualTo(1);
        await Assert.That(trie.Root.Fail).IsSameReferenceAs(trie.Root);
    }

    [Test]
    public async Task Build_SetsEveryFailLink()
    {
        builder.AddString("abc");
        builder.AddString("bcd");
        builder.AddString("xyz");
        var trie = builder.Build();

        foreach (var node in trie.Vertices)
        {
            await Assert.That(node.Fail).IsNotNull();
        }
    }

    [Test]
    public async Task Build_RootAndDepthOneNodes_FailToRoot()
    {
        builder.AddString("ab");
        builder.AddString("cd");
        var trie = builder.Build();

        await Assert.That(trie.Root.Fail).IsSameReferenceAs(trie.Root);
        await Assert.That(Walk(trie.Root, "a").Fail).IsSameReferenceAs(trie.Root);
        await Assert.That(Walk(trie.Root, "c").Fail).IsSameReferenceAs(trie.Root);
    }

    // The classic example from the Aho–Corasick paper.
    [Test]
    [Arguments("he", "")]
    [Arguments("she", "he")]
    [Arguments("sh", "h")]
    [Arguments("his", "s")]
    [Arguments("hers", "s")]
    [Arguments("her", "")]
    public async Task Build_ClassicExample_FailLinkPointsToLongestSuffix(
        string nodePath,
        string expectedFailPath
    )
    {
        foreach (var w in new[] { "he", "she", "his", "hers" })
        {
            builder.AddString(w);
        }
        var trie = builder.Build();

        var node = Walk(trie.Root, nodePath);
        var expected = Walk(trie.Root, expectedFailPath);

        await Assert.That(node.Fail).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task Build_FailLinkFollowsChain_WhenDirectSuffixHasNoMatchingChild()
    {
        // At "abc", the fail of "ab" is "b", but "b" has no 'c' child,
        // so the search continues to root, which has a 'c' child.
        builder.AddString("abc");
        builder.AddString("bd");
        builder.AddString("c");
        var trie = builder.Build();

        await Assert.That(Walk(trie.Root, "ab").Fail).IsSameReferenceAs(Walk(trie.Root, "b"));
        await Assert.That(Walk(trie.Root, "abc").Fail).IsSameReferenceAs(Walk(trie.Root, "c"));
    }

    [Test]
    public async Task Build_MergesOutputsAlongFailChain()
    {
        // "she" -> "he" -> "e": every suffix that's a pattern must be reported at "she".
        builder.AddString("she");
        builder.AddString("he");
        builder.AddString("e");
        var trie = builder.Build();

        await Assert.That(Walk(trie.Root, "she").Outputs).IsEquivalentTo(["she", "he", "e"]);
        await Assert.That(Walk(trie.Root, "he").Outputs).IsEquivalentTo(["he", "e"]);
        await Assert.That(Walk(trie.Root, "sh").Outputs.Count).IsEqualTo(0);
    }
}
