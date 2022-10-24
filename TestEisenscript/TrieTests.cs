using Eisenscript.Data_Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestEisenscript
{
    [TestClass]
    public class TrieTests
    {
        [TestMethod]
        public void TestTrie()
        {
            var trie = new Trie<int>();
            trie.Insert("bar", 0);
            trie.Insert("bare", 1);
            trie.Insert("bake", 2);
            trie.Insert("cake", 3);
            trie.Insert("barabbas", 4);

            var (val, stop, found) = trie.Search('m');
            Assert.IsTrue(stop);
            trie.ResetSearch();
            (val, stop, found) = trie.Search('b');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('a');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('r');
            Assert.IsTrue(!stop && found && val == 0);
            (val, stop, found) = trie.Search('e');
            Assert.IsTrue(!stop && found && val == 1);
            (val, stop, found) = trie.Search('m');
            Assert.IsTrue(stop);
            trie.ResetSearch();
            (val, stop, found) = trie.Search('b');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('a');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('k');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('e');
            Assert.IsTrue(!stop && found && val == 2);
            trie.ResetSearch();
            (val, stop, found) = trie.Search('b');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('a');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('r');
            Assert.IsTrue(!stop && found && val == 0);
            (val, stop, found) = trie.Search('a');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('b');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('b');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('a');
            Assert.IsTrue(!stop && !found);
            (val, stop, found) = trie.Search('s');
            Assert.IsTrue(!stop && found && val == 4);
        }
    }
}
