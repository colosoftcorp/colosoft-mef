namespace Colosoft.Mef
{
    public interface IAssemblyRepositoryCatalogObserver
    {
        void FailOnLoadType(FailOnLoadTypeEventArgs e);

        void FailOnLoadAssembly(FailOnLoadAssemblyEventArgs e);

        void FailOnLoadPackages(FailOnLoadPackagesEventArgs e);

        void AssemblyFromExportNotFound(AssemblyFromExportNotFoundEventArgs e);
    }
}
