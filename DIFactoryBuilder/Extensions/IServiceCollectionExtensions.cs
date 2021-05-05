// ***********************************************************************
// Assembly         : DIFactoryBuilder
// Author           : CryoM
// Created          : 04-12-2021
//
// Last Modified By : CryoM
// Last Modified On : 04-12-2021
// ***********************************************************************
// <copyright file="IServiceCollectionExtensions.cs" company="DIFactoryBuilder">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace DIFactoryBuilder.Extensions
{
    /// <summary>
    /// Class IServiceCollectionExtensions.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the factories.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection RegisterFactories(this IServiceCollection serviceCollection, Assembly assembly)
        {
            var possibleFactoryTypes = assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => t.BaseType is not null)
                .Where(t => t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(IDIFactory<>));

            foreach (var factoryType in possibleFactoryTypes)
            {
                var factoryForType = factoryType.BaseType?.GetGenericArguments()[0]!;
                serviceCollection.AddSingleton(factoryType);
            }

            return serviceCollection;
        }
    }
}
