using System;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;

namespace Colosoft.Mef
{
    internal class ProviderImportDefinition : ContractBasedImportDefinition
    {
        private readonly Lazy<MemberInfo> member;

        public ProviderImportDefinition(Lazy<MemberInfo> member, ImportDescription importDescription, System.ComponentModel.Composition.CreationPolicy creationPolicy)
            : base(
            CompositionServices.GetContractNameFromImportDescription(member, importDescription),
            CompositionServices.GetTypeIdentityFromImportDescription(member, importDescription),
            CompositionServices.GetMetadataFromImportDescription(member, importDescription),
            GetCardinality(importDescription.AllowDefault),
            importDescription.Recomposable,
            importDescription.Prerequisite,
            creationPolicy)
        {
            this.AllowDefault = importDescription.AllowDefault;
            this.member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public bool AllowDefault { get; protected set; }

        private static ImportCardinality GetCardinality(bool allowDefault)
        {
            return allowDefault ?
                   ImportCardinality.ZeroOrOne :
                   ImportCardinality.ExactlyOne;
        }

        public MemberInfo Member
        {
            get { return this.member.Value; }
        }

        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{0} (ContractName=\"{1}\")",
                this.member.IsValueCreated ? this.member.Value.Name : string.Empty,
                this.ContractName);
        }
    }
}
