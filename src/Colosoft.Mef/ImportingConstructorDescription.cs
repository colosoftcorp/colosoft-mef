using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Mef
{
    internal class ImportingConstructorDescription : ImportDescription
    {
        private readonly Reflection.TypeName typeName;
        private Type type;
        private ImportingConstructorParameterDescription[] parameters;

        public ImportingConstructorDescription(Reflection.TypeName typeName, ImportingConstructorParameterDescription[] parameters)
        {
            this.typeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            this.parameters = parameters;
        }

        private ImportingConstructorParameterDescription[] GetParameters(IAssemblyContainer assemblyContainer)
        {
            if (this.parameters == null || this.parameters.Length == 0)
            {
                var type1 = this.GetType(assemblyContainer);

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                var constructors = type1.GetConstructors(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

                // Carrega os parametros do primeiro construtor com parametros que encontrar
                foreach (var ctor in constructors)
                {
                    // Recupera os parametros do construtor
                    var ctorParameters = ctor.GetParameters();

                    // Verifica se o construtor possui parametros
                    if (ctorParameters.Length > 0)
                    {
                        this.parameters = ctorParameters.Select(f => new ImportingConstructorParameterDescription
                        {
                            Name = f.Name,
                            ContractType = f.ParameterType,
                        }).ToArray();
                    }
                }
            }

            return this.parameters;
        }

        private Type GetType(IAssemblyContainer assemblyContainer)
        {
            if (this.type == null)
            {
                if (assemblyContainer.TryGet(this.typeName.AssemblyName.Name, out var assembly))
                {
                    this.type = assembly.GetType(this.typeName.FullName, true);
                }
                else
                {
                    throw new InvalidOperationException($"Assembly '{this.typeName.AssemblyName.Name}' not found");
                }
            }

            return this.type;
        }

        public System.Reflection.ConstructorInfo GetConstructor(IAssemblyContainer assemblyContainer)
        {
            // Recupera o tipo associado
            var type1 = this.GetType(assemblyContainer);

            // Carrega os construtores para serem analizados
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var constructors = type1.GetConstructors(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

            // Carrrega os parametros esperados
            var parameters1 = this.GetParameters(assemblyContainer) ?? Array.Empty<ImportingConstructorParameterDescription>();

            foreach (var i in constructors)
            {
                var ctorParameters = i.GetParameters();

                if (ctorParameters.Length == parameters1.Length)
                {
                    var equals = true;

                    // Pecorre os parametros para verificar se são equivalentes
                    for (var j = 0; equals && j < ctorParameters.Length; j++)
                    {
                        equals = ctorParameters[j].Name == parameters1[j].Name &&
                                 (parameters1[j].ContractType == null || (parameters1[j].ContractType == ctorParameters[j].ParameterType));
                    }

                    if (equals)
                    {
                        return i;
                    }
                }
            }

            return null;
        }

        public IEnumerable<Tuple<ImportingConstructorParameterDescription, System.Reflection.ParameterInfo>> GetParameterImportDefinitions(IAssemblyContainer assemblyContainer)
        {
            var constructor = this.GetConstructor(assemblyContainer);

            if (constructor != null)
            {
                // Recupera os parametros que deverão ser utilizados
                var parameters1 = this.GetParameters(assemblyContainer);
                var ctorParameters = constructor.GetParameters();

                for (var i = 0; i < ctorParameters.Length; i++)
                {
                    yield return new Tuple<ImportingConstructorParameterDescription, System.Reflection.ParameterInfo>(parameters1[i], ctorParameters[i]);
                }
            }
        }
    }
}
