namespace Colosoft.Mef
{
    public class AssemblyRepositoryCatalogConflict
    {
        public string ContractName { get; set; }

        public Reflection.TypeName ContractType { get; set; }

        public AssemblyRepositoryCatalogConflict(string contractName, Reflection.TypeName contractType)
        {
            this.ContractName = contractName;
            this.ContractType = contractType;
        }
    }
}
