// ***********************************************************************
// Assembly         : DIFactoryBuilder
// Author           : CryoM
// Created          : 04-09-2021
//
// Last Modified By : CryoM
// Last Modified On : 04-09-2021
// ***********************************************************************
// <copyright file="RequiresFactoryAttribute.cs" company="DIFactoryBuilder">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DIFactoryBuilder.Attributes
{
    using System;

    /// <summary>
    /// Class RequiresFactoryAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresFactoryAttribute : Attribute
    {
        
    }
}
