using System;
using System.Diagnostics;

namespace Colosoft.Mef
{
    internal class ComposableType : ComposableMember
    {
        public ComposableType(System.Reflection.MemberInfo member)
            : base(member)
        {
        }

        public override Type DeclaringType
        {
            [DebuggerStepThrough]
            get { return this.ReturnType; }
        }

        public override bool IsLazyType
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        public override bool IsReadable
        {
            [DebuggerStepThrough]
            get { return true; }
        }

        public override bool IsInstanceNeeded
        {
            [DebuggerStepThrough]
            get { return true; }
        }

        public override bool IsWritable
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        public override Type ReturnType
        {
            [DebuggerStepThrough]
            get { return (Type)this.Member; }
        }

        public override object GetValue(object instance)
        {
            return instance;
        }
    }
}
