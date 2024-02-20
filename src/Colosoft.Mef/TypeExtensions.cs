using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Mef
{
    public static class TypeExtensions
    {
        private static Type[] knownLazyTypes;

        public static bool IsEnumerable(this Type source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source == typeof(string))
            {
                return false;
            }

            return source == typeof(System.Collections.IEnumerable) || source.GetInterfaces().Contains(typeof(System.Collections.IEnumerable));
        }

        public static bool IsLazyType(this Type source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (knownLazyTypes == null)
            {
                knownLazyTypes = new[] { typeof(Lazy<>), typeof(Lazy<,>) };
            }

            while (!source.IsGenericType && source.BaseType != null)
            {
                source = source.BaseType;
            }

            if (!source.IsGenericType)
            {
                return false;
            }

            var genericTypeDefinition = source.GetGenericTypeDefinition();

            source = genericTypeDefinition.IsAssignableFrom(typeof(IEnumerable<>))
                         ? source.GetGenericArguments().FirstOrDefault().GetGenericTypeDefinition()
                         : genericTypeDefinition;

            return knownLazyTypes.Any(known => known.IsAssignableFrom(source));
        }

        public static Type ToLazyType(this Type source)
        {
            if (!source.IsLazyType())
            {
                return null;
            }

            var arguments =
                source.GetGenericArguments();

            var contractType = (arguments.Length > 0) ? arguments[0] : typeof(object);

            var metadataType = (arguments.Length > 1) ? arguments[1] : typeof(IDictionary<string, object>);

            return typeof(Lazy<,>).MakeGenericType(contractType, metadataType);
        }

        public static Type ToLazyCollectionType(this Type source)
        {
            if (!source.IsLazyType())
            {
                return null;
            }

            Type[] arguments =
                source.GetGenericArguments();

            Type contractType = (arguments.Length > 0) ?
                arguments[0] : null;

            Type metadataType = (arguments.Length > 1) ?
                arguments[1] : null;

            if (contractType == null)
            {
                return typeof(List<Lazy<object>>);
            }

            return (metadataType == null) ?
                typeof(List<>).MakeGenericType(typeof(Lazy<>).MakeGenericType(contractType)) :
                typeof(List<>).MakeGenericType(typeof(Lazy<,>).MakeGenericType(contractType, metadataType));
        }
    }
}
