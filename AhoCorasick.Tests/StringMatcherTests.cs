using AhoCorasick.Lib;
// TUnit.Assertions also has a StringMatcher type.
using StringMatcher = AhoCorasick.Lib.StringMatcher;

namespace AhoCorasick.Tests;

public class StringMatcherTests
{
    private static (string Value, int Position)[] Find(string text, params string[] patterns)
    {
        var builder = new TrieBuilder();
        foreach (var p in patterns)
        {
            builder.AddString(p);
        }
        var matcher = new StringMatcher(builder.Build());

        return matcher.GetMatches(text).Select(m => (m.Value, m.Position)).ToArray();
    }

    [Test]
    public async Task ClassicExample_FindsAllOverlappingMatches()
    {
        var matches = Find("ushers", "he", "she", "his", "hers");

        await Assert.That(matches).IsEquivalentTo([("she", 1), ("he", 2), ("hers", 2)]);
    }

    [Test]
    public async Task Sentence_FindsWordsAndSubwords()
    {
        var matches = Find("let's eat meat", "eat", "meat");

        await Assert.That(matches).IsEquivalentTo([("eat", 6), ("meat", 10), ("eat", 11)]);
    }

    [Test]
    public async Task SamePatternOverlappingItself_ReportsEveryOccurrence()
    {
        var matches = Find("aaaa", "aa");

        await Assert.That(matches).IsEquivalentTo([("aa", 0), ("aa", 1), ("aa", 2)]);
    }

    [Test]
    public async Task PatternEqualsWholeText()
    {
        var matches = Find("needle", "needle");

        await Assert.That(matches).IsEquivalentTo([("needle", 0)]);
    }

    [Test]
    public async Task MatchAtEndOfText()
    {
        var matches = Find("xxxend", "end");

        await Assert.That(matches).IsEquivalentTo([("end", 3)]);
    }

    [Test]
    public async Task MismatchAfterPartialMatch_RecoversThroughFailLink()
    {
        // Reading "abc" goes down the "abcd" branch; on 'e' the matcher must
        // fall back to "bc" and continue to find "bce".
        var matches = Find("abce", "abcd", "bce");

        await Assert.That(matches).IsEquivalentTo([("bce", 1)]);
    }

    [Test]
    public async Task MismatchFallsBackToRoot_ThenMatchesLater()
    {
        var matches = Find("abxab", "abc", "ab");

        await Assert.That(matches).IsEquivalentTo([("ab", 0), ("ab", 3)]);
    }

    [Test]
    public async Task PatternNestedInsideAnother_BothReported()
    {
        var matches = Find("abcde", "abcde", "bcd", "c");

        await Assert.That(matches).IsEquivalentTo([("c", 2), ("bcd", 1), ("abcde", 0)]);
    }

    [Test]
    public async Task NoMatches_ReturnsEmpty()
    {
        var matches = Find("hello world", "xyz", "abc");

        await Assert.That(matches).IsEmpty();
    }

    [Test]
    public async Task EmptyText_ReturnsEmpty()
    {
        var matches = Find("", "abc");

        await Assert.That(matches).IsEmpty();
    }

    [Test]
    public async Task NoPatterns_ReturnsEmpty()
    {
        var matches = Find("anything at all");

        await Assert.That(matches).IsEmpty();
    }

    [Test]
    public async Task IsCaseSensitive()
    {
        var matches = Find("EAT eat", "eat");

        await Assert.That(matches).IsEquivalentTo([("eat", 4)]);
    }

    [Test]
    public async Task NonLetterCharacters_AreMatchedLiterally()
    {
        var matches = Find("a-b a b a.b", "a b", "a.b");

        await Assert.That(matches).IsEquivalentTo([("a b", 4), ("a.b", 8)]);
    }

    [Test]
    public async Task MatcherCanBeReused()
    {
        var builder = new TrieBuilder();
        builder.AddString("ab");
        var matcher = new StringMatcher(builder.Build());

        var first = matcher.GetMatches("ab").Length;
        var second = matcher.GetMatches("abab").Length;

        await Assert.That(first).IsEqualTo(1);
        await Assert.That(second).IsEqualTo(2);
    }
}
