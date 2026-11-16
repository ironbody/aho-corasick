using AhoCorasick.Lib;

Trie trie = new();

trie.AddString("ABCDEFG");
trie.AddString("ZXCABCWER");


Console.WriteLine(trie.Print());
