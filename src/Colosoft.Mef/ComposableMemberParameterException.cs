using System;

namespace Colosoft.Mef
{
    [Serializable]
    public class ComposableMemberParameterException : Exception
    {
        internal ComposableMember Member { get; private set; }

        internal ProviderParameterImportDefinition Parameter { get; set; }

        public ComposableMemberParameterException()
        {
        }

        public ComposableMemberParameterException(string message)
            : base(message)
        {
        }

        public ComposableMemberParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ComposableMemberParameterException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        internal ComposableMemberParameterException(ComposableMember member, ProviderParameterImportDefinition parameter, Exception innerException)
            : base(
                  ResourceMessageFormatter.Create(
                      () => Properties.Resources.ComposableMemberParameterExcepton_Message,
                      parameter != null ? parameter.Parameter.Name : null,
                      parameter != null ? parameter.Parameter.ParameterType.FullName : null,
                      member != null ? member.DeclaringType.FullName : null).Format(),
                  innerException)
        {
            this.Member = member;
            this.Parameter = parameter;
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
