using System;
using System.Diagnostics;

namespace Colosoft.Mef
{
    internal class ComposableField : ComposableMember
    {
        public System.Reflection.FieldInfo Field { get; protected set; }

        public override bool IsLazyType
        {
            get { return this.ReturnType.IsLazyType(); }
        }

        public override bool IsReadable
        {
            [DebuggerStepThrough]
            get { return true; }
        }

        public override bool IsInstanceNeeded
        {
            [DebuggerStepThrough]
            get { return !this.Field.IsStatic; }
        }

        public override bool IsWritable
        {
            [DebuggerStepThrough]
            get { return !this.Field.IsInitOnly && !this.Field.IsLiteral; }
        }

        public override Type ReturnType
        {
            [DebuggerStepThrough]
            get { return this.Field.FieldType; }
        }

        public ComposableField(System.Reflection.MemberInfo member)
            : base(member)
        {
            var info = member as System.Reflection.FieldInfo;
            if (info == null)
            {
                throw new InvalidOperationException("The specified value for the member parameter was not a FieldInfo instance.");
            }

            this.Field = info;

            this.ValueGetter =
                getInstance =>
                this.Field.GetValue(getInstance);

            this.ValueSetter =
                (setInstance, setValue) =>
                this.Field.SetValue(setInstance, setValue);
        }
    }
}
