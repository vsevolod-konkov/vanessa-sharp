using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тесты класса <see cref="OneSTransaction"/>.</summary>
    [TestFixture]
    public sealed class TransactionTests : TestsBase
    {
        /// <summary>Тестирование метода <see cref="OneSConnection.BeginTransaction()"/>.</summary>
        [Test(Description = "Тестирование метода BeginTransaction")]
        public void TestBeginTransaction()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                Assert.IsNull(connection.CurrentTransaction);

                Assert.Throws<InvalidOperationException>(() =>
                {
                    var transaction = connection.BeginTransaction();
                });

                connection.Open();

                {
                    var transaction2 = connection.BeginTransaction();
                    Assert.IsNotNull(transaction2);
                    Assert.AreEqual(IsolationLevel.Unspecified, transaction2.IsolationLevel);
                    Assert.IsNotNull(connection.CurrentTransaction);
                    Assert.AreSame(transaction2, connection.CurrentTransaction);
                    transaction2.Rollback();
                    Assert.IsNull(connection.CurrentTransaction);
                }

                {
                    connection.BeginTransaction();
                    Assert.IsNotNull(connection.CurrentTransaction);
                    connection.Close();
                    Assert.IsNull(connection.CurrentTransaction);
                }

                {
                    connection.Open();
                    var transaction2 = connection.BeginTransaction();
                    transaction2.Commit();
                }
            }
        }
    }
}
