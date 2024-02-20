using System;
using System.Collections.Generic;

namespace Colosoft.Mef
{
    internal class ImportDescription
    {
        public bool AllowDefault { get; set; }

        public string ContractName { get; set; }

        public Reflection.TypeName ContractTypeName { get; set; }

        public Type ContractType { get; set; }

        public string MemberName { get; set; }

        public bool Prerequisite { get; set; }

        public bool Recomposable { get; set; }

        public ICollection<string> RequiredMetadata { get; set; }
    }
}
