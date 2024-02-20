using System;

namespace Colosoft.Mef
{
    public class FailOnLoadTypeEventArgs : EventArgs
    {
        public Reflection.TypeName Type { get; set; }

        public Exception Error { get; set; }

        public FailOnLoadTypeEventArgs(Reflection.TypeName type, Exception exception)
        {
            this.Type = type;
            this.Error = exception;
        }
    }

    public delegate void FailOnLoadTypeHandler(object sender, FailOnLoadTypeEventArgs e);
}
