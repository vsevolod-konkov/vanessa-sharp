using System.Collections;
using System.Data.Common;
using System.Text;

namespace VsevolodKonkov.OneSSharp.Data
{
    /// <summary>Построитель строки соединения с базой 1С.</summary>
    public sealed class OneSConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string CATALOG_KEY = "File";
        private const string USER_KEY = "Usr";
        private const string PASSWORD_KEY = "Pwd";
        
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
    }
}
