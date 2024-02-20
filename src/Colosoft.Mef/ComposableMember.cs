using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Colosoft.Mef
{
    internal abstract class ComposableMember
    {
        private static readonly MethodInfo ToLazyTM;
        private static readonly MethodInfo ToLazyT;

        private Type elementType;

        public virtual Type DeclaringType
        {
            get { return this.Member.DeclaringType; }
        }

        public Type ElementType
        {
            get
            {
                if (this.elementType == null)
                {
                    this.elementType = this.GetElementType();
                }

                return this.elementType;
            }
        }

        public abstract bool IsLazyType { get; }

        public abstract bool IsReadable { get; }

        public abstract bool IsInstanceNeeded { get; }

        public abstract bool IsWritable { get; }

        protected MemberInfo Member { get; set; }

        public abstract Type ReturnType { get; }

        protected Func<object, object> ValueGetter { get; set; }

        protected Action<object, object> ValueSetter { get; set; }

#pragma warning disable S3963 // "static" fields should be initialized inline
#pragma warning disable CA1810 // Initialize reference type static fields inline
        static ComposableMember()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            ToLazyTM = typeof(ComposableMember).GetMethod("ToLazyTM", BindingFlags.NonPublic | BindingFlags.Static);
            ToLazyT = typeof(ComposableMember).GetMethod("ToLazyT", BindingFlags.NonPublic | BindingFlags.Static);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        protected ComposableMember(MemberInfo member)
        {
            this.Member = member;
        }

        public static ComposableMember Create(MemberInfo member)
        {
            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return new ComposableField(member);

                case MemberTypes.Method:
                    return new ComposableMethod(member);

                case MemberTypes.Property:
                    return new ComposableProperty(member);

                case MemberTypes.Constructor:
                    return new ComposableConstructor(member);

                case MemberTypes.NestedType:
                case MemberTypes.TypeInfo:
                    return new ComposableType(member);
            }

            throw new InvalidOperationException("Unsupported member type.");
        }

        public object GetImportValueFromExports(IEnumerable<System.ComponentModel.Composition.Primitives.Export> exports)
        {
            return this.ReturnType.IsEnumerable() ?
                this.GetImportValueCollection(exports) : this.GetImportValueSingle(exports);
        }

        public virtual object GetValue(object instance)
        {
            if (this.ValueGetter == null)
            {
                throw new InvalidOperationException("No function for retrieving the value has been set.");
            }

            return this.ValueGetter.Invoke(instance);
        }

        public virtual void SetValue(object instance, object value)
        {
            if (!this.IsWritable || this.ValueSetter == null)
            {
                throw new InvalidOperationException("Either the member is not writeable or oo function for assigning the value has been set.");
            }

            this.ValueSetter.Invoke(instance, value);
        }

        public Type ToLazyType()
        {
            return this.ElementType != null ?
                this.ElementType.ToLazyCollectionType() : this.ReturnType.ToLazyType();
        }

        private Type GetElementType()
        {
            if (this.ReturnType.IsEnumerable())
            {
                Type[] collectionInterfaces =
                    new[] { typeof(ICollection<>), typeof(IEnumerable<>), typeof(IList<>) };

                if (this.ReturnType.IsGenericType)
                {
                    var genericTypeDefinition = this.ReturnType.GetGenericTypeDefinition();
                    if (collectionInterfaces.Contains(genericTypeDefinition))
                    {
                        return this.ReturnType.GetGenericArguments()[0];
                    }
                }

                foreach (Type targetInterface in this.ReturnType.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var genericTypeDefinition = targetInterface.GetGenericTypeDefinition();
                    if (collectionInterfaces.Contains(genericTypeDefinition))
                    {
                        return targetInterface.GetGenericArguments()[0];
                    }
                }
            }

            return null;
        }

        private object GetImportValueCollection(IEnumerable<System.ComponentModel.Composition.Primitives.Export> exports)
        {
            if (!exports.Any())
            {
                return null;
            }

            var isLazy = this.ElementType.IsLazyType();

            if (!isLazy)
            {
                var exportEnumerator = exports.GetEnumerator();

                System.Collections.IList collection = null;

                if (exportEnumerator.MoveNext())
                {
                    Type[] genericTypeInformation =
                        this.ReturnType.GetGenericArguments();

                    if (genericTypeInformation.Length == 0)
                    {
                        return exportEnumerator.Current.Value;
                    }
                    else
                    {
                        Type collectionType =
                            typeof(List<>).MakeGenericType(genericTypeInformation[0]);
                        collection = (System.Collections.IList)Activator.CreateInstance(collectionType);
                    }

                    collection.Add(exportEnumerator.Current.Value);

                    while (exportEnumerator.MoveNext())
                    {
                        collection.Add(exportEnumerator.Current.Value);
                    }
                }
                else
                {
                    collection = new System.Collections.ArrayList();
                }

                return collection;
            }

            var exportCollection =
                (System.Collections.IList)Activator.CreateInstance(this.ToLazyType(), null);

            MethodInfo lazyFactory;
            if (isLazy)
            {
                var genericArgs = this.ElementType.GetGenericArguments();

                if (genericArgs.Length == 0)
                {
                    throw new InvalidOperationException($"Not found generic arguments for type '{this.ElementType.FullName}' in lazy.");
                }

                Type contractType = genericArgs[0];
                if (genericArgs.Length == 2)
                {
                    Type metadataViewType = genericArgs.Length == 2 ? genericArgs[1] : typeof(IDictionary<string, object>);
                    lazyFactory = ToLazyTM.MakeGenericMethod(contractType, metadataViewType);
                }
                else
                {
                    lazyFactory = ToLazyT.MakeGenericMethod(contractType);
                }
            }
            else
            {
                lazyFactory = ToLazyT.MakeGenericMethod(this.ElementType);
            }

            foreach (var item in exports)
            {
                var typed = lazyFactory.Invoke(null, new[] { item });
                exportCollection.Add(typed);
            }

            return exportCollection;
        }

        private object GetImportValueSingle(IEnumerable<System.ComponentModel.Composition.Primitives.Export> exports)
        {
            var single = exports.FirstOrDefault();

            if (single == null)
            {
                return null;
            }

            MethodInfo lazyFactory;

            if (!this.ReturnType.IsLazyType())
            {
                return single.Value;
            }

            if (!this.ReturnType.IsGenericType)
            {
                return single;
            }

            var genericArgs = this.ReturnType.GetGenericArguments();
            Type contractType = genericArgs[0];
            if (genericArgs.Length == 2)
            {
                Type metadataViewType = genericArgs.Length == 2 ? genericArgs[1] : typeof(IDictionary<string, object>);
                lazyFactory = ToLazyTM.MakeGenericMethod(contractType, metadataViewType);
            }
            else
            {
                lazyFactory = ToLazyT.MakeGenericMethod(contractType);
            }

            return lazyFactory.Invoke(null, new[] { single });
        }
    }
}
