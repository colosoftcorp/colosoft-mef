using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Colosoft.Mef
{
    public class AssemblyRepositoryCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
    {
        private readonly Lazy<Reflection.IAssemblyRepository> assemblyRepository;
        private readonly Lazy<Reflection.Composition.IExportManager> exportManager;
        private readonly string uiContext;
        private readonly AggregateAssemblyRepositoryCatalogObserver observer;
        private readonly List<ContractInfo> contracts = new List<ContractInfo>();
        private readonly object objLock = new object();
        private AggregateCatalog catalogCollection;
        private volatile bool isDisposed;

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        public event FailOnLoadTypeHandler FailOnLoadType;

        public event FailOnLoadAssemblyHandler FailOnLoadAssembly;

        public event FailOnLoadPackagesHandler FailOnLoadPackages;

        public event AssemblyFromExportNotFoundHandler AssemblyFromExportNotFound;

        protected Reflection.IAssemblyRepository AssemblyRepository
        {
            get
            {
                var repos = this.assemblyRepository.Value;

                if (repos == null)
                {
                    throw new InvalidOperationException("AssemblyRepository undefined.");
                }

                return repos;
            }
        }

        public Reflection.Composition.IExportManager ExportManager
        {
            get
            {
                var manager = this.exportManager.Value;

                if (manager == null)
                {
                    throw new InvalidOperationException("IExportManager undefined.");
                }

                return manager;
            }
        }

        public AggregateAssemblyRepositoryCatalogObserver Observer => this.observer;

        public AssemblyRepositoryCatalog(
            Lazy<Reflection.IAssemblyRepository> assemblyRepository,
            Lazy<Reflection.Composition.IExportManager> exportManager,
            string uiContext)
        {
            this.assemblyRepository = assemblyRepository ?? throw new ArgumentNullException(nameof(assemblyRepository));
            this.exportManager = exportManager ?? throw new ArgumentNullException(nameof(exportManager));
            this.uiContext = uiContext;

            this.observer = new AggregateAssemblyRepositoryCatalogObserver();
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
        }

        protected virtual void OnFailLoadType(Colosoft.Reflection.TypeName type, Exception exception)
        {
            var e = new FailOnLoadTypeEventArgs(type, exception);
            this.FailOnLoadType?.Invoke(this, e);

            this.observer.FailOnLoadType(e);
        }

        protected virtual void OnFailLoadAssembly(System.Reflection.AssemblyName name, Exception exception)
        {
            var e = new FailOnLoadAssemblyEventArgs(name, exception);
            this.FailOnLoadAssembly?.Invoke(this, e);

            this.observer.FailOnLoadAssembly(e);
        }

        protected virtual void OnFailLoadPackages(Colosoft.Reflection.AssemblyPart[] assemblyParts, Exception error)
        {
            var e = new FailOnLoadPackagesEventArgs(assemblyParts, error);
            this.FailOnLoadPackages?.Invoke(this, e);

            this.observer.FailOnLoadPackages(e);
        }

        protected virtual void OnAssemblyFromExportNotFound(Colosoft.Reflection.Composition.IExport export, System.Reflection.AssemblyName assemblyName, Exception exception)
        {
            if (this.AssemblyFromExportNotFound != null)
            {
                var args = new AssemblyFromExportNotFoundEventArgs(export, assemblyName, exception);

                this.AssemblyFromExportNotFound(this, args);
                this.observer.AssemblyFromExportNotFound(args);

                if (args.IsErrorHandled)
                {
                    return;
                }
            }

            throw new InvalidOperationException(
                ResourceMessageFormatter.Create(
                    () => Properties.Resources.InvalidOperation_AssemblyFromExportNotFound,
                    assemblyName,
                    export).Format(),
                exception);
        }

        protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }

        protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            this.Changing?.Invoke(this, e);
        }

        private void DiscoverTypes(IEnumerable<ContractInfo> contracts)
        {
            lock (this.objLock)
            {
                if (this.catalogCollection == null)
                {
                    this.catalogCollection = new AggregateCatalog();
                }

                var exports = new List<Tuple<ContractInfo, Colosoft.Reflection.Composition.IExport>>();

                foreach (var i in contracts)
                {
                    // Verifica se não existe nenhum tipo associado com o contrato
                    if (i.Types.Count == 0)
                    {
                        // Cria uma tupla com com os dados do contrato e o export associado
                        var tuple = new Tuple<ContractInfo, Reflection.Composition.IExport>(
                            i,
                            this.ExportManager.GetExport(i.ContractType, i.ContractName, this.uiContext));

                        if (tuple.Item2 != null)
                        {
                            // Recupera o export do contrato
                            exports.Add(tuple);
                        }
                    }
                    else
                    {
                        // Recupera os exports do contrato
                        var contractExports = this.ExportManager.GetExports(i.ContractType, i.ContractName, this.uiContext);
                        int contractCount = 0;

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                        foreach (var j in contractExports)
                        {
                            if (i.Types.Contains(j.Type, Reflection.TypeNameEqualityComparer.Instance))
                            {
                                contractCount++;
                                var tuple = new Tuple<ContractInfo, Reflection.Composition.IExport>(i, j);
                                exports.Add(tuple);
                            }
                        }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions

                    }
                }

                if (exports.Count > 0)
                {
                    var pkgParts = new List<Reflection.AssemblyPart>();
                    var partDescriptions = new List<PartDescription>();

                    // Pecorre as exportações
                    foreach (var i in exports)
                    {
                        // Recupera as partes de assembly do exports
                        var assemblyPart = new Reflection.AssemblyPart($"{i.Item2.Type.AssemblyName.Name}.dll");

                        // Verifica se a parte do pacote já não foi carregada
                        if (!pkgParts.Exists(f => StringComparer.InvariantCultureIgnoreCase.Equals(f.Source, assemblyPart.Source)))
                        {
                            pkgParts.Add(assemblyPart);
                        }

                        System.ComponentModel.Composition.CreationPolicy creationPolicy = System.ComponentModel.Composition.CreationPolicy.Any;

                        // Verifica política de criação
                        switch (i.Item2.CreationPolicy)
                        {
                            case Reflection.Composition.CreationPolicy.NonShared:
                                creationPolicy = System.ComponentModel.Composition.CreationPolicy.NonShared;
                                break;
                            case Reflection.Composition.CreationPolicy.Shared:
                                creationPolicy = System.ComponentModel.Composition.CreationPolicy.Shared;
                                break;
                        }

                        ImportingConstructorDescription importingConstructor = null;

                        if (i.Item2.ImportingConstructor)
                        {
                            importingConstructor = new ImportingConstructorDescription(i.Item2.Type, null);
                        }

                        var exportsPartDescription = new[]
                        {
                            new ExportDescription
                            {
                                ContractTypeName = i.Item1.ContractType,
                                ContractName = string.IsNullOrEmpty(i.Item2.ContractName) ? null : i.Item2.ContractName.Trim(),
                                Metadata = i.Item2.Metadata,
                            },
                        };

                        // Cria a descrição da parte
                        var partDescription = new PartDescription(
                            i.Item2.Type,
                            exportsPartDescription,
                            Array.Empty<ImportDescription>(),
                            i.Item2.UseDispatcher,
                            importingConstructor,
                            creationPolicy);

                        partDescriptions.Add(partDescription);
                    }

                    // Cria o catalogo que será usado para carregar as partes dos assemblies
#pragma warning disable CA2000 // Dispose objects before losing scope
                    var catalog = new DefinitionCatalog(new DiscoverAssembliesContainer(this, this.assemblyRepository, pkgParts.ToArray(), exports), partDescriptions);
#pragma warning restore CA2000 // Dispose objects before losing scope

                    var addedDefinitions = catalog.Parts;

                    // Cria um instancia para realizar a composição
                    using (AtomicComposition composition = new AtomicComposition())
                    {
                        var args = new ComposablePartCatalogChangeEventArgs(addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), composition);
                        this.OnChanging(args);

                        this.catalogCollection.Catalogs.Add(catalog);

                        composition.Complete();
                    }

                    ComposablePartCatalogChangeEventArgs e = new ComposablePartCatalogChangeEventArgs(addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), null);
                    this.OnChanged(e);
                }
            }
        }

        protected void Initialized()
        {
            var exportManager1 = this.exportManager.Value;
            var exports = exportManager1.GetExports(this.uiContext);
            var groups = new Dictionary<string, List<Reflection.Composition.IExport>>();

            foreach (var export in exports)
            {
                string typeAssembly = export.Type.AssemblyName.FullName;

                if (!groups.ContainsKey(typeAssembly))
                {
                    var list = new List<Reflection.Composition.IExport>();
                    list.Add(export);

                    groups.Add(typeAssembly, list);
                }
                else
                {
                    var list = groups[typeAssembly];

                    var index = list.BinarySearch(export, Reflection.Composition.ExportComparer.Instance);

                    if (index < 0)
                    {
                        // Adiciona o export na lista
                        list.Insert(~index, export);
                    }
                    else
                    {
                        var item = list[index];
                        if (string.IsNullOrEmpty(item.ContractName) &&
                            string.IsNullOrEmpty(export.ContractName))
                        {
                            list.Insert(index, export);
                        }
                        else
                        {
                            throw new AssemblyRepositoryCatalogException(
                                ResourceMessageFormatter.Create(
                                    () => Properties.Resources.AssemblyRepositoryCatalog_DuplicateExport,
                                    export.ContractType,
                                    export.ContractName).Format());
                        }
                    }
                }
            }

            foreach (var group in groups)
            {
                var catalogRegister = new AssemblyRepositoryCatalogRegister();

                foreach (var export in group.Value)
                {
                    catalogRegister.Add(export.ContractType, export.ContractName, export.Type);
                }

                var conflict = this.Add(catalogRegister).FirstOrDefault();

                if (conflict != null)
                {
                    throw new AssemblyRepositoryCatalogException(
                        ResourceMessageFormatter.Create(
                            () => Properties.Resources.AssemblyRepositoryCatalog_DuplicateExport,
                            conflict.ContractType,
                            conflict.ContractName).Format());
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposed = true;
            }

            this.catalogCollection.Dispose();

            base.Dispose(disposing);
        }

        public AssemblyRepositoryCatalogConflict[] Add(AssemblyRepositoryCatalogRegister register)
        {
            var contractsTemp = new List<ContractInfo>();
            var conflicts = new List<AssemblyRepositoryCatalogConflict>();

#pragma warning disable CA1062 // Validate arguments of public methods
            foreach (var i in register.Contracts)
            {
                var index = this.contracts.FindIndex(f => ContractInfoEqualityComparer.Instance.Equals(f, i));

                if (index < 0)
                {
                    contractsTemp.Add(i);
                    this.contracts.Add(i);
                }
                else
                {
                    conflicts.Add(new AssemblyRepositoryCatalogConflict(i.ContractName, i.ContractType));
                }
            }
#pragma warning restore CA1062 // Validate arguments of public methods

            if (this.catalogCollection != null && contractsTemp.Count > 0)
            {
                this.DiscoverTypes(contractsTemp);
            }

            return conflicts.ToArray();
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                this.ThrowIfDisposed();

                if (this.catalogCollection == null)
                {
                    this.DiscoverTypes(this.contracts);
                    this.Initialized();
                }

                return this.catalogCollection.Parts;
            }
        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            var exports = base.GetExports(definition).ToList();
            return exports;
        }

        internal sealed class ContractInfo
        {
            public Reflection.TypeName ContractType { get; private set; }

            public string ContractName { get; private set; }

            public List<Reflection.TypeName> Types { get; } = new List<Reflection.TypeName>();

            public ContractInfo(
                Reflection.TypeName contractTypeName,
                string contractName,
                Reflection.TypeName type)
            {
                this.ContractName = contractName;
                this.ContractType = contractTypeName;
                if (type != null)
                {
                    this.Types.Add(type);
                }
            }

            public override string ToString()
            {
                return $"[ContractName: {this.ContractName}, ContractType: {this.ContractType}]";
            }
        }

        private sealed class ContractInfoEqualityComparer : IEqualityComparer<ContractInfo>
        {
            public static readonly ContractInfoEqualityComparer Instance = new ContractInfoEqualityComparer();

            public bool Equals(ContractInfo x, ContractInfo y)
            {
                return (x == null && y == null) ||
                        (x != null && y != null &&
                        x.ContractName == y.ContractName &&
                        Reflection.TypeNameEqualityComparer.Instance.Equals(x.ContractType, y.ContractType) &&
                        (x.Types.Count == 0 || y.Types.Count == 0 || x.Types.Intersect(y.Types, Reflection.TypeNameEqualityComparer.Instance).Any()));
            }

            public int GetHashCode(ContractInfo obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return $"[{obj.ContractType} : {obj.ContractName}]".GetHashCode();
            }
        }

        private sealed class DiscoverAssembliesContainer : IAssemblyContainer
        {
            private readonly AssemblyRepositoryCatalog assemblyRepositoryCatalog;
            private readonly Reflection.AssemblyPart[] packageParts;
            private readonly IEnumerable<Tuple<ContractInfo, Reflection.Composition.IExport>> exports;
            private readonly Lazy<Reflection.IAssemblyRepository> assemblyRepository;
            private readonly object objLock = new object();
            private Dictionary<string, System.Reflection.Assembly> assemblies;
            private bool isInitializing;

            private Dictionary<string, System.Reflection.Assembly> Assemblies
            {
                get
                {
                    if (this.isInitializing || (this.assemblies == null || this.assemblies.Count == 0))
                    {
                        lock (this.objLock)
                            if (!this.isInitializing || this.assemblies == null || this.assemblies.Count == 0)
                            {
                                this.isInitializing = true;
                                this.Initialize();
                            }
                    }

                    return this.assemblies;
                }
            }

            public DiscoverAssembliesContainer(
                AssemblyRepositoryCatalog assemblyRepositoryCatalog,
                Lazy<Reflection.IAssemblyRepository> assemblyRepository,
                Reflection.AssemblyPart[] packageParts,
                IEnumerable<Tuple<ContractInfo, Reflection.Composition.IExport>> exports)
            {
                this.assemblyRepositoryCatalog = assemblyRepositoryCatalog;
                this.assemblyRepository = assemblyRepository;
                this.packageParts = packageParts;
                this.exports = exports;
            }

            private void Initialize()
            {
                this.assemblies = new Dictionary<string, System.Reflection.Assembly>(StringComparer.InvariantCultureIgnoreCase);
                var assemblyParts = new List<Reflection.AssemblyPart>(this.packageParts);

                var resolverManager = this.assemblyRepository.Value.AssemblyResolverManager;

                for (var i = 0; i < assemblyParts.Count; i++)
                {
                    var assemblyName = System.IO.Path.GetFileNameWithoutExtension(assemblyParts[i].Source);

                    if (resolverManager.CheckAssembly(assemblyParts[i].Source))
                    {
                        try
                        {
                            var asm = resolverManager.AppDomain.Load(assemblyName);

                            if (asm != null)
                            {
                                if (!this.assemblies.ContainsKey(assemblyName))
                                {
                                    this.assemblies.Add(assemblyName, asm);
                                }

                                assemblyParts.RemoveAt(i--);
                            }
                        }
                        catch (Exception ex)
                        {
                            Exception exception = ex;

                            if (exception is AggregateException aggregateException)
                            {
                                exception = aggregateException.InnerException;
                            }

                            if (exception is System.Reflection.ReflectionTypeLoadException)
                            {
                                ex = exception;
                            }

                            this.assemblyRepositoryCatalog.OnFailLoadAssembly(new System.Reflection.AssemblyName(assemblyParts[i].Source), ex);
                        }
                    }
                }

                using (var resolver = new Colosoft.Reflection.AssemblyPartsResolver(this.assemblyRepository.Value, assemblyParts))
                {
                    resolverManager.Insert(0, resolver);
                    try
                    {
                        foreach (var assemblyName in assemblyParts.Select(f => f.Source))
                        {
                            System.Reflection.Assembly assembly = null;

                            try
                            {
                                assembly = resolverManager.AppDomain.Load(new System.Reflection.AssemblyName(assemblyName));
                            }
                            catch (Exception ex)
                            {
                                if (ex.InnerException is Reflection.AssemblyResolverException)
                                {
                                    ex = ex.InnerException;
                                }

                                if (ex.InnerException is IO.Xap.LoadXapPackageAssembliesException)
                                {
                                    ex = ex.InnerException;
                                }

                                Exception exception = ex;

                                if (exception is AggregateException aggregateException)
                                {
                                    exception = aggregateException.InnerException;
                                }

                                if (exception is System.Reflection.ReflectionTypeLoadException)
                                {
                                    ex = exception;
                                }

                                this.assemblyRepositoryCatalog.OnFailLoadAssembly(new System.Reflection.AssemblyName(assemblyName), ex);

                                continue;
                            }

                            this.assemblies.Add(assembly.GetName().Name, assembly);
                        }
                    }
                    finally
                    {
                        resolverManager.Remove(resolver);
                    }
                }

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                foreach (var i in this.exports)
                {
                    System.Reflection.Assembly assembly = null;

                    if (!this.assemblies.TryGetValue(i.Item2.Type.AssemblyName.Name, out assembly))
                    {
                        Exception lastException = null;

                        try
                        {
                            // Tenta carrega o assembly do domínio
                            assembly = resolverManager.AppDomain.Load(i.Item2.Type.AssemblyName.Name);
                        }
                        catch (Exception ex)
                        {
                            lastException = ex;
                        }

                        if (assembly == null)
                        {
                            this.assemblyRepositoryCatalog.OnAssemblyFromExportNotFound(i.Item2, i.Item2.Type.AssemblyName, lastException);
                            continue;
                        }
                    }

                    Type partType = null;

                    try
                    {
                        partType = assembly.GetType(i.Item2.Type.FullName, true);
                    }
                    catch (Exception ex)
                    {
                        this.assemblyRepositoryCatalog.OnFailLoadType(i.Item2.Type, ex);
                        continue;
                    }

                    if (partType == null)
                    {
                        this.assemblyRepositoryCatalog.OnFailLoadType(i.Item2.Type, null);
                    }
                }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
            }

            public bool TryGet(string assemblyName, out System.Reflection.Assembly assembly)
            {
                return this.Assemblies.TryGetValue(assemblyName, out assembly);
            }

            public bool TryGet(string assemblyName, out System.Reflection.Assembly assembly, out Exception exception)
            {
                exception = null;
                return this.Assemblies.TryGetValue(assemblyName, out assembly);
            }

            public Colosoft.Reflection.AssemblyLoaderGetResult Get(string[] assemblyNames)
            {
                var result = new List<Colosoft.Reflection.AssemblyLoaderGetResult.Entry>();

                foreach (var assemblyName in assemblyNames)
                {
                    System.Reflection.Assembly assembly = null;
                    Exception exception = null;

                    var getResult = this.TryGet(assemblyName, out assembly, out exception);
                    result.Add(new Reflection.AssemblyLoaderGetResult.Entry(assemblyName, assembly, getResult, exception));
                }

                return new Reflection.AssemblyLoaderGetResult(result);
            }

            public IEnumerator<System.Reflection.Assembly> GetEnumerator()
            {
                return this.Assemblies.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.Assemblies.Values.GetEnumerator();
            }
        }
    }
}
