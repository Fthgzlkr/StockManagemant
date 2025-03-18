using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace StockManagemant.Web.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositoriesAndManagers(this IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("StockManagemant.Business") || a.FullName.StartsWith("StockManagemant.DataAccess"));

            foreach (var assembly in assemblies)
            {
                // Repositoryleri ekle
                var repositoryTypes = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Repository") && !t.IsInterface && !t.IsAbstract);

                foreach (var repo in repositoryTypes)
                {
                    var interfaceType = repo.GetInterfaces().FirstOrDefault(i => i.Name.EndsWith(repo.Name));
                    if (interfaceType != null)
                    {
                        services.AddScoped(interfaceType, repo);
                    }
                }

                // Managerları ekle
                var managerTypes = assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Manager") && !t.IsInterface && !t.IsAbstract);

                foreach (var manager in managerTypes)
                {
                    var interfaceType = manager.GetInterfaces().FirstOrDefault(i => i.Name.EndsWith(manager.Name));
                    if (interfaceType != null)
                    {
                        services.AddScoped(interfaceType, manager);
                    }
                }
            }

            return services;
        }
    }
}
