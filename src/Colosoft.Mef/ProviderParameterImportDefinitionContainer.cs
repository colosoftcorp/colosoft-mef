using System.Collections.Generic;

namespace Colosoft.Mef
{
    internal class ProviderParameterImportDefinitionContainer
    {
        private readonly PartDescription partDescription;
        private readonly IAssemblyContainer assemblyContainer;

        public ProviderParameterImportDefinitionContainer(PartDescription partDescription, IAssemblyContainer assemblyContainer)
        {
            this.partDescription = partDescription ?? throw new System.ArgumentNullException(nameof(partDescription));
            this.assemblyContainer = assemblyContainer ?? throw new System.ArgumentNullException(nameof(assemblyContainer));
        }

        public ProviderParameterImportDefinition[] GetImportDefinitions()
        {
            var imports = new List<ProviderParameterImportDefinition>();

            if (this.partDescription.ImportingConstructor != null)
            {
                foreach (var i in this.partDescription.ImportingConstructor.GetParameterImportDefinitions(this.assemblyContainer))
                {
                    imports.Add(new ProviderParameterImportDefinition(i.Item2, i.Item1, System.ComponentModel.Composition.CreationPolicy.Any));
                }
            }

            return imports.ToArray();
        }
    }
}
