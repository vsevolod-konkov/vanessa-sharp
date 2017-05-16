using System.Collections.Generic;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>Тестовый справочник с табличной частью.</summary>
    [OneSDataSource("Справочник.СправочникСТабличнойЧастью")]
    public sealed class WithTablePartDictionary
    {
        [OneSDataColumn("Наименование")]
        public string Name { get; set; }

        [OneSDataColumn("Сумма")]
        public decimal Summa { get; set; }

        [OneSDataColumn("Состав", OneSDataColumnKind.TablePart)]
        public IEnumerable<CompositeItem> Composite { get; set; } 

        public sealed class CompositeItem
        {
            [OneSDataColumn("Наименование")]
            public string Name { get; set; }

            [OneSDataColumn("Цена")]
            public decimal Price { get; set; }

            [OneSDataColumn("Количество")]
            public int Quantity { get; set; }
        }
    }
}
