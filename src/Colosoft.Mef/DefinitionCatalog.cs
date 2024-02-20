using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;

namespace Colosoft.Mef
{
    internal class DefinitionCatalog : ComposablePartCatalog
    {
        private readonly IEnumerable<PartDescription> partDescriptions;
        private readonly IAssemblyContainer assemblies;
        private IQueryable<ComposablePartDefinition> parts;

        public override IQueryable<ComposablePartDefinition> Parts
        {
            [DebuggerStepThrough]
            get
            {
                if (this.parts == null)
                {
                    this.parts = this.GetComposablePartDefinitions().Cast<ComposablePartDefinition>().AsQueryable();
                }

                return this.parts;
            }
        }

        public DefinitionCatalog(IAssemblyContainer assemblies, IEnumerable<PartDescription> partDescriptions)
        {
            this.assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            this.partDescriptions = partDescriptions ?? throw new ArgumentNullException(nameof(partDescriptions));
        }

        private List<ProviderComposablePartDefinition> GetComposablePartDefinitions()
        {
            var definitions = new List<ProviderComposablePartDefinition>();

            foreach (PartDescription part in this.partDescriptions)
            {
                var container = new ProviderParameterImportDefinitionContainer(part, this.assemblies);
                definitions.Add(
                    new ProviderComposablePartDefinition(
                        this,
                        part.TypeName,
                        this.GetExportDefinitions(part),
                        this.GetImportDefinitions(part, container),
                        container,
                        part.PartCreationPolicy,
                        part.UseDispatcher));
            }

            return definitions;
        }

        private IList<ProviderExportDefinition> GetExportDefinitions(PartDescription part)
        {
            var exports = new List<ProviderExportDefinition>();

            foreach (var i in part.Exports)
            {
                ExportDescription export = i;
                var getter = new ProviderExportDefinitionMemberGetter(this, part, export);
                exports.Add(new ProviderExportDefinition(getter.GetMember, export, part.PartCreationPolicy));
            }

            return exports;
        }

        private IList<ProviderParameterImportDefinition> GetParametersImportDefinitions(PartDescription part)
        {
            var imports = new List<ProviderParameterImportDefinition>();

            if (part.ImportingConstructor != null)
            {
                foreach (var i in part.ImportingConstructor.GetParameterImportDefinitions(this.assemblies))
                {
                    imports.Add(new ProviderParameterImportDefinition(i.Item2, i.Item1, System.ComponentModel.Composition.CreationPolicy.Any));
                }
            }

            return imports;
        }

        private Lazy<IEnumerable<ImportDefinition>> GetImportDefinitions(
            PartDescription part,
            ProviderParameterImportDefinitionContainer providerParameterImportContainer)
        {
            return new Lazy<IEnumerable<ImportDefinition>>(() =>
            {
                var imports = new List<ImportDefinition>();

                foreach (var i in part.Imports)
                {
                    ImportDescription import = i;

                    imports.Add(
                        new ProviderImportDefinition(
                            new Lazy<System.Reflection.MemberInfo>(() => this.GetMemberInfo(part.TypeName, import.MemberName), false),
                            import,
                            part.PartCreationPolicy));
                }

                foreach (var i in providerParameterImportContainer.GetImportDefinitions())
                {
                    imports.Add(i);
                }

                return imports;
            });
        }

        public System.Reflection.MemberInfo GetMemberInfo(Reflection.TypeName typeName, string memberName)
        {
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            System.Reflection.Assembly assembly = null;

            if (!this.assemblies.TryGet(typeName.AssemblyName.Name, out assembly))
            {
                throw new InvalidOperationException($"Not found assembly '{typeName.AssemblyName.Name}'");
            }

            Type inspectedType = assembly.GetType(typeName.FullName, true, true);

            return string.IsNullOrEmpty(memberName) ? inspectedType : inspectedType.GetMember(memberName).FirstOrDefault();
        }

        private sealed class ProviderExportDefinitionMemberGetter
        {
            private DefinitionCatalog catalog;
            private PartDescription part;
            private ExportDescription export;
            private System.Reflection.MemberInfo member;

            public ProviderExportDefinitionMemberGetter(
                DefinitionCatalog catalog,
                PartDescription part,
                ExportDescription export)
            {
                this.catalog = catalog;
                this.part = part;
                this.export = export;
            }

            public System.Reflection.MemberInfo GetMember()
            {
                if (this.member == null)
                {
                    if (this.part != null && this.part.ImportingConstructor != null)
                    {
                        this.member = this.part.ImportingConstructor.GetConstructor(this.catalog.assemblies);
                    }
                    else if (this.part != null)
                    {
                        this.member = this.catalog.GetMemberInfo(this.part.TypeName, this.export.MemberName);
                    }

                    if (this.member != null)
                    {
                        this.catalog = null;
                        this.part = null;
                        this.export = null;
                    }
                }

                return this.member;
            }
        }
    }
}
