namespace Eisenscript
{
    internal class ParserException : Exception
    {
        internal int ILine
        {
            get;
            private set;
        }

        internal ParserException(string msg, int iLine) : base(msg)
        {
            ILine = iLine;
        }
    }
}
