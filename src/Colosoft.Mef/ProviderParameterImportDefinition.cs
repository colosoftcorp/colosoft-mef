using System;
using System.ComponentModel.Composition.Primitives;

namespace Colosoft.Mef
{
    internal class ProviderParameterImportDefinition : ContractBasedImportDefinition
    {
        public ProviderParameterImportDefinition(
            System.Reflection.ParameterInfo parameter,
            ImportDescription importDescription,
            System.ComponentModel.Composition.CreationPolicy creationPolicy)
            : base(
                CompositionServices.GetContractNameFromImportDescription(parameter, importDescription),
                CompositionServices.GetTypeIdentityFromImportDescription(parameter, importDescription),
                CompositionServices.GetMetadataFromImportDescription(parameter, importDescription),
                ImportCardinality.ExactlyOne,
                (importDescription?.Recomposable).GetValueOrDefault(),
                (importDescription?.Prerequisite).GetValueOrDefault(),
                creationPolicy)
        {
            if (importDescription is null)
            {
                throw new ArgumentNullException(nameof(importDescription));
            }

            this.AllowDefault = importDescription.AllowDefault;
            this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        public bool AllowDefault { get; protected set; }

        public System.Reflection.ParameterInfo Parameter { get; protected set; }

        public override string ToString()
        {
            return $"{this.Parameter.Name} (ContractName=\"{this.ContractName}\")";
        }
    }
}
