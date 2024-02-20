using System;
using System.Diagnostics;

namespace Colosoft.Mef
{
    internal class ComposableConstructor : ComposableMember
    {
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
            get { return !this.Constructor.IsStatic; }
        }

        public override bool IsWritable
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        public System.Reflection.ConstructorInfo Constructor { get; protected set; }

        public override Type ReturnType
        {
            [DebuggerStepThrough]
            get { return this.Constructor.DeclaringType; }
        }

        public ComposableConstructor(System.Reflection.MemberInfo member)
            : base(member)
        {
            var info = member as System.Reflection.ConstructorInfo;
            if (info == null)
            {
                throw new InvalidOperationException("The specified value for the member parameter was not a MethodInfo instance.");
            }

            this.Constructor = info;
        }

        public override object GetValue(object instance)
        {
            return instance;
        }
    }
}
