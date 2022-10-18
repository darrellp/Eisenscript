namespace Eisenscript.Data_Structures
{
    internal class TrieNode<T>
    {
        #region Private Variables
        private readonly Dictionary<char, TrieNode<T>> _children = new();
        private bool _finished;
        private T? _value;
        #endregion

        #region Properties
        public T? Value => _value;
        public bool Finished => _finished;
        #endregion

        #region Overloads
        internal TrieNode<T>? this[char ch]
        {
            get => _children.ContainsKey(ch) ? _children[ch] : null;

            set
            {
                if (value != null)
                {
                    _children[ch] = value;
                }
            }
        }
        #endregion

        #region Operations
        internal void Insert(string word, T value)
        {
            Insert(word, 0, value);
        }

        private void Insert(string word, int index, T value)
        {
            if (index == word.Length)
            {
                _finished = true;
                _value = value;
                return;
            }

            var ch = word[index];
            var newNode = this[ch] ?? (this[ch] = new TrieNode<T>());

            newNode.Insert(word, ++index, value);
        }
        #endregion
    }
}
