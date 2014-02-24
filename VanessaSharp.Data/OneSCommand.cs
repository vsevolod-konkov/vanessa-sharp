using System;
using System.Data.Common;
using System.Data;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Команда запроса к 1С.</summary>
    public sealed class OneSCommand : DbCommand
    {
        // TODO: Нужен Рефакторинг. Убрать интерфейс после внедрения параметрических запросов. Нужна просто фабрика объектов.
        /// <summary>Поставщик глобального контекста 1С.</summary>
        private IGlobalContextProvider _globalContextProvider;

        /// <summary>Объект соединения.</summary>
        private OneSConnection _connection;

        /// <summary>Установка соединения.</summary>
        /// <param name="connection">Соединение</param>
        /// <param name="globalContextProvider">Поставщик глобального контекста.</param>
        private void SetConnection(OneSConnection connection, IGlobalContextProvider globalContextProvider)
        {
            _connection = connection;
            _globalContextProvider = globalContextProvider;
        }

        /// <summary>Установка соединения.</summary>
        /// <param name="connection">Соединение</param>
        private void SetConnection(OneSConnection connection)
        {
            SetConnection(connection, connection);
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(
                (_connection == null && _globalContextProvider == null)
                ||
                (_connection != null && _globalContextProvider != null)
                );
        }

        /// <summary>Конструктор принимающий поставщика глобального контекста.</summary>
        internal OneSCommand(IGlobalContextProvider globalContextProvider, OneSConnection connection)
        {
            SetConnection(connection, globalContextProvider);
        }
        
        /// <summary>Конструктор без аргументов.</summary>
        public OneSCommand() : this(null)
        {}

        /// <summary>Конструктор принимающий соединение.</summary>
        /// <param name="connection">Соединение к 1С.</param>
        public OneSCommand(OneSConnection connection) : this(connection, null)
        {}

        /// <summary>Конструктор принимающий соединение и строку запроса.</summary>
        /// <param name="connection">Соединение.</param>
        /// <param name="commandText">Строка запроса.</param>
        public OneSCommand(OneSConnection connection, string commandText)
        {
            Connection = connection;
            CommandText = commandText;
        }

        /// <summary>Пытается отменить выполнение запроса.</summary>
        /// <remarks>Не поддерживается 1С.</remarks>
        public override void Cancel()
        {
            throw new NotSupportedException("Метод отмены выполнения запроса (Cancel) не поддерживается 1С.");
        }

        /// <summary>Строка запроса данных к информационной базе 1С.</summary>
        public override string CommandText { get; set; }

        /// <summary>Получает или задает время ожидания перед завершением попытки выполнить команду и генерацией ошибки.</summary>
        /// <remarks>Не поддерживается 1С.</remarks>
        public override int CommandTimeout
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotSupportedException(
                    "Настройка времени ожидания выполнения команды не поддерживается 1С.");
            }
        }

        /// <summary>Получает или задает значение, указывающее, как будет интерпретироваться свойство <see cref="CommandText"/>.</summary>
        /// <remarks>В текущей версии поддерживается только <see cref="System.Data.CommandType.Text"/>.</remarks>
        public override CommandType CommandType
        {
            get
            {
                return CommandType.Text;
            }
            set
            {
                if (value != CommandType.Text)
                {
                    throw new NotSupportedException(string.Format(
                        "Значение типа команды отличное от {0} не поддерживается в текущей версии.",
                        CommandType.Text));
                }
            }
        }

        /// <summary>Создает экземпляр параметра запроса типа <see cref="OneSParameter"/>.</summary>
        public new OneSParameter CreateParameter()
        {
            throw new NotImplementedException();
        }

        /// <summary>Создает экземпляр параметра запроса.</summary>
        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        /// <summary>Получает и устанавливает соединение с информационной базой 1С.</summary>
        public new OneSConnection Connection
        {
            get { return _connection; }
            set { SetConnection(value);}
        }

        /// <summary>Получает и устанавливает соединение с информационной базой 1С.</summary>
        protected override DbConnection DbConnection
        {
            get
            {
                return Connection;
            }
            set
            {
                OneSConnection typedValue = null;

                if (value != null)
                {
                    typedValue = value as OneSConnection;
                    if (typedValue == null)
                    {
                        throw new ArgumentException(string.Format(
                            "Ожидалось значение подключения типа \"{0}\", а было значение \"{1}\" типа \"{2}\".", 
                            typeof(OneSConnection), value, value.GetType()), "value");
                    }
                }
                
                Connection = typedValue;
            }
        }

        /// <summary>Коллекция параметров запроса к информационной базе 1С.</summary>
        public new OneSParameterCollection Parameters
        {
            get
            {
                Contract.Ensures(Contract.Result<OneSParameterCollection>() != null);
                
                return _parameters;
            }
        }
        private readonly OneSParameterCollection _parameters = new OneSParameterCollection();

        /// <summary>Коллекция параметров запроса к информационной базе 1С.</summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        /// <summary>Текущая транзакция в которой будет выполняться запрос.</summary>
        public new OneSTransaction Transaction
        {
            get
            {
                return (Connection == null)
                    ? null
                    : Connection.CurrentTransaction;
            }
        }
        
        /// <summary>Получает и устанавливает транзакцию в которой будет выполняться запрос.</summary>
        /// <remarks>Установка транзакции отличной от текущей транзакции соединения не поддерживается.</remarks>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return Transaction;
            }
            set
            {
                throw new NotSupportedException(
                    "Установка транзакции для команды не поддерживается. Команда всегда выполняется в рамках текущей транзакции соединения.");
            }
        }

        /// <summary>
        /// Получает или задает значение, указывающее, будет ли объект команды видимым в элементе управления Windows Forms Designer.
        /// </summary>
        public override bool DesignTimeVisible { get; set; }

        /// <summary>Выполняет текст команды применительно к соединению к информационной базе 1С.</summary>
        /// <param name="behavior">Поведение выполнения команды, специфицируя описание результатов запроса и его воздействия на базу данных.</param>
        /// <returns>Читатель данных.</returns>
        public new OneSDataReader ExecuteReader(CommandBehavior behavior)
        {
            const CommandBehavior NOT_SUPPORT_BEHAVIOR = ~(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);

            if ((behavior & NOT_SUPPORT_BEHAVIOR) != default(CommandBehavior))
            {
                throw new NotSupportedException(string.Format(
                    "Значение поведения для команды выполнения запроса выборки \"{0}\" не поддерживается.",
                    behavior));
            }

            if (Connection == null)
            {
                throw new InvalidOperationException(
                    "Выполнение команды запроса невозможно так как не задано подключение к информационной базе.");
            }

            // Получение контекста
            var globalContext = _globalContextProvider.GlobalContext;
            using (var query = globalContext.NewObject<IQuery>())
            {
                query.Text = CommandText;
                var queryResult = query.Execute();

                return new OneSDataReader(queryResult);
            }
        }

        /// <summary>Выполняет текст команды применительно к соединению к информационной базе 1С.</summary>
        /// <returns>Читатель данных.</returns>
        public new OneSDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>Выполняет текст команды применительно к соединению к информационной базе 1С.</summary>
        /// <param name="behavior">Поведение выполнения команды, специфицируя описание результатов запроса и его воздействия на базу данных.</param>
        /// <returns>Читатель данных.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        /// <summary>Выполняет для запрос и возвращает количество задействованных в инструкции строк.</summary>
        /// <returns>
        /// Язык запросов 1С не поддерживает манипуляции данными, поэтому данный метод не поддерживается.
        /// </returns>
        public override int ExecuteNonQuery()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Выполняет запрос и возвращает первый столбец первой строки результирующего набора, возвращаемого запросом. 
        /// Дополнительные столбцы и строки игнорируются.
        /// </summary>
        /// <returns></returns>
        public override object ExecuteScalar()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Создает подготовленную версию команды в информационной базе 1С.</summary>
        public override void Prepare()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Получает или задает способ применения результатов команды к объекту <see cref="DataRow"/>
        /// при использовании методом <see cref="System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)"/> объекта <see cref="DbDataAdapter"/>.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return UpdateRowSource.None;
            }
            set
            {
                throw new NotSupportedException("Настройка свойства UpdatedRowSource не поддерживается.");
            }
        }
    }
}