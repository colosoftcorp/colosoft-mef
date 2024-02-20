using System.Collections.Generic;

namespace Colosoft.Mef
{
    internal class PartDescription
    {
        public PartDescription()
            : this(null, null, null, false, null)
        {
        }

        public PartDescription(
            Reflection.TypeName typeName,
            IEnumerable<ExportDescription> exports,
            IEnumerable<ImportDescription> imports,
            bool useDispatcher,
            ImportingConstructorDescription importingConstruct = null,
            System.ComponentModel.Composition.CreationPolicy partCreationPolicy = System.ComponentModel.Composition.CreationPolicy.Shared)
        {
            this.UseDispatcher = useDispatcher;
            this.Exports = exports;
            this.Imports = imports;
            this.ImportingConstructor = importingConstruct;
            this.TypeName = typeName;
            this.PartCreationPolicy = partCreationPolicy;
        }

        public IEnumerable<ExportDescription> Exports { get; set; }

        public IEnumerable<ImportDescription> Imports { get; set; }

        public ImportingConstructorDescription ImportingConstructor { get; set; }

        public Colosoft.Reflection.TypeName TypeName { get; set; }

        public System.ComponentModel.Composition.CreationPolicy PartCreationPolicy { get; set; }

        public bool UseDispatcher { get; set; }

        public override string ToString()
        {
            if (this.TypeName != null)
            {
                return $"{this.TypeName.FullName}, {this.TypeName.AssemblyName.Name}";
            }

            return base.ToString();
        }
    }
}
