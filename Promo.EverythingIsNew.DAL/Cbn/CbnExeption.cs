using System;
using System.Runtime.Serialization;

namespace Promo.EverythingIsNew.DAL.Cbn
{
    [Serializable]
    public class CbnException : Exception
    {
        public CbnException()
            : base()
        { }

        public CbnException(string message)
            : base(message)
        { }

        public CbnException(string format, params object[] args)
            : base(string.Format(format, args))
        { }

        public CbnException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public CbnException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        { }

        protected CbnException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
