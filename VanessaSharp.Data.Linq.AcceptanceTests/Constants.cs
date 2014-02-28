﻿using System;
using System.IO;

namespace VanessaSharp.Data.Linq.AcceptanceTests
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
            = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AcceptanceTestDb");

        /// <summary>Имя пользователя.</summary>
        public const string TEST_USER = "";
    }
}
