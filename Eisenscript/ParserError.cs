namespace Eisenscript
{
    internal class ParserException : Exception
    {
        internal int Line
        {
            get;
        }

        internal ParserException(string msg, int line) : base(msg)
        {
            Line = line;
        }
    }
}
