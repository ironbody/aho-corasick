using AhoCorasick.Lib;

TrieBuilder tb = new();

// trie.AddString("the");
// trie.AddString("fox");
// trie.AddString("dog");
tb.AddString("");
tb.AddString("eat");
tb.AddString("meat");
var trie = tb.Build();

StringMatcher sm = new(trie);

var matches = sm.GetMatches("let's eat meat");


Console.WriteLine(trie.Print());
File.WriteAllText("/tmp/trie.d2", trie.Print());
