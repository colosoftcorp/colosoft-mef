using System;
using System.ComponentModel.Composition.Primitives;

namespace Colosoft.Mef
{
    public class ProviderExportDefinition : ExportDefinition
    {
        private readonly Func<System.Reflection.MemberInfo> memberGetter;

        public ProviderExportDefinition(
            Func<System.Reflection.MemberInfo> memberGetter,
            ExportDescription exportDescription,
            System.ComponentModel.Composition.CreationPolicy creationPolicy)
            : base(
            CompositionServices.GetContractNameFromExportDescription(memberGetter, exportDescription),
            CompositionServices.GetMetadataFromExportDescription(memberGetter, exportDescription, creationPolicy))
        {
            this.memberGetter = memberGetter ?? throw new ArgumentNullException(nameof(memberGetter));
        }

        public System.Reflection.MemberInfo Member
        {
            get
            {
                return this.memberGetter();
            }
        }

        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{0} (ContractName=\"{1}\")",
                this.Member != null ? this.Member.Name : string.Empty,
                this.ContractName);
        }
    }
}
