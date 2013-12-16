namespace VanessaSharp.Data.UnitTests
{
    /// <summary>Мокововый класс состояния для обхода проблем с Moq.</summary>
    internal abstract class MockConnectionState 
        : OneSConnection.StateObject
    {
        protected MockConnectionState()
            : base(null)
        {}
            
        // Запечатывание метода, чтобы не было проблем с работой Moq
        protected sealed override void InternalDisposed()
        {
            base.InternalDisposed();
        }
    }
}