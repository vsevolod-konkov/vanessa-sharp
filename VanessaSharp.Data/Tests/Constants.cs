using System;
using System.IO;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Константы для тестирования.</summary>
    internal static class Constants
    {
        /// <summary>Путь к тестовой БД.</summary>
        public static string TestCatalog
        {
            get { return _testCatalog; }
        }
        private static readonly string _testCatalog 
            = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "Db");

        /// <summary>Имя пользователя.</summary>
        public const string TestUser = "Абдулов (директор)";

        /// <summary>Имя альтернативного пользователя.</summary>
        public const string AlternativeTestUser = "Бакинская (бухгалтер-экономист)";
    }
}
