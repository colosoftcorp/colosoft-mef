using System;
using System.Diagnostics;

namespace Colosoft.Mef
{
    internal class ComposableParameter : ComposableMember
    {
        public override Type ReturnType
        {
            get { return this.Parameter.ParameterType; }
        }

        protected System.Reflection.ParameterInfo Parameter { get; private set; }

        public override bool IsLazyType
        {
            [DebuggerStepThrough]
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
            get { return true; }
        }

        public override bool IsWritable
        {
            [DebuggerStepThrough]
            get { return true; }
        }

        public ComposableParameter(System.Reflection.ParameterInfo parameter)
            : base(null)
        {
            this.Parameter = parameter;
        }
    }
}
