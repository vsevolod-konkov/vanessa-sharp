using System;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>Класс тестового справочника.</summary>
    [OneSDataSource("Справочник.ТестовыйСправочник")]
    public sealed class TestDictionary
    {
        [OneSDataColumn("СтроковоеПоле")]
        public string StringField;

        [OneSDataColumn("ЦелочисленноеПоле")]
        public int IntegerField;
            
        [OneSDataColumn("ЧисловоеПоле")]
        public double RealField;

        [OneSDataColumn("БулевоПоле")]
        public bool BooleanField;

        [OneSDataColumn("ДатаПоле")] 
        public DateTime DateField;

        [OneSDataColumn("ДатаВремяПоле")]
        public DateTime DateTimeField;

        [OneSDataColumn("ВремяПоле")]
        public DateTime TimeField;

        [OneSDataColumn("НеограниченноеСтроковоеПоле")] 
        public string UnboundedStringField;

        [OneSDataColumn("СимвольноеПоле")]
        public char CharField;
    }
}