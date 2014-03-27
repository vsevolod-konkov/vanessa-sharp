using System;

namespace VanessaSharp.Data.Linq.PredefinedData
{
    // TODO: Прототип
    /// <summary>Поля.</summary>
    public static class Fields
    {


        public static string GetLocalizedName(this Catalog value)
        {
            var enumType = typeof(Catalog);
            
            var enumName = Enum.GetName(enumType, value);

            var field = enumType.GetField(enumName);

            var attr = (LocalizedNameAttribute)field.GetCustomAttributes(typeof(LocalizedNameAttribute), false)[0];

            return attr.LocalizedName;
        }

        /// <summary>Предопределенные поля каталога.</summary>
        public enum Catalog
        {
            [LocalizedName("Ссылка")]
            Ref,

            [LocalizedName("ПометкаУдаления")]
            DeletionMark,

            [LocalizedName("Код")]
            Code,

            [LocalizedName("Наименование")]
            Description,

            [LocalizedName("Предопределенный")]
            Predefined,

            [LocalizedName("ЭтоГруппа")]
            IsFolder,

            [LocalizedName("Владелец")]
            Owner,

            [LocalizedName("Родитель")]
            Parent,

            [LocalizedName("Представление")]
            Presentation,
        }
    }
}
