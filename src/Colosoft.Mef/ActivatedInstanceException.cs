using System;

namespace Colosoft.Mef
{
    [Serializable]
    public class ActivatedInstanceException : Exception
    {
        public ActivatedInstanceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ActivatedInstanceException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public ActivatedInstanceException()
        {
        }

        public ActivatedInstanceException(string message)
            : base(message)
        {
        }
    }
}
