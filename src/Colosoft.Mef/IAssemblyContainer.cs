using Colosoft.Reflection;
using System.Collections.Generic;

namespace Colosoft.Mef
{
    internal interface IAssemblyContainer : IEnumerable<System.Reflection.Assembly>, IAssemblyLoader
    {
    }
}
