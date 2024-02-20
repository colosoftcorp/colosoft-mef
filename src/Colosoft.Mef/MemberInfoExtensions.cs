namespace Colosoft.Mef
{
    internal static class MemberInfoExtensions
    {
        public static System.ComponentModel.Composition.Primitives.ImportCardinality GetCardinality(this System.Reflection.MemberInfo member, bool allowDefault)
        {
            var propertyInfo = member as System.Reflection.PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType.IsEnumerable())
                {
                    return System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrMore;
                }

                return allowDefault
                    ? System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne
                    : System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne;
            }

            var fieldInfo = member as System.Reflection.FieldInfo;
            if (fieldInfo != null)
            {
                if (fieldInfo.FieldType.IsEnumerable())
                {
                    return System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrMore;
                }

                return allowDefault
                    ? System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne
                    : System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne;
            }

            if (member is System.Reflection.ConstructorInfo)
            {
                return System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne;
            }

            throw new System.ComponentModel.Composition.ImportCardinalityMismatchException();
        }

        public static ComposableMember ToComposableMember(this System.Reflection.MemberInfo member)
        {
            return ComposableMember.Create(member);
        }

        public static ComposableMember ToComposableMember(this System.Reflection.ParameterInfo parameter)
        {
            return new ComposableParameter(parameter);
        }
    }
}
