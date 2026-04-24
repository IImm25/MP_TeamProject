namespace Backend.GMPL
{
    public class ValidationError : Exception
    {
        public ValidationError(string msg) : base(msg) { }
    }
}
