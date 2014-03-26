using System;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Значение из 1С.
    /// </summary>
    public sealed class OneSValue
    {
        public static explicit operator string(OneSValue value)
        {
            return null;
        }

        public static explicit operator int(OneSValue value)
        {
            return 0;
        }

        public static explicit operator double(OneSValue value)
        {
            return 0;
        }

        public static explicit operator bool(OneSValue value)
        {
            return false;
        }

        public static explicit operator DateTime(OneSValue value)
        {
            return DateTime.Now;
        }

        public static explicit operator char(OneSValue value)
        {
            return 'A';
        }

        public object ToObject()
        {
            throw new NotImplementedException();
        }
    }
}
