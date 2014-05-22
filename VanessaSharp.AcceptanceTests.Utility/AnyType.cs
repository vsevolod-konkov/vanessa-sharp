namespace VanessaSharp.AcceptanceTests.Utility
{
    /// <summary>
    /// Тип - маркер,
    /// указывающий, что нет необходимости проверять тип в выходном результате.
    /// </summary>
    public sealed class AnyType
    {
        public static AnyType Instance
        {
            get { return _instance; }
        }
        private static readonly AnyType _instance = new AnyType();
    }
}
