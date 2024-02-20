namespace Colosoft.Mef
{
    public class AggregateAssemblyRepositoryCatalogObserver : AggregateObserver<IAssemblyRepositoryCatalogObserver>
    {
        public void FailOnLoadType(FailOnLoadTypeEventArgs e)
        {
            foreach (var i in this.Observers)
            {
                i.FailOnLoadType(e);
            }
        }

        public void FailOnLoadAssembly(FailOnLoadAssemblyEventArgs e)
        {
            foreach (var i in this.Observers)
            {
                i.FailOnLoadAssembly(e);
            }
        }

        public void FailOnLoadPackages(FailOnLoadPackagesEventArgs e)
        {
            foreach (var i in this.Observers)
            {
                i.FailOnLoadPackages(e);
            }
        }

        public void AssemblyFromExportNotFound(AssemblyFromExportNotFoundEventArgs e)
        {
            foreach (var i in this.Observers)
            {
                i.AssemblyFromExportNotFound(e);
            }
        }
    }
}
