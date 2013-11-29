using System.Collections;
using System.Data.Common;
using System.Collections.ObjectModel;
using System.Linq;
using VanessaSharp.Data.Utility;

namespace VanessaSharp.Data
{
    /// <summary>Построитель строки соединения с базой 1С.</summary>
    public sealed class OneSConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string CATALOG_KEY = "File";
        private const string USER_KEY = "Usr";
        private const string PASSWORD_KEY = "Pwd";
        
        /// <summary>Ключи подключения.</summary>
        private static readonly ReadOnlyCollection<string> _keywords 
            = new ReadOnlyCollection<string>(new[]{CATALOG_KEY, USER_KEY, PASSWORD_KEY});

        /// <summary>Полный путь к каталогу 1С базы.</summary>
        public string Catalog
        {
            get
            {
                return GetValue(CATALOG_KEY);
            }

            set
            {
                this[CATALOG_KEY] = value;
            }
        }

        /// <summary>Имя пользователя, под которым производится соединение.</summary>
        public string User
        {
            get
            {
                return GetValue(USER_KEY);
            }

            set
            {
                this[USER_KEY] = value;
            }
        }

        /// <summary>Пароль пользователя, под которым производится соединение.</summary>
        public string Password
        {
            get
            {
                return GetValue(PASSWORD_KEY);
            }

            set
            {
                this[PASSWORD_KEY] = value;
            }
        }

        private string GetValue(string key)
        {
            object obj;
            if (!TryGetValue(key, out obj))
                return null;

            return (string)obj;
        }

        /// <summary>Получение ключей строки подключения.</summary>
        private ReadOnlyCollection<string> GetKeys()
        {
            return (base.Keys == null)
                           ? _keywords
                           : base.Keys.OfType<string>().Union(_keywords).ToReadOnly();
        }

        public override ICollection Keys
        {
            get { return GetKeys(); }
        }

        public override ICollection Values
        {
            get
            {
                return (
                           from key in GetKeys()
                           select (object)GetValue(key)
                       )
                    .ToReadOnly();
            }
        }
    }
}
