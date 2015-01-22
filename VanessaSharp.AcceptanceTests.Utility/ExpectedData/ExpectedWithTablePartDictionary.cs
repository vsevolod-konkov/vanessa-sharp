﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>
    /// Тестовые данные в словаре СправочникСТабличнойЧастью тестовой БД 1С.
    /// </summary>
    public sealed class ExpectedWithTablePartDictionary
    {
        [Field("Наименование")]
        public string Name;

        [Field("Сумма", FieldType = typeof(double))]
        public decimal Summa;

        public sealed class CompositionTablePart
        {
            [Field("Наименование")]
            public string Name;

            [Field("Цена", FieldType = typeof(double))]
            public decimal Price;

            [Field("Количество")]
            public int Quantity;
        }

        [Field("Состав")]
        public CompositionTablePart[] Composition;

        public static IEnumerable<ExpectedWithTablePartDictionary> ExpectedData
        {
            get
            {
                yield return new ExpectedWithTablePartDictionary
                    {
                        Name = "Первый",
                        Summa = 1000.00m,
                        Composition = new []
                            {
                                new CompositionTablePart
                                    {
                                        Name = "Кросовки",
                                        Price = 200m,
                                        Quantity = 4
                                    }
                            }
                    };

                yield return new ExpectedWithTablePartDictionary
                    {
                        Name = "Второй",
                        Summa = 1500m,
                        Composition = new CompositionTablePart[0]
                    };
            }
        }
    }
}