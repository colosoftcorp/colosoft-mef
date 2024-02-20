using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Mef
{
    internal class ProviderComposablePart : System.ComponentModel.Composition.Primitives.ComposablePart
    {
        public ProviderComposablePart(ProviderComposablePartDefinition definition)
            : this(null, definition)
        {
        }

        public ProviderComposablePart(object instance)
            : this(instance, null)
        {
        }

        private ProviderComposablePart(object instance, ProviderComposablePartDefinition definition)
        {
            this.Definition = definition;

            this.ImportedValues = new Dictionary<System.ComponentModel.Composition.Primitives.ImportDefinition, ImportableInfo>();
            this.Instance = instance;
        }

        public ProviderComposablePartDefinition Definition { get; private set; }

        public override IEnumerable<System.ComponentModel.Composition.Primitives.ExportDefinition> ExportDefinitions
        {
            get { return this.Definition.ExportDefinitions; }
        }

        public override IEnumerable<System.ComponentModel.Composition.Primitives.ImportDefinition> ImportDefinitions
        {
            get
            {
                var imports = this.Definition.ImportDefinitions.Concat(this.Definition.ImportsParametersDefinitions).ToArray();
                return imports;
            }
        }

        private Dictionary<System.ComponentModel.Composition.Primitives.ImportDefinition, ImportableInfo> ImportedValues { get; set; }

        private object Instance { get; set; }

        private bool IsComposed { get; set; }

        private object GetActivatedInstance(ComposableMember exportable)
        {
            if (this.Definition.PartCreationPolicy == System.ComponentModel.Composition.CreationPolicy.Shared &&
                this.Instance != null)
            {
                return this.Instance;
            }

            var constructor = exportable as ComposableConstructor;

            if (constructor == null)
            {
                try
                {
                    if (this.Definition.UseDispatcher &&
                        Threading.DispatcherManager.Dispatcher != null &&
                        Threading.DispatcherManager.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                    {
                        this.Instance = Threading.DispatcherManager.Dispatcher
                            .Invoke(new Func<Type, object>(Activator.CreateInstance), exportable.DeclaringType);
                    }
                    else
                    {
                        this.Instance = Activator.CreateInstance(exportable.DeclaringType);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.Reflection.TargetInvocationException && ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    throw new ActivatedInstanceException($"An error ocurred when create instance for type '{exportable.DeclaringType}'. \r\nMessage: {ex.Message}", ex);
                }
            }
            else
            {
                var ctorParameters = constructor.Constructor.GetParameters();
                var parameters = new object[ctorParameters.Length];

                var i = 0;
                foreach (var j in this.Definition.ImportsParametersDefinitions)
                {
                    var value = this.ImportedValues[j];
                    value.ClearExportValue();
                    try
                    {
                        parameters[i] = value.GetValue();
                    }
                    catch (Exception ex)
                    {
                        throw new ComposableMemberParameterException(exportable, j, ex);
                    }

                    i++;
                }

                try
                {
                    if (this.Definition.UseDispatcher &&
                        Threading.DispatcherManager.Dispatcher != null &&
                        Threading.DispatcherManager.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                    {
                        var declaringType = exportable.DeclaringType;

                        this.Instance = Colosoft.Threading.DispatcherManager.Dispatcher
                            .Invoke(new Func<Type, object[], object>(Activator.CreateInstance), declaringType, parameters);
                    }
                    else
                    {
                        this.Instance = constructor.Constructor.Invoke(parameters);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.Reflection.TargetInvocationException && ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    throw new ActivatedInstanceException($"An error ocurred when create instance for type '{exportable.DeclaringType}'. \r\nMessage: {ex.Message}", ex);
                }
            }

            if (this.IsComposed)
            {
                this.SatisfyPostCompositionImports();
            }

            return this.Instance;
        }

        public override object GetExportedValue(System.ComponentModel.Composition.Primitives.ExportDefinition definition)
        {
            try
            {
                var export = definition as ProviderExportDefinition;

                if (export == null)
                {
                    throw new InvalidOperationException("The supplied export definition was of an unknown type.");
                }

                if (export.Member == null)
                {
                    throw new InvalidOperationException($"Not found member to export definition. {export.ContractName}");
                }

                ComposableMember exportable = export.Member.ToComposableMember();

                object instance = null;

                if (exportable.IsInstanceNeeded)
                {
                    instance = this.GetActivatedInstance(exportable);
                }

                var value = exportable.GetValue(instance);
                return value;
            }
            catch (Exception ex)
            {
                ComposablePartErrorHandler.NotifyGetExportedValueError(definition, ex);
                throw;
            }
        }

        public override void Activate()
        {
            this.OnComposed();
        }

        public void OnComposed()
        {
            if (this.Instance != null && !this.IsComposed)
            {
                this.SatisfyPostCompositionImports();
            }

            this.IsComposed = true;
        }

        private void SatisfyPostCompositionImports()
        {
            IEnumerable<System.ComponentModel.Composition.Primitives.ImportDefinition> members =
                this.ImportDefinitions.Where(import => !import.IsPrerequisite);

            foreach (var i in members)
            {
                var definition = i as ProviderImportDefinition;

                if (definition == null)
                {
                    continue;
                }

                ImportableInfo value;
                if (this.ImportedValues.TryGetValue(definition, out value))
                {
                    ComposableMember importable = definition.Member.ToComposableMember();

                    importable.SetValue(this.Instance, value.GetValue());
                }
            }
        }

        public override void SetImport(
            System.ComponentModel.Composition.Primitives.ImportDefinition definition,
            IEnumerable<System.ComponentModel.Composition.Primitives.Export> exports)
        {
            var import = definition as ProviderImportDefinition;

            this.ImportedValues[definition] = new ImportableInfo(definition, import, exports, this);
        }

        private sealed class ImportableInfo
        {
            private readonly System.ComponentModel.Composition.Primitives.ImportDefinition definition;
            private readonly ProviderImportDefinition import;
            private readonly IEnumerable<System.ComponentModel.Composition.Primitives.Export> exports;
            private readonly ProviderComposablePart composablePart;

            public ImportableInfo(
                System.ComponentModel.Composition.Primitives.ImportDefinition definition,
                ProviderImportDefinition import,
                IEnumerable<System.ComponentModel.Composition.Primitives.Export> exports,
                ProviderComposablePart composablePart)
            {
                this.definition = definition;
                this.import = import;
                this.exports = exports;
                this.composablePart = composablePart;
            }

            public void ClearExportValue()
            {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                var field = typeof(System.ComponentModel.Composition.Primitives.Export).GetField("_exportedValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var emptyValue = typeof(System.ComponentModel.Composition.Primitives.Export).GetField("_EmptyValue", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

                foreach (var export in this.exports)
                {
                    field.SetValue(export, emptyValue);
                }
            }

            public object GetValue()
            {
                ComposableMember importable = null;

                if (this.import == null)
                {
                    if (this.definition is ProviderParameterImportDefinition providerParameterImportDefinition)
                    {
                        importable = providerParameterImportDefinition.Parameter.ToComposableMember();
                    }
                    else
                    {
                        throw new InvalidOperationException("The supplied import definition was of an unknown type.");
                    }
                }
                else
                {
                    importable = this.import.Member.ToComposableMember();
                }

                var value = importable.GetImportValueFromExports(this.exports);

                return value;
            }
        }
    }
}
