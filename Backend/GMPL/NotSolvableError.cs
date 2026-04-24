namespace Backend.GMPL
{
    public class NotSolvableError : Exception
    {
        public NotSolvableError(string  message) : base(message) { }
    }
}
