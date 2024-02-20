using System.Collections.Generic;

namespace Colosoft.Mef
{
    public class AssemblyRepositoryCatalogRegister
    {
        internal List<AssemblyRepositoryCatalog.ContractInfo> Contracts { get; } = new List<AssemblyRepositoryCatalog.ContractInfo>();

        public AssemblyRepositoryCatalogRegister Add<T>()
        {
            return this.Add<T>(null);
        }

        public AssemblyRepositoryCatalogRegister Add<T>(string contractName)
        {
            var typeName = new Reflection.TypeName(typeof(T).AssemblyQualifiedName);
            return this.Add(typeName, contractName, null);
        }

        public AssemblyRepositoryCatalogRegister Add(Colosoft.Reflection.TypeName typeName)
        {
            return this.Add(typeName, null, null);
        }

        public AssemblyRepositoryCatalogRegister Add(
            Reflection.TypeName contractTypeName,
            string contractName,
            Reflection.TypeName type)
        {
            contractName = string.IsNullOrEmpty(contractName) ? null : contractName;

            var index = this.Contracts.FindIndex(
                f => f.ContractName == contractName &&
                     Reflection.TypeNameEqualityComparer.Instance.Equals(f.ContractType, contractTypeName));

            if (index < 0)
            {
                this.Contracts.Add(new AssemblyRepositoryCatalog.ContractInfo(contractTypeName, contractName, type));
            }
            else
            {
                this.Contracts[index].Types.Add(type);
            }

            return this;
        }
    }
}
