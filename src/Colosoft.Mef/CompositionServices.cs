using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Colosoft.Mef
{
    internal static class CompositionServices
    {
        public static string GetContractNameFromExportDescription(Func<MemberInfo> memberGetter, ExportDescription exportDescription)
        {
            if (memberGetter is null)
            {
                throw new ArgumentNullException(nameof(memberGetter));
            }

            if (exportDescription is null)
            {
                throw new ArgumentNullException(nameof(exportDescription));
            }

            if (!string.IsNullOrEmpty(exportDescription.ContractName))
            {
                return exportDescription.ContractName;
            }

            if (exportDescription.ContractTypeName != null)
            {
                return exportDescription.ContractTypeName.FullName;
            }

            if (exportDescription.ContractType != null)
            {
                return AttributedModelServices.GetContractName(exportDescription.ContractType);
            }

            var memberValue = memberGetter();

            if (memberValue == null)
            {
                throw new InvalidOperationException("member is null");
            }

            if (memberValue.MemberType == MemberTypes.Method)
            {
                return AttributedModelServices.GetTypeIdentity((MethodInfo)memberValue);
            }

            return AttributedModelServices.GetContractName(memberValue.GetDefaultTypeFromMember());
        }

        public static string GetContractNameFromImportDescription(Lazy<MemberInfo> member, ImportDescription importDescription)
        {
            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (importDescription is null)
            {
                throw new ArgumentNullException(nameof(importDescription));
            }

            if (!string.IsNullOrEmpty(importDescription.ContractName))
            {
                return importDescription.ContractName;
            }

            if (importDescription.ContractType != null)
            {
                return AttributedModelServices.GetContractName(importDescription.ContractType);
            }

            var memberValue = member.Value;

            if (memberValue == null)
            {
                throw new InvalidOperationException("member is null");
            }

            if (memberValue.MemberType == MemberTypes.Method)
            {
                return AttributedModelServices.GetTypeIdentity((MethodInfo)memberValue);
            }
            else if (memberValue.MemberType == MemberTypes.Constructor)
            {
                return AttributedModelServices.GetContractName(memberValue.GetDefaultTypeFromMember());
            }

            return AttributedModelServices.GetContractName(memberValue.GetDefaultTypeFromMember());
        }

        public static string GetContractNameFromImportDescription(ParameterInfo member, ImportDescription importDescription)
        {
            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (importDescription is null)
            {
                throw new ArgumentNullException(nameof(importDescription));
            }

            if (!string.IsNullOrEmpty(importDescription.ContractName))
            {
                return importDescription.ContractName;
            }

            if (importDescription.ContractType != null)
            {
                return AttributedModelServices.GetContractName(importDescription.ContractType);
            }

            return AttributedModelServices.GetContractName(member.ParameterType);
        }

        public static IDictionary<string, object> GetMetadataFromExportDescription(Func<MemberInfo> memberGetter, ExportDescription exportDescription, CreationPolicy creationPolicy)
        {
            if (memberGetter is null)
            {
                throw new ArgumentNullException(nameof(memberGetter));
            }

            if (exportDescription is null)
            {
                throw new ArgumentNullException(nameof(exportDescription));
            }

            var metadata = new Dictionary<string, object>();
            var contract = GetTypeIdentityFromExportDescription(memberGetter, exportDescription);

            metadata.Add(
                System.ComponentModel.Composition.Hosting.CompositionConstants.ExportTypeIdentityMetadataName, contract);

            if (creationPolicy != CreationPolicy.Any)
            {
                metadata.Add("System.ComponentModel.Composition.CreationPolicy", creationPolicy);
            }

            if (exportDescription.Metadata != null)
            {
                foreach (var pair in exportDescription.Metadata)
                {
                    metadata.Add(pair.Key, pair.Value);
                }
            }

            return metadata;
        }

        public static IEnumerable<KeyValuePair<string, Type>> GetMetadataFromImportDescription(this Lazy<MemberInfo> member, ImportDescription importDescription)
        {
            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (importDescription is null)
            {
                throw new ArgumentNullException(nameof(importDescription));
            }

            var metadata = new Dictionary<string, Type>();

            return metadata;
        }

        public static IEnumerable<KeyValuePair<string, Type>> GetMetadataFromImportDescription(this ParameterInfo parameter, ImportDescription importDescription)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (importDescription is null)
            {
                throw new ArgumentNullException(nameof(importDescription));
            }

            var metadata = new Dictionary<string, Type>();

            return metadata;
        }

        public static string GetTypeIdentityFromExportDescription(this Func<MemberInfo> memberGetter, ExportDescription export)
        {
            if (export.ContractTypeName != null)
            {
                return export.ContractTypeName.FullName;
            }

            if (export.ContractType != null)
            {
                return AttributedModelServices.GetTypeIdentity(export.ContractType);
            }

            var memberValue = memberGetter();

            if (memberValue == null)
            {
                throw new InvalidOperationException("member is null");
            }

            if (memberValue.MemberType == MemberTypes.Method)
            {
                return AttributedModelServices.GetTypeIdentity((MethodInfo)memberValue);
            }

            return memberValue.MemberType == MemberTypes.Method
                ? AttributedModelServices.GetTypeIdentity((MethodInfo)memberValue)
                : AttributedModelServices.GetTypeIdentity(memberValue.GetDefaultTypeFromMember());
        }

        public static string GetTypeIdentityFromImportDescription(this Lazy<MemberInfo> member, ImportDescription import)
        {
            if (import.ContractTypeName != null)
            {
                return import.ContractTypeName.FullName;
            }

            if (import.ContractType != null)
            {
                return AttributedModelServices.GetTypeIdentity(import.ContractType);
            }

            var memberValue = member.Value;

            if (memberValue == null)
            {
                throw new InvalidOperationException("member is null");
            }

            if (memberValue.MemberType == MemberTypes.Method)
            {
                return AttributedModelServices.GetTypeIdentity((MethodInfo)memberValue);
            }

            return memberValue.MemberType == MemberTypes.Method
                ? AttributedModelServices.GetTypeIdentity((MethodInfo)memberValue)
                : AttributedModelServices.GetTypeIdentity(memberValue.GetDefaultTypeFromMember());
        }

        public static string GetTypeIdentityFromImportDescription(this ParameterInfo parameterInfo, ImportDescription import)
        {
            if (import.ContractTypeName != null)
            {
                return import.ContractTypeName.FullName;
            }

            if (import.ContractType != null)
            {
                return AttributedModelServices.GetTypeIdentity(import.ContractType);
            }

            return AttributedModelServices.GetTypeIdentity(parameterInfo.ParameterType);
        }

        private static Type GetDefaultTypeFromMember(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;

                case MemberTypes.NestedType:
                case MemberTypes.TypeInfo:
                    return (Type)member;

                case MemberTypes.Constructor:
                    return ((ConstructorInfo)member).DeclaringType;

                case MemberTypes.Field:
                default:
                    return ((FieldInfo)member).FieldType;
            }
        }

        internal static CreationPolicy GetRequiredCreationPolicy(this System.ComponentModel.Composition.Primitives.ImportDefinition definition)
        {
            var definition2 = definition as System.ComponentModel.Composition.Primitives.ContractBasedImportDefinition;
            if (definition2 != null)
            {
                return definition2.RequiredCreationPolicy;
            }

            return CreationPolicy.Any;
        }
    }
}
