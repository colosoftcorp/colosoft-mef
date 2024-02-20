using System;

namespace Colosoft.Mef
{
    public class FailOnLoadAssemblyEventArgs : EventArgs
    {
        public System.Reflection.AssemblyName AssemblyName { get; set; }

        public Exception Error { get; set; }

        public FailOnLoadAssemblyEventArgs(System.Reflection.AssemblyName assemblyName, Exception error)
        {
            this.AssemblyName = assemblyName;
            this.Error = error;
        }
    }

    public delegate void FailOnLoadAssemblyHandler(object sender, FailOnLoadAssemblyEventArgs e);
}
