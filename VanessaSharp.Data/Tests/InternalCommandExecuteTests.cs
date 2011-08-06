using NUnit.Framework;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тесты выполнения запросов.</summary>
    [TestFixture]
    public sealed class InternalCommandExecuteTests : InternalTestBase
    {
        /// <summary>Тестирование выполнения запроса.</summary>
        [Test(Description="Тестирование выполнения запроса.")]
        public void TestCommandExecute()
        {
            // Формирование тестового агента глобального контекста.
            var testGlobalContext = new TestGlobalContext();
            
            Connection.LockContextDelegate = () => testGlobalContext;

            using (var command = new OneSCommand(Connection))
            {
                const string sql = "ВЫБРАТЬ Справочник.Валюты.Код КАК Код, Справочник.Валюты.Наименование КАК Наименование ИЗ Справочник.Валюты";

                // Выполнение запроса
                command.CommandText = sql;
                command.ExecuteReader();

                // Проверки
                
                // Глобальный контекст должен быть разблокирован
                Assert.IsFalse(testGlobalContext.Locked);
                
                // Нужный запрос
                Assert.AreEqual(sql, testGlobalContext.Query.Text);

                // Запрос был выполнен один раз
                Assert.AreEqual(1, testGlobalContext.Query.CallExecuteCount);
            }
        }

        #region Тестовые агенты

        /// <summary>Тестовая реализация интерфейса <see cref="IGlobalContext"/>.</summary>
        private sealed class TestGlobalContext : IGlobalContext
        {
            /// <summary>Конструктор.</summary>
            public TestGlobalContext()
            {
                Locked = true;
                Query = new TestQuery();
            }

            /// <summary>Блокирован ли доступ.</summary>
            public bool Locked { get; private set; }

            /// <summary>Возвращение контекста подключению.</summary>
            void IGlobalContext.Unlock()
            {
                Locked = false;
            }

            /// <summary>Создание объекта запроса.</summary>
            IQuery IGlobalContext.CreateQuery()
            {
                return Query;
            }

            /// <summary>Тестовый объект запроса.</summary>
            public TestQuery Query
            {
                get;
                private set;
            }
        }

        /// <summary>Тестовая реализация интерфейса <see cref="IQuery"/>.</summary>
        private sealed class TestQuery : IQuery
        {
            /// <summary>Текст запроса.</summary>
            string IQuery.Text
            {
                get { return Text; }
                set { Text = value; }
            }
            
            /// <summary>Текст запроса.</summary>
            public string Text
            {
                get;
                private set;
            }

            /// <summary>Выполнение запроса.</summary>
            void IQuery.Execute()
            {
                CallExecuteCount = CallExecuteCount + 1;
            }

            /// <summary>Количество вызовов метода <see cref="Execute"/>.</summary>
            public int CallExecuteCount
            {
                get;
                private set;
            }
        }

        #endregion
    }
}
