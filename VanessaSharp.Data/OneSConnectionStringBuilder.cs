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

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found, trying to get it returns a null reference (Nothing in Visual Basic), and trying to set it creates a new element using the specified key.Passing a null (Nothing in Visual Basic) key throws an <see cref="T:System.ArgumentNullException"/>. Assigning a null value removes the key/value pair.
        /// </returns>
        /// <param name="keyword">The key of the item to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="keyword"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set, and the <see cref="T:System.Data.Common.DbConnectionStringBuilder"/> is read-only. -or-The property is set, <paramref name="keyword"/> does not exist in the collection, and the <see cref="T:System.Data.Common.DbConnectionStringBuilder"/> has a fixed size.</exception>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        /// <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
        /// </PermissionSet>
        public override object this[string keyword]
        {
            get
            {
                return (_keywords.Contains(keyword))
                           ? GetKnownFieldValue(keyword)
                           : base[keyword];
            }
            set
            {
                base[keyword] = value;
            }
        }

        /// <summary>Полный путь к каталогу 1С базы.</summary>
        public string Catalog
        {
            get
            {
                return GetKnownFieldValue(CATALOG_KEY);
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
                return GetKnownFieldValue(USER_KEY);
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
                return GetKnownFieldValue(PASSWORD_KEY);
            }

            set
            {
                this[PASSWORD_KEY] = value;
            }
        }

        private string GetKnownFieldValue(string key)
        {
            object obj;
            if (TryGetValue(key, out obj))
                return (string)obj;

            return string.Empty;
        }

        /// <summary>Получение ключей строки подключения.</summary>
        private ReadOnlyCollection<string> GetKeys()
        {
            return (base.Keys == null)
                           ? _keywords
                           : base.Keys.OfType<string>().Union(_keywords).ToReadOnly();
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection"/> that contains the keys in the <see cref="T:System.Data.Common.DbConnectionStringBuilder"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection"/> that contains the keys in the <see cref="T:System.Data.Common.DbConnectionStringBuilder"/>.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        /// <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
        /// </PermissionSet>
        public override ICollection Keys
        {
            get { return GetKeys(); }
        }
    }
}
