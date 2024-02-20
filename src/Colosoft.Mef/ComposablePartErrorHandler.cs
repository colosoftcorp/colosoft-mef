using System;
using System.ComponentModel.Composition.Primitives;

namespace Colosoft.Mef
{
    public static class ComposablePartErrorHandler
    {
        public static event Action<ExportDefinition, Exception> GetExportedValueError;

        internal static void NotifyGetExportedValueError(ExportDefinition definition, Exception e)
        {
            GetExportedValueError?.Invoke(definition, e);
        }
    }
}
