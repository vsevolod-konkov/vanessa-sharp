using System;
using System.Data.Common;
using System.Data;

namespace VanessaSharp.Data
{
    /// <summary>Параметр к команде запроса в 1С.</summary>
    public sealed class OneSParameter : DbParameter
    {
        /// <summary>Конструктор без аргументов.</summary>
        public OneSParameter()
        {
            SourceColumnNullMapping = true;
            SourceVersion = DataRowVersion.Default;
            _value = DBNull.Value;
        }

        /// <summary>Конструктор принимающий имя параметра.</summary>
        /// <param name="parameterName">Имя параметра.</param>
        public OneSParameter(string parameterName) : this()
        {
            ParameterName = parameterName;
        }

        /// <summary>Конструктор принимающий имя и значение параметра.</summary>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="value">Значение параметра.</param>
        public OneSParameter(string parameterName, object value) : this(parameterName)
        {
            Value = value;
        }
        
        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DbType"/> of the parameter.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.DbType"/> values. The default is <see cref="F:System.Data.DbType.String"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property is not set to a valid <see cref="T:System.Data.DbType"/>.</exception>
        /// <filterpriority>1</filterpriority>
        public override DbType DbType
        {
            get { return DbType.Object; }
            set
            {
                if (value != DbType.Object)
                {
                    throw new NotSupportedException(string.Format(
                        "Значение \"{0}\" недопустимо для свойства DbType, так как 1С не поддерживает типизированные параметры. Допустимо только значение \"{1}\".",
                        value, DbType.Object));    
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.ParameterDirection"/> values. The default is Input.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property is not set to one of the valid <see cref="T:System.Data.ParameterDirection"/> values.</exception>
        /// <filterpriority>1</filterpriority>
        public override ParameterDirection Direction
        {
            get
            {
                return ParameterDirection.Input;
            }
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new NotSupportedException(string.Format(
                        "Значение \"{0}\" недопустимо для свойства Direction, так как 1С поддерживает только входные параметры. Допустимо только значение \"{1}\".",
                        value, ParameterDirection.Input));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter accepts null values.
        /// </summary>
        /// <returns>
        /// true if null values are accepted; otherwise false. The default is false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool IsNullable
        {
            get { return true; }
            set
            {
                if (!value)
                {
                    throw new NotSupportedException(string.Format(
                        "Значение \"{0}\" недопустимо для свойства IsNullable, так как 1С не поддерживает контроль значений для параметров. Допустимо только значение \"{1}\".",
                        value, true));
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="T:System.Data.Common.DbParameter"/>.
        /// </summary>
        /// <returns>
        /// The name of the <see cref="T:System.Data.Common.DbParameter"/>. The default is an empty string ("").
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string ParameterName
        {
            get; set;
        }

        /// <summary>
        /// Resets the DbType property to its original settings.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void ResetDbType()
        {}

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <returns>
        /// The maximum size, in bytes, of the data within the column. The default value is inferred from the parameter value.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Size
        {
            get { return 0; }
            set
            {
                if (value != 0)
                {
                    throw new NotSupportedException(string.Format(
                        "Значение \"{0}\" недопустимо для свойства Size, так как 1С не поддерживает строго типизированные параметры. Допустимо только значение \"{1}\".",
                        value, 0));
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the source column mapped to the <see cref="T:System.Data.DataSet"/> and used for loading or returning the <see cref="P:System.Data.Common.DbParameter.Value"/>.
        /// </summary>
        /// <returns>
        /// The name of the source column mapped to the <see cref="T:System.Data.DataSet"/>. The default is an empty string.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string SourceColumn { get; set; }

        /// <summary>
        /// Sets or gets a value which indicates whether the source column is nullable. This allows <see cref="T:System.Data.Common.DbCommandBuilder"/> to correctly generate Update statements for nullable columns.
        /// </summary>
        /// <returns>
        /// true if the source column is nullable; false if it is not.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool SourceColumnNullMapping { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DataRowVersion"/> to use when you load <see cref="P:System.Data.Common.DbParameter.Value"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.DataRowVersion"/> values. The default is Current.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property is not set to one of the <see cref="T:System.Data.DataRowVersion"/> values.</exception>
        /// <filterpriority>1</filterpriority>
        public override DataRowVersion SourceVersion { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that is the value of the parameter. The default value is null.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override object Value
        {
            get { return _value; }
            set { _value = value ?? DBNull.Value; }
        }
        private object _value;
    }
}
