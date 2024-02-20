using System;

namespace Colosoft.Mef
{
    [Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class AssemblyRepositoryCatalogException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        public AssemblyRepositoryCatalogException(string message)
            : base(message)
        {
        }

        public AssemblyRepositoryCatalogException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AssemblyRepositoryCatalogException()
        {
        }

        protected AssemblyRepositoryCatalogException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
        }
    }
}
