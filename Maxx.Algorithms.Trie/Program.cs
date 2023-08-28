using System.Diagnostics;
using System.Text;

using RestSharp;

namespace Maxx.Algorithms.Trie;

internal class Program
{
    private static Trie _trie = new();

    static async Task Main(string[] args)
    {
        RestClient client = new(new RestClientOptions("https://www.mit.edu"));
        RestResponse response = await client.ExecuteAsync(new ("~ecprice/wordlist.10000"));
        IEnumerable<string> words = response.Content.Split('\n') ?? throw new("No words received from source");

        foreach (var word in words)
        {
            _trie.Insert(word);
        }

        SearchWordStartingWith("ab");
        SearchWordStartingWith("acc");
        SearchWordStartingWith("zu");
    }

    private static void SearchWordStartingWith(string prefix)
    {
        Console.WriteLine($"Words starting with '{prefix}':");
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var wordsThatStartWith = _trie.StartsWith("ab");
        foreach (var word in wordsThatStartWith)
        {
            Console.WriteLine(word);
        }
        stopWatch.Stop();
        Console.WriteLine($"Search completed in:{stopWatch.ElapsedMilliseconds} ms");
    }
}


public class Trie
{
    private TrieNode Root { get; }

    public Trie()
    {
        Root = new();
    }

    private class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; set; }
        public bool IsEndOfWord { get; set; }

        public TrieNode()
        {
            Children = new();
            IsEndOfWord = false;
        }
    }

    public void Insert(string word)
    {
        var current = Root;
        foreach (var ch in word)
        {
            if (!current.Children.ContainsKey(ch))
            {
                current.Children.Add(ch, new());
            }
            current = current.Children[ch];
        }
        current.IsEndOfWord = true;
    }

    public void Delete(string word)
    {
        Delete(Root, word, 0);
    }

    private bool Delete(TrieNode current, string word, int index)
    {
        if (index == word.Length)
        {
            if (!current.IsEndOfWord)
            {
                return false;
            }
            current.IsEndOfWord = false;
            return current.Children.Count == 0;
        }
        if (!current.Children.TryGetValue(word[index], out var node))
        {
            return false;
        }
        var shouldDeleteCurrentNode = Delete(node, word, index + 1) && !node.IsEndOfWord;
        if (shouldDeleteCurrentNode)
        {
            current.Children.Remove(word[index]);
            return current.Children.Count == 0;
        }
        return false;
    }

    public bool Search(string word)
    {
        var current = Root;
        foreach (var ch in word)
        {
            if (!current.Children.TryGetValue(ch, out var node))
            {
                return false;
            }
            current = node;
        }

        return current.IsEndOfWord;
    }

    public List<string> StartsWith(string prefix)
    {
        List<string> result = new();

        var current = Root;
        foreach (var ch in prefix)
        {
            if (!current.Children.TryGetValue(ch, out var node))
            {
                return result;
            }
            current = node;
        }
    
        StringBuilder sbPrefix = new(prefix);
        foreach (var pair in current.Children)
        {
            CreateStrings(sbPrefix.Append(pair.Key), pair, result);
            sbPrefix.Remove(sbPrefix.Length - 1, 1);
        }

        return result;
    }

    private void CreateStrings(StringBuilder prefix, KeyValuePair<char, TrieNode> pair, List<string> result)
    {
        if (pair.Value.Children.Count == 0)
        {
            result.Add(prefix.ToString());
            return;
        }

        foreach (var child in pair.Value.Children)
        {
            CreateStrings(prefix.Append(child.Key), child, result);
            prefix.Remove(prefix.Length - 1, 1);
        }
    }
}
