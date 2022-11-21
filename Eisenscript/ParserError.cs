namespace Eisenscript
{
    public class ParserException : Exception
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
