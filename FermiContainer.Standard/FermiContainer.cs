/******************************************************************************
The MIT License (MIT)

Copyright (c) 2014 Runar Ovesen Hjerpbakk

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
******************************************************************************
    FermiContainer version 1.1.1
    http://www.hjerpbakk.com/fermicontainer
    https://twitter.com/hjerpbakk
******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Hjerpbakk.FermiContainer
{
    /// <summary>
    ///  Defines a set of methods used to register services into the service container.
    /// </summary>
    public class FermiContainer : IFermiContainer
    {
        private static readonly Lazy<IFermiContainer> defaultInstance;

        /// <summary>
        /// Contains the registered services.
        /// </summary>
        protected readonly Dictionary<Type, Service> Services;

        static FermiContainer()
        {
            defaultInstance = new Lazy<IFermiContainer>(() => new FermiContainer());
        }

        /// <summary>
        /// Default contstructor.
        /// </summary>
        public FermiContainer()
        {
            Services = new Dictionary<Type, Service>();
        }

        /// <summary>
        /// The default instance of the container.
        /// </summary>
        public static IFermiContainer DefaultInstance { get { return defaultInstance.Value; } }

        /// <summary>
        /// Registers an interface with a given implementing class in the container.
        /// The first constructor of the implementing class is used, and constructor
        /// arguments are automatically resolved using the container.
        /// </summary>
        /// <typeparam name="TInterface">The interface which the class satisfies.</typeparam>
        /// <typeparam name="TClass">The implementing class of the interface.</typeparam>
        public void Register<TInterface, TClass>() where TClass : class, TInterface
        {
            var type = typeof(TClass);
            var ctor = type.GetConstructors()[0];
            var neededParameters = ctor.GetParameters();
            var n = neededParameters.Length;
            var arguments = new Expression[n];
            for (int i = 0; i < n; i++)
            {
                var argumentType = neededParameters[i].ParameterType;
                Expression<Func<object>> getService = () => Services[argumentType].Factory();
                arguments[i] = Expression.Convert(getService.Body, argumentType);
            }

            var newExpression = Expression.New(ctor, arguments);
            var newAsLambda = Expression.Lambda<Func<object>>(newExpression);
            Services.Add(typeof(TInterface), new Service(newAsLambda.Compile()));
        }

        /// <summary>
        /// Registers a class without an interface in the container.
        /// </summary>
        /// <typeparam name="TClass">The class to be registered.</typeparam>
        public void Register<TClass>() where TClass : class
        {
            Register<TClass, TClass>();
        }

        /// <summary>
        /// Registers an implementing class in the container using a factory method.
        /// Use this if your implementation has dependencies not present
        /// in the container.
        /// </summary>
        /// <param name="factory">The factory method to use.</param>
        /// <typeparam name="TInterface">The interface which the class satisfies.</typeparam>
        public void Register<TInterface>(Func<object> factory)
        {
            Services.Add(typeof(TInterface), new Service(factory));
        }

        /// <summary>
        /// Returns the registered implementing class of the given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface from which to get the implementation.</typeparam>
        public TInterface Resolve<TInterface>() where TInterface : class
        {
            return (TInterface)Services[typeof(TInterface)].Factory();
        }

        /// <summary>
        /// Returns the registered implementing class of the given interface as a singleton.
        /// This method will always return the same instance for a given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface from which to get the implementation.</typeparam>
        public TInterface Singleton<TInterface>() where TInterface : class
        {
            var service = Services[typeof(TInterface)];
            if (Interlocked.Exchange(ref service.SingletonInitialized, 1) == 0)
            {
                var value = (TInterface)service.Factory();
                service.Factory = () => value;
                return value;
            }

            return (TInterface)service.Factory();
        }

        /// <summary>
        /// The definition of a registerable service.
        /// </summary>
        protected class Service
        {
            /// <summary>
            /// Whether a singleton has already been initialized.
            /// </summary>
            public int SingletonInitialized;

            /// <summary>
            /// The factory used to create an instance of a registered service.
            /// </summary>
            public Func<object> Factory;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="factory">The factory used to create an instance of a registered service.</param>
            public Service(Func<object> factory)
            {
                Factory = factory;
            }
        }
    }

    /// <summary>
    ///  Defines a set of methods used to register services into the service container.
    /// </summary>
    public interface IFermiContainer
    {
        /// <summary>
        /// Registers an interface with a given implementing class in the container.
        /// The first constructor of the implementing class is used, and constructor
        /// arguments are automatically resolved using the container.
        /// </summary>
        /// <typeparam name="TInterface">The interface which the class satisfies.</typeparam>
        /// <typeparam name="TClass">The implementing class of the interface.</typeparam>
        void Register<TInterface, TClass>() where TClass : class, TInterface;

        /// <summary>
        /// Registers a class without an interface in the container.
        /// </summary>
        /// <typeparam name="TClass">The class to be registered.</typeparam>
        void Register<TClass>() where TClass : class;

        /// <summary>
        /// Registers an implementing class in the container using a factory method.
        /// Use this if your implementation has dependencies not present
        /// in the container.
        /// </summary>
        /// <param name="factory">The factory method to use.</param>
        /// <typeparam name="TInterface">The interface which the class satisfies.</typeparam>
        void Register<TInterface>(Func<object> factory);

        /// <summary>
        /// Returns the registered implementing class of the given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface from which to get the implementation.</typeparam>
        TInterface Resolve<TInterface>() where TInterface : class;

        /// <summary>
        /// Returns the registered implementing class of the given interface as a singleton.
        /// This method will always return the same instance for a given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface from which to get the implementation.</typeparam>
        TInterface Singleton<TInterface>() where TInterface : class;
    }
}

