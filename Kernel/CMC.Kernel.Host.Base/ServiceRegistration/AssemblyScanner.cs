using Microsoft.Extensions.DependencyInjection;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class AssemblyScanner
    {
        private const string AssemblyContainName = ".Kernel";
        private const string AssemblyStartWith = "CMC.";
        private const string RepositoryNamePart = "Repository";
        private const string ServiceNamePart = "Service";
        private const string DllWildcardName = "CMC*.dll";

        /// <summary>
        /// Registr of the sercvies in the Assemblies 
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterRunningModuleDependencies(this IServiceCollection services)
        {
            var basePath = Directory.GetParent(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath).FullName;
            var assemblies = MakeSureAllAssembliesAreLoaded(basePath);
            var runningAssemblies = assemblies;
            var baseRepositoryType = typeof(IBaseRepository);
            var baseServiceType = typeof(IApplicationService);
            var allTypes = runningAssemblies
                .SelectMany(x => x.GetTypes()).Where(x => !x.IsGenericType && !x.IsAbstract && x.IsClass &&
                            (baseRepositoryType.IsAssignableFrom(x) ||
                            baseServiceType.IsAssignableFrom(x))).ToList();
            allTypes.ForEach(type =>
            {
                var interfaceType = type.GetInterfaces().Except(type.BaseType.GetInterfaces()).FirstOrDefault(x =>
                x.Name.EndsWith(RepositoryNamePart, StringComparison.OrdinalIgnoreCase) ||
                x.Name.EndsWith(ServiceNamePart, StringComparison.OrdinalIgnoreCase));
                if (interfaceType != null)
                {
                    if (typeof(ISingletonDependency).IsAssignableFrom(type))
                    {
                        services.AddSingleton(interfaceType, type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(type))
                    {
                        services.AddTransient(interfaceType, type);
                    }
                    else if (typeof(IScopedDependency).IsAssignableFrom(type))
                    {
                        services.AddScoped(interfaceType, type);
                    }
                    else
                        services.AddScoped(interfaceType, type);
                }
            });
        }
        /// <summary>
        /// Double check the adding of the assemblies 
        /// </summary>
        /// <param name="binDirectory"></param>
        /// <returns></returns>
        private static IEnumerable<Assembly> MakeSureAllAssembliesAreLoaded(string binDirectory)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && x.FullName.StartsWith(AssemblyStartWith, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var fileName in Directory.EnumerateFiles(binDirectory, DllWildcardName, SearchOption.TopDirectoryOnly))
            {
                if (!assemblies.Any(x => x.CodeBase.Equals(new Uri(fileName).AbsoluteUri, StringComparison.OrdinalIgnoreCase)))
                {
                    var assembly = Assembly.LoadFrom(fileName);
                    assemblies.Add(assembly);
                }
            }
            return assemblies;
        }
    }
}
