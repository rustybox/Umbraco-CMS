﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="IFactory"/> class.
    /// </summary>
    public static class ServiceProviderExtensions
    {

        /// <summary>
        /// Creates an instance with arguments.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="serviceProvider">The factory.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>
        /// <para>Throws an exception if the factory failed to get an instance of the specified type.</para>
        /// <para>The arguments are used as dependencies by the factory.</para>
        /// </remarks>
        public static T CreateInstance<T>(this IServiceProvider serviceProvider, params object[] args)
            where T : class
            => (T)serviceProvider.CreateInstance(typeof(T), args);

        /// <summary>
        /// Creates an instance of a service, with arguments.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="type">The type of the instance.</param>
        /// <param name="args">Named arguments.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>
        /// <para>The instance type does not need to be registered into the factory.</para>
        /// <para>The arguments are used as dependencies by the factory. Other dependencies
        /// are retrieved from the factory.</para>
        /// </remarks>
        public static object CreateInstance(this IServiceProvider serviceProvider, Type type, params object[] args)
        {
            // LightInject has this, but then it requires RegisterConstructorDependency etc and has various oddities
            // including the most annoying one, which is that it does not work on singletons (hard to fix)
            //return factory.GetInstance(type, args);

            // this method is essentially used to build singleton instances, so it is assumed that it would be
            // more expensive to build and cache a dynamic method ctor than to simply invoke the ctor, as we do
            // here - this can be discussed

            // TODO: we currently try the ctor with most parameters, but we could want to fall back to others

            //var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
            //if (ctor == null) throw new InvalidOperationException($"Could not find a public constructor for type {type.FullName}.");

            //var ctorParameters = ctor.GetParameters();
            //var ctorArgs = new object[ctorParameters.Length];
            //var availableArgs = new List<object>(args);
            //var i = 0;
            //foreach (var parameter in ctorParameters)
            //{
            //    // no! IsInstanceOfType is not ok here
            //    // ReSharper disable once UseMethodIsInstanceOfType
            //    var idx = availableArgs.FindIndex(a => parameter.ParameterType.IsAssignableFrom(a.GetType()));
            //    if (idx >= 0)
            //    {
            //        // Found a suitable supplied argument
            //        ctorArgs[i++] = availableArgs[idx];

            //        // A supplied argument can be used at most once
            //        availableArgs.RemoveAt(idx);
            //    }
            //    else
            //    {
            //        // None of the provided arguments is suitable: get an instance from the factory
            //        ctorArgs[i++] = serviceProvider.GetRequiredService(parameter.ParameterType);
            //    }
            //}
            //return ctor.Invoke(ctorArgs);

            return ActivatorUtilities.CreateInstance(serviceProvider, type, args);
        }
    }
}
