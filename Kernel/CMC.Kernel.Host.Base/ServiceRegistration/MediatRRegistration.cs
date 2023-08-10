using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class MediatRRegistration
    {
        private const string AssemblyStartWith = "CMC.";
        private const string BehaviorNamePart = "Behavior";
        private const string ExceptionHandlerNamePart = "ExceptionHandler";
        private const string DllWildcardName = "CMC*.dll";
        /// <summary>
        /// Used To Register MediatR
        /// </summary>
        /// <param name="services"></param>
        public static void AddMediatRRegistration(this IServiceCollection services)
        {
            var files = GetAllMediatorFiles(services);
            RegisterMediator(services);
            RegisterBehaviors(services, files);
        }
        /// <summary>
        ///  Used To Register Mediator
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterMediator(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            var mappingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                                           .Where(x => x.ManifestModule != null &&
                                                                       x.ManifestModule.Name.StartsWith(AssemblyStartWith, StringComparison.OrdinalIgnoreCase) &&
                                                                       (x.ManifestModule.Name.EndsWith("Application.dll", StringComparison.OrdinalIgnoreCase))).ToList();

            var loadedPaths = mappingAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*Application.dll");
            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

            toLoad.ForEach(path => mappingAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

            foreach (var type in mappingAssemblies)
            {
                services.AddMediatR(type);
            }
        }
        /// <summary>
        ///  Used To Register Behaviors
        /// </summary>
        /// <param name="services"></param>
        /// <param name="types"></param>
        private static void RegisterBehaviors(IServiceCollection services, List<Type> types)
        {
            var basePipelineBehaviorType = typeof(IPipelineBehavior<,>);
            var baseExceptionHandlerType = typeof(IRequestExceptionHandler<,,>);

            types.Where(x => x.IsClass && x.Name.Contains(BehaviorNamePart, StringComparison.OrdinalIgnoreCase)).ToList()
                .ForEach(type =>
                {
                    //services.AddScoped(typeof(IPipelineBehavior<,>), type);
                    services.AddSingleton(typeof(IPipelineBehavior<,>), type);
                });

            types.Where(x => x.IsClass && x.Name.Contains(ExceptionHandlerNamePart, StringComparison.OrdinalIgnoreCase)).ToList()
                .ForEach(type =>
                {
                    services.AddSingleton(typeof(IRequestExceptionHandler<,,>), type);
                });
        }
        /// <summary>
        /// To Get All Mediator Files
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static List<Type> GetAllMediatorFiles(IServiceCollection services)
        {
            var basePath = Directory.GetParent(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath).FullName;
            var assemblies = MakeSureAllAssembliesAreLoaded(basePath);

            var allTypes = assemblies
                .SelectMany(x => x.GetTypes()).Where(x => !x.IsAbstract &&
                                                     (x.IsClass && x.Name.Contains(BehaviorNamePart, StringComparison.OrdinalIgnoreCase) ||
                                                     x.IsClass && x.Name.Contains(ExceptionHandlerNamePart, StringComparison.OrdinalIgnoreCase))).ToList();

            return allTypes;
        }
        /// <summary>
        /// To Make Sure All Assemblies Are Loaded
        /// </summary>
        /// <param name="binDirectory"></param>
        /// <returns></returns>
        private static IEnumerable<Assembly> MakeSureAllAssembliesAreLoaded(string binDirectory)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && x.FullName.StartsWith(AssemblyStartWith, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var fileName in Directory.EnumerateFiles(binDirectory, DllWildcardName, SearchOption.TopDirectoryOnly))
            {
                if (!assemblies.Any(predicate: x => x.CodeBase.Equals(new Uri(fileName).AbsoluteUri, StringComparison.OrdinalIgnoreCase)))
                {
                    var assembly = Assembly.LoadFrom(fileName);
                    assemblies.Add(assembly);
                }
            }
            return assemblies;
        }
    }
}
