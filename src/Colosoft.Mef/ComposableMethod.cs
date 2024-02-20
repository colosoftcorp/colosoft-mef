using Colosoft.Reflection;
using System;
using System.Diagnostics;

namespace Colosoft.Mef
{
    internal class ComposableMethod : ComposableMember
    {
        public ComposableMethod(System.Reflection.MemberInfo member)
            : base(member)
        {
            var info = member as System.Reflection.MethodInfo;
            this.Method = info ?? throw new InvalidOperationException("The specified value for the member parameter was not a MethodInfo instance.");

            this.ValueGetter =
                getInstance =>
                this.Method.CreateDelegate(getInstance);
        }

        public System.Reflection.MethodInfo Method { get; protected set; }

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
            get { return !this.Method.IsStatic; }
        }

        public override bool IsWritable
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        public override Type ReturnType
        {
            [DebuggerStepThrough]
            get { return this.Method.ReturnType; }
        }
    }
}
