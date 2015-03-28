using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>
    /// Ожидаемые данные для тестового иерархического справочника.
    /// </summary>
    public sealed class ExpectedHierarchicalTestDictionary
    {
        /// <summary>Ожидаемые данные в БД 1С.</summary>
        public static ReadOnlyCollection<ExpectedHierarchicalTestDictionary> ExpectedData
        {
            get { return _expectedData; }
        }
        private static readonly ReadOnlyCollection<ExpectedHierarchicalTestDictionary> _expectedData
            = new ReadOnlyCollection<ExpectedHierarchicalTestDictionary>(GetExpectedDataArray());

        private static ExpectedHierarchicalTestDictionary[] GetExpectedDataArray()
        {
            return new []
                {
                    new ExpectedHierarchicalTestDictionary
                        {
                            Name = "Одежда",
                            Children = new []
                                {
                                    new ExpectedHierarchicalTestDictionary
                                        {
                                            Name = "Женская одежда и белье",
                                            Children = new []
                                                {
                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Гольфы Грация 2+2",
                                                            Quantity = 150,
                                                            Price = 65m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Колготки Золотая Грация",
                                                            Quantity = 300,
                                                            Price = 152m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Комплект Charmante",
                                                            Quantity = 720,
                                                            Price = 791m
                                                        }
                                                }
                                        },

                                    new ExpectedHierarchicalTestDictionary
                                        {
                                            Name = "Мужская одежда",
                                            Children = new []
                                                {
                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Носки Пингонс",
                                                            Quantity = 1400,
                                                            Price = 80m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Пижама Peche Monnaie",
                                                            Quantity = 380,
                                                            Price = 3145m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Халат Peche Monnaie",
                                                            Quantity = 25,
                                                            Price = 4120
                                                        }
                                                }
                                        },

                                    new ExpectedHierarchicalTestDictionary
                                        {
                                            Name = "Обувь",
                                            Children = new[]
                                                {
                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Балеринки ISOTONER",
                                                            Quantity = 650,
                                                            Price = 1020m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Мокасины ISOTONER",
                                                            Quantity = 450,
                                                            Price = 1020m
                                                        }
                                                }
                                        }
                                }
                        },

                    new ExpectedHierarchicalTestDictionary
                        {
                            Name = "Продукты",
                            Children = new[]
                                {
                                    new ExpectedHierarchicalTestDictionary
                                        {
                                            Name = "Бакалея",
                                            Children = new[]
                                                {
                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Гречка Мистраль",
                                                            Quantity = 200,
                                                            Price = 83m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Фасоль Шебекинская Южная",
                                                            Quantity = 1000,
                                                            Price = 112m
                                                        }
                                                }
                                        },

                                    new ExpectedHierarchicalTestDictionary
                                        {
                                            Name = "Молочные продукты",
                                            Children = new[]
                                                {
                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Коктейль Новая деревня",
                                                            Quantity = 500,
                                                            Price = 90.20m
                                                        },

                                                    new ExpectedHierarchicalTestDictionary
                                                        {
                                                            Name = "Сливки Чистый Край",
                                                            Quantity = 200,
                                                            Price = 174m
                                                        }
                                                }
                                        }
                                }
                        }
                };
        }

        [Field("Наименование")]
        public string Name;

        [Field("Количество", FieldType = typeof(double))] 
        public int? Quantity;

        [Field("Цена", FieldType = typeof(double))]
        public decimal? Price;

        public ExpectedHierarchicalTestDictionary[] Children;
    }
}
