// ***********************************************************************
// Assembly         : DIFactoryBuilder
// Author           : CryoM
// Created          : 05-05-2021
//
// Last Modified By : CryoM
// Last Modified On : 05-06-2021
// ***********************************************************************
// <copyright file="FactoryMethodNameAttribute.cs" company="Codari">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DIFactoryBuilder.Attributes
{
    /// <summary>
    /// Class FactoryMethodAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Constructor)]
    public class FactoryMethodNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the factory method.
        /// </summary>
        /// <value>The name of the factory method.</value>
        public string FactoryMethodName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryMethodNameAttribute" /> class.
        /// </summary>
        /// <param name="factoryMethodName">Name of the factory method.</param>
        public FactoryMethodNameAttribute(string factoryMethodName)
        {
            this.FactoryMethodName = factoryMethodName;
        }
    }
}
