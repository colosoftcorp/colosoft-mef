using System;

namespace Colosoft.Mef
{
    public class FailOnLoadPackagesEventArgs : EventArgs
    {
        public Exception Error { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        public Colosoft.Reflection.AssemblyPart[] AssemblyParts { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public FailOnLoadPackagesEventArgs(Reflection.AssemblyPart[] assemblyParts, Exception error)
        {
            this.AssemblyParts = assemblyParts;
            this.Error = error;
        }
    }

    public delegate void FailOnLoadPackagesHandler(object sender, FailOnLoadPackagesEventArgs e);
}
