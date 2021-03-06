﻿using System;
using System.Collections;
using System.Collections.Generic;
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

        private const string SERVER_NAME_KEY = "Srvr";
        private const string SERVER_DATABASE_NAME_KEY = "Ref";

        private const string USER_KEY = "Usr";
        private const string PASSWORD_KEY = "Pwd";

        /// <summary>Ключи подключения.</summary>
        private static readonly KnownKeywordsCollection _knownKeywords
            = new KnownKeywordsCollection(
                CATALOG_KEY,
                SERVER_NAME_KEY, SERVER_DATABASE_NAME_KEY,
                USER_KEY, PASSWORD_KEY);

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
                string normalizeKeyword;
                
                return (_knownKeywords.Contains(keyword, out normalizeKeyword))
                           ? GetKnownFieldValue(normalizeKeyword)
                           : base[normalizeKeyword];
            }
            set
            {
                base[_knownKeywords.GetNormalizeKeyword(keyword)] = value;
            }
        }

        /// <summary>Полный путь к каталогу 1С базы.</summary>
        public string Catalog
        {
            get { return GetKnownFieldValue(CATALOG_KEY); }

            set { this[CATALOG_KEY] = value; }
        }

        /// <summary>
        /// Имя сервера 1С:Предприятия.
        /// </summary>
        public string ServerName
        {
            get { return GetKnownFieldValue(SERVER_NAME_KEY); }

            set { this[SERVER_NAME_KEY] = value; }
        }

        /// <summary>
        /// Имя информационной базы на сервере 1С.
        /// </summary>
        public string ServerDatabaseName
        {
            get { return GetKnownFieldValue(SERVER_DATABASE_NAME_KEY); }

            set { this[SERVER_DATABASE_NAME_KEY] = value; }
        }

        /// <summary>Имя пользователя, под которым производится соединение.</summary>
        public string User
        {
            get { return GetKnownFieldValue(USER_KEY); }

            set { this[USER_KEY] = value; }
        }

        /// <summary>Пароль пользователя, под которым производится соединение.</summary>
        public string Password
        {
            get { return GetKnownFieldValue(PASSWORD_KEY); }

            set { this[PASSWORD_KEY] = value; }
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
                ? _knownKeywords.Collection
                : base.Keys.Cast<string>().Union(_knownKeywords.Collection).ToReadOnly();
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

        #region Вспомогательные типы

        /// <summary>Коллекция ключей известных полей.</summary>
        /// <remarks>
        /// Инкапсулирует работу с ключами известными полей в строке подключения.
        /// Структура немутабельна.
        /// </remarks>
        internal sealed class KnownKeywordsCollection
        {
            /// <summary>Сравнитель ключей.</summary>
            private static readonly IComparer<string> _keywordsComparer = StringComparer.OrdinalIgnoreCase;

            /// <summary>Отсортированные ключи.</summary>
            private readonly string[] _sortedKeywords;

            /// <summary>Коллекция ключей.</summary>
            private readonly ReadOnlyCollection<string> _keywordsCollection;

            /// <summary>Конструктор принимающий массив ключей.</summary>
            /// <param name="normalizeKeywords">
            /// Массив ключей, которые будут в коллекции.
            /// Строки ключи проверяются на эквивалентность игнорируя регистр.
            /// Вид заданный в конструкторе считается "нормализованным".
            /// </param>
            public KnownKeywordsCollection(params string[] normalizeKeywords)
            {
                _sortedKeywords = normalizeKeywords.ToArray();
                Array.Sort(_sortedKeywords, _keywordsComparer);

                _keywordsCollection = new ReadOnlyCollection<string>(_sortedKeywords);
            }

            /// <summary>
            /// Проверяется есть ли ключ в коллекции.
            /// Сравнение идет с игнорированием регистров символов.
            /// Если ключ есть, то возвращается его нормализованный вид,
            /// В ином случае возвращается исходный ключ.
            /// </summary>
            /// <param name="keyword">Исходный ключ.</param>
            /// <param name="normalizeKeyword">Нормализованный ключ.</param>
            public bool Contains(string keyword, out string normalizeKeyword)
            {
                var index = Array.BinarySearch(_sortedKeywords, keyword, _keywordsComparer);
                
                if (index < 0)
                {
                    normalizeKeyword = keyword;
                    return false;    
                }
                
                normalizeKeyword = _sortedKeywords[index];
                return true;
            }

            /// <summary>Возвращает нормализованный ключ.</summary>
            /// <remarks>
            /// Если ключ есть в коллекции возвращает его нормализованный вид.
            /// В ином случае возвращается исходный ключ.
            /// </remarks>
            /// <param name="keyword">Исходный ключ.</param>
            public string GetNormalizeKeyword(string keyword)
            {
                string result;
                Contains(keyword, out result);

                return result;
            }

            /// <summary>Коллекция нормализованных ключей заданных в конструкторе.</summary>
            public ReadOnlyCollection<string> Collection
            {
                get { return _keywordsCollection; }
            }
        }

        #endregion
    }
}
