namespace Eisenscript.Data_Structures
{
    public class Trie<T>
    {
        #region Private variables
        private readonly TrieNode<T> _root;
        private TrieNode<T> _curSearchNode;
        #endregion

        #region Constructor
#pragma warning disable CS8618
        public Trie()
        {
            _root = new TrieNode<T>();
            ResetSearch();
        }
#pragma warning restore CS8618
        #endregion

        #region Operations
        public void Insert(string word, T value)
        {
            _root.Insert(word, value);
        }

        public (T? value, bool fPathEnd, bool fFound) Search(char ch)
        {
            var nextNode = _curSearchNode[ch];
            if (nextNode == null)
            {
                return (default(T), true, false);
            }

            _curSearchNode = nextNode;
            if (nextNode.Finished)
            {
                return (nextNode.Value, false, true);
            }

            return (default(T), false, false);
        }

        public void ResetSearch()
        {
            _curSearchNode = _root;
        }
        #endregion
    }
}
