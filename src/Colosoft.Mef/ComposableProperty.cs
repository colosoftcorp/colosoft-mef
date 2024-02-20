using System;
using System.Diagnostics;

namespace Colosoft.Mef
{
    internal class ComposableProperty : ComposableMember
    {
        public ComposableProperty(System.Reflection.MemberInfo member)
            : base(member)
        {
            var info = member as System.Reflection.PropertyInfo;
            this.Property = info ?? throw new InvalidOperationException("The specified value for the member parameter was not a PropertyInfo instance.");

            var getMethod = this.Property.GetGetMethod(false);
            if (getMethod != null)
            {
                this.ValueGetter =
                    getInstance =>
                    getMethod.Invoke(getInstance, null);
            }

            var setMethod = this.Property.GetSetMethod(false);
            if (setMethod != null)
            {
                this.ValueSetter =
                    (setInstance, setValue) =>
                    setMethod.Invoke(setInstance, new[] { setValue });
            }
        }

        public System.Reflection.PropertyInfo Property { get; protected set; }

        public override bool IsLazyType
        {
            get { return this.ReturnType.IsLazyType(); }
        }

        public override bool IsReadable
        {
            [DebuggerStepThrough]
            get { return this.ValueGetter != null; }
        }

        public override bool IsInstanceNeeded
        {
            [DebuggerStepThrough]
            get
            {
                var info =
                    this.Property.GetGetMethod() ?? this.Property.GetSetMethod();
                return !info.IsStatic;
            }
        }

        public override bool IsWritable
        {
            [DebuggerStepThrough]
            get { return this.ValueSetter != null; }
        }

        public override Type ReturnType
        {
            [DebuggerStepThrough]
            get { return this.Property.PropertyType; }
        }
    }
}
