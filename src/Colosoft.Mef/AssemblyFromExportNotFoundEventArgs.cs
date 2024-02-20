using System;

namespace Colosoft.Mef
{
    public class AssemblyFromExportNotFoundEventArgs : EventArgs
    {
        public Reflection.Composition.IExport Export { get; set; }

        public System.Reflection.AssemblyName AssemblyName { get; set; }

        public Exception Error { get; set; }

        public bool IsErrorHandled { get; set; }

        public AssemblyFromExportNotFoundEventArgs(
            Reflection.Composition.IExport export,
            System.Reflection.AssemblyName assemblyName,
            Exception error)
        {
            this.Export = export;
            this.AssemblyName = assemblyName;
            this.Error = error;
        }
    }

    public delegate void AssemblyFromExportNotFoundHandler(object sender, AssemblyFromExportNotFoundEventArgs e);
}
