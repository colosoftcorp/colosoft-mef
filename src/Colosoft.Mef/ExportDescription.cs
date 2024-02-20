using System;
using System.Collections.Generic;

namespace Colosoft.Mef
{
    public class ExportDescription
    {
        public string ContractName { get; set; }

        public Reflection.TypeName ContractTypeName { get; set; }

        public Type ContractType { get; set; }

        public string MemberName { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, object> Metadata { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
