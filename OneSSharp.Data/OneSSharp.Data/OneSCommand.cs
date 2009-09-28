using System.Data.Common;
using System.Data;

namespace VsevolodKonkov.OneSSharp.Data
{
    /// <summary>Команда запроса к 1С.</summary>
    public sealed class OneSCommand : DbCommand
    {
        public override void Cancel()
        {
            throw new System.NotImplementedException();
        }

        public override string CommandText
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override int CommandTimeout
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override CommandType CommandType
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new System.NotImplementedException();
        }

        protected override DbConnection DbConnection
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { throw new System.NotImplementedException(); }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new System.NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new System.NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new System.NotImplementedException();
        }

        public override void Prepare()
        {
            throw new System.NotImplementedException();
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
