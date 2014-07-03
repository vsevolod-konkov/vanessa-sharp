using System;
using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>
    /// Тестовые данные в словаре ТестовыйСправочник тестовой БД 1С.
    /// </summary>
    public sealed class ExpectedTestDictionary
    {
        [Field("СтроковоеПоле")]
        public string StringField;

        [Field("ЦелочисленноеПоле", FieldType = typeof(double))]
        public int IntField;

        [Field("ЧисловоеПоле")]
        public double NumberField;

        [Field("БулевоПоле")]
        public bool BooleanField;

        [Field("ДатаПоле")]
        public DateTime DateField;

        [Field("ДатаВремяПоле")]
        public DateTime DateTimeField;

        [Field("ВремяПоле")]
        public DateTime TimeField;

        [Field("НеограниченноеСтроковоеПоле")]
        public string UndoundStringField;

        [Field("СимвольноеПоле", FieldType = typeof(string))]
        public char CharField;

        /// <summary>Ожидаемые данные в БД 1С.</summary>
        public static ReadOnlyCollection<ExpectedTestDictionary> ExpectedData
        {
            get { return _expectedData; }
        }
        private static readonly ReadOnlyCollection<ExpectedTestDictionary> _expectedData 
            = new ReadOnlyCollection<ExpectedTestDictionary>(GetExpectedDataArray());

        private static ExpectedTestDictionary[] GetExpectedDataArray()
        {
            return new[]
            {
                new ExpectedTestDictionary
                {
                    StringField = "Тестирование",
                    IntField = 234,
                    NumberField = 546.323,
                    BooleanField = true,
                    DateField = new DateTime(2014, 01, 15),
                    DateTimeField = new DateTime(2014, 01, 08, 4, 33, 43),
                    TimeField = new DateTime(100, 1, 1, 23, 43, 43),
                    UndoundStringField = @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                    CharField = 'А'
                },

                new ExpectedTestDictionary
                {
                    StringField = "",
                    IntField = 0,
                    NumberField = 0,
                    BooleanField = false,
                    DateField = new DateTime(100, 1, 1),
                    DateTimeField = new DateTime(100, 1, 1),
                    TimeField = new DateTime(100, 1, 1),
                    UndoundStringField = "",
                    CharField = ' '
                }
            };
        }
    }
}
