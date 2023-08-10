using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CMC.Kernel.Host.Base.ServiceRegistration
{
    public static class FluentValidationRegistration
    {
        /// <summary>
        /// Registr Fluent Validation
        /// </summary>
        /// <param name="service"></param>
        public static void AddFluentValidationRegistration(this IServiceCollection service)
        {
            service.AddFluentValidation(fv =>
            {
                var basePath = Directory.GetParent(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath).FullName;
                var assemblies = MakeSureAllAssembliesAreLoaded(basePath);

                fv.AutomaticValidationEnabled = false;
                fv.LocalizationEnabled = true;
                fv.RegisterValidatorsFromAssemblies(assemblies.Where(x =>
                    !x.IsDynamic && x.Location.EndsWith(".Application.dll", StringComparison.OrdinalIgnoreCase)).ToList());
                fv.DisableDataAnnotationsValidation = true;
            });
        }
        /// <summary>
        /// To make sure the assemblies are loaded 
        /// </summary>
        /// <param name="binDirectory"></param>
        /// <returns></returns>
        private static IEnumerable<Assembly> MakeSureAllAssembliesAreLoaded(string binDirectory)
        {
            var AssemblyStartWith = "CMC.";
            var DllWildcardName = "CMC*.dll";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && x.FullName.StartsWith(AssemblyStartWith, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var fileName in Directory.EnumerateFiles(binDirectory, DllWildcardName, SearchOption.TopDirectoryOnly))
            {
                if (!assemblies.Any(x => x.Location.Equals(new Uri(fileName).AbsoluteUri, StringComparison.OrdinalIgnoreCase)))
                {
                    var assembly = Assembly.LoadFrom(fileName);
                    assemblies.Add(assembly);
                }
            }
            return assemblies;
        }
    }
}
