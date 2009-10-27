using System;
using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тестирование внутреннего состояния <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class InternalConnectionTests : InternalTestBase
    {
        /// <summary>Тестирование блокирования контекста в открытом состоянии.</summary>
        [Test(Description="Тестирование блокирования контекста в открытом состоянии.")]
        public void TestOpenedLockContext()
        {
            var context = Connection.LockContext();
            Assert.IsNotNull(context);
            
            context.Unlock();
        }

        /// <summary>Тестирование блокирования контекста в транзакции.</summary>
        [Test(Description = "Тестирование блокирования контекста в транзакции.")]
        public void TestLockContextInTx()
        {
            var tx = Connection.BeginTransaction();
            try
            {
                var context = Connection.LockContext();
                Assert.IsNotNull(context);

                context.Unlock();
            }
            finally
            {
                tx.Rollback();
            }
        }

        /// <summary>Тестирование блокирования контекста в закрытом состоянии.</summary>
        [Test(Description = "Тестирование блокирования контекста в закрытом состоянии.")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestClosedLockContext()
        {
            Connection.Close();
            Connection.LockContext();
        }

        /// <summary>Тестирование невозможности повторного блокирования.</summary>
        [Test(Description = "Тестирование невозможности повторного блокирования.")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestAlreadyLockedLockContext()
        {
            var context = Connection.LockContext();
            try
            {
                Connection.LockContext();
            }
            finally
            {
                context.Unlock();
            }
        }

        /// <summary>Тестирование возможности закрытия при блокировке.</summary>
        [Test(Description = "Тестирование возможности закрытия при блокировке.")]
        public void TestCloseLockedContext()
        {
            var context = Connection.LockContext();
            try
            {
                Connection.Close();
            }
            finally
            {
                context.Unlock();
            }
        }

        /// <summary>Тестирование возможности отката транзакции при блокировке.</summary>
        [Test(Description = "Тестирование возможности отката транзакции при блокировке.")]
        public void TestRollbackTxLockedContext()
        {
            var tx = Connection.BeginTransaction();
            var context = Connection.LockContext();
            try
            {
                tx.Rollback();
            }
            finally
            {
                context.Unlock();
            }
        }

        /// <summary>Тестирование невозможности начала транзакции при блокировке.</summary>
        [Test(Description = "Тестирование невозможности начала транзакции при блокировке.")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestBeginTxLockedContext()
        {
            var context = Connection.LockContext();
            try
            {
                Connection.BeginTransaction();
            }
            finally
            {
                context.Unlock();
            }
        }

        /// <summary>Тестирование невозможности фиксирования транзакции при блокировке.</summary>
        [Test(Description = "Тестирование невозможности фиксирования транзакции при блокировке.")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestCommitTxLockedContext()
        {
            var tx = Connection.BeginTransaction();
            var context = Connection.LockContext();
            try
            {
                tx.Commit();
                Connection.BeginTransaction();
            }
            finally
            {
                context.Unlock();
                try
                {
                    tx.Rollback();
                }
                catch (Exception) 
                {
                    Console.WriteLine("Ошибка отката транзакции");
                }
            }
        }

        /// <summary>Тестирование множественного блокирования контекста.</summary>
        /// <param name="count">Количество парных вызовов.</param>
        [Test(Description="Тестирование множественного блокирования контекста.")]
        public void TestMultiplyLockContext([Random(2, 10, 3)] int count)
        {
            for (var counter = 0; counter < count; counter++)
            {
                var context = Connection.LockContext();
                context.Unlock();
            }
        }
    }
}
