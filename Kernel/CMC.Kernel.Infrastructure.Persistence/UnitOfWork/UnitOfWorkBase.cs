using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.UnitOfWork
{
    /// <summary>
    /// Unit Of Work Base
    /// </summary>
    /// <typeparam name="TMappingInterface"></typeparam>
    public abstract class UnitOfWorkBase<TMappingInterface> : DbContext, IUnitOfWork
    {
        /// <summary>
        /// disposed variable  
        /// </summary>
        private bool disposed = false;
        public UnitOfWorkBase() { }
        /// <summary>
        /// constructor of Unit Of Wor kBase
        /// </summary>
        /// <param name="options"></param>
        public UnitOfWorkBase(DbContextOptions options) : base(options)
        {
        }
        /// <summary>
        /// On Model Creating method 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            RegisterMappings(modelBuilder);
        }
        /// <summary>
        /// Register Mappings
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected virtual void RegisterMappings(ModelBuilder modelBuilder)
        {
            var mappingAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.ManifestModule != null && x.ManifestModule.Name.StartsWith("CMC.", StringComparison.OrdinalIgnoreCase)
            && x.ManifestModule.Name.EndsWith("Persistence.dll", StringComparison.OrdinalIgnoreCase)).ToList();
            var mappingTypes = PickMappingTypes(mappingAssemblies);
            foreach (var type in mappingTypes)
            {
                modelBuilder.ApplyConfiguration((dynamic)Activator.CreateInstance(type));
            }
        }
        /// <summary>
        /// Pick Mapping Types
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        private IEnumerable<Type> PickMappingTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsClass && !x.IsAbstract && typeof(TMappingInterface).IsAssignableFrom(x));
        }
        /// <summary>
        /// Begin Transaction
        /// </summary>
        public void BeginTransaction()
        {
            if (Database.CurrentTransaction == null)
                Database.BeginTransaction();
        }
        /// <summary>
        /// Commit
        /// </summary>
        public void Commit()
        {
            if (Database.CurrentTransaction != null)
                Database.CommitTransaction();
        }
        /// <summary>
        /// Roll back
        /// </summary>
        public void Rollback()
        {
            if (Database.CurrentTransaction != null)
                Database.RollbackTransaction();
        }
        /// <summary>
        /// Save Changes Asyncronce 
        /// </summary>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync()
        {
            var result = await base.SaveChangesAsync();
            return result;
        }
        /// <summary>
        /// Dispose 
        /// </summary>
        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
