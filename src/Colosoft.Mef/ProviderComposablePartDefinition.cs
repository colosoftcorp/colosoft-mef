using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;

namespace Colosoft.Mef
{
    internal class ProviderComposablePartDefinition : ComposablePartDefinition
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IEnumerable<ExportDefinition> exports;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Lazy<IEnumerable<ImportDefinition>> imports;

        private readonly ProviderParameterImportDefinitionContainer parameterImportDefinitions;

        private readonly Reflection.TypeName typeName;

        private ProviderComposablePart composablePart;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IEnumerable<ProviderParameterImportDefinition> importsParameters;

        public ComposablePartCatalog Catalog { get; private set; }

        public System.ComponentModel.Composition.CreationPolicy PartCreationPolicy { get; private set; }

        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            [DebuggerStepThrough]
            get { return this.exports; }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            [DebuggerStepThrough]
            get { return this.imports.Value; }
        }

        public IEnumerable<ProviderParameterImportDefinition> ImportsParametersDefinitions
        {
            get
            {
                if (this.importsParameters == null)
                {
                    this.importsParameters = this.parameterImportDefinitions.GetImportDefinitions();
                }

                return this.importsParameters;
            }
        }

        public bool UseDispatcher { get; private set; }

        public ProviderComposablePartDefinition(
            ComposablePartCatalog catalog,
            Reflection.TypeName typeName,
            IList<ProviderExportDefinition> exportDefinitions,
            Lazy<IEnumerable<ImportDefinition>> importDefinitions,
            ProviderParameterImportDefinitionContainer parameterImportDefinitions,
            System.ComponentModel.Composition.CreationPolicy partCreationPolicy,
            bool useDispatcher)
        {
            this.exports = exportDefinitions.Cast<ExportDefinition>().ToArray();
            this.imports = importDefinitions;
            this.parameterImportDefinitions = parameterImportDefinitions;

            this.Catalog = catalog;
            this.typeName = typeName;
            this.PartCreationPolicy = partCreationPolicy;
            this.UseDispatcher = useDispatcher;
        }

        public override ComposablePart CreatePart()
        {
            if (this.composablePart == null)
            {
                this.composablePart = new ProviderComposablePart(this);
            }

            return this.composablePart;
        }

        public override string ToString()
        {
            return $"{this.typeName.FullName}, {this.typeName.AssemblyName.Name}";
        }
    }
}
