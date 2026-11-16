using AhoCorasick.Lib;

Trie trie = new();

trie.AddString("ABC");
trie.AddString("AEF");

Console.WriteLine(trie.Print());
