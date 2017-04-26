﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;

    /// <summary>
    /// Factory generator for <see cref="IRegistrationVisitor"/>
    /// </summary>
    internal static class VisitorFactory
    {
        private static readonly Lazy<Type[]> NonGenericVisitorTypes = new Lazy<Type[]>(GetNonGenericVisitorTypes);

        private static readonly Lazy<Type[]> GenericVisitorTypes = new Lazy<Type[]>(GetGenericVisitorTypes);

        /// <summary>
        /// Creates the visitors that are <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> to the specified type.
        /// </summary>
        /// <typeparam name="TRegistration">The type of visitor.</typeparam>
        /// <returns>
        /// The visitors that are <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> to the specified type.
        /// </returns>
        public static IEnumerable<IRegistrationVisitor<TRegistration>> CreateVisitors<TRegistration>()
            where TRegistration : class, IRegistration
        {
            TypeInfo visitorTypeInfo = typeof(IRegistrationVisitor<TRegistration>).GetTypeInfo();
            TypeInfo registrationTypeInfo = typeof(TRegistration).GetTypeInfo();

            // Non generic types are much simpler.
            if (!registrationTypeInfo.IsGenericType)
            {
                // Get the non generic types that are assignable to the visitor type.
                IEnumerable<Type> types = NonGenericVisitorTypes.Value.Where(visitorTypeInfo.IsAssignableFrom);

                // Create the visitors.
                return types.Select(Activator.CreateInstance).Cast<IRegistrationVisitor<TRegistration>>();
            }

            // Get the generic arguments of the registration type.
            Type[] typeArguments = registrationTypeInfo.GenericTypeArguments;

            // Get the generic types with the same number of type parameters.
            IEnumerable<Type> genericTypeDefinitions =
                GenericVisitorTypes.Value
                    .Where(t => t.GetTypeInfo().GenericTypeParameters.Length == typeArguments.Length);

            // Make the generic types and get the ones that are assignable to the visitor type.
            IEnumerable<Type> genericTypes =
                genericTypeDefinitions
                    .Select(gt => gt.GetTypeInfo().MakeGenericType(typeArguments))
                    .Where(visitorTypeInfo.IsAssignableFrom);

            // Create the visitors.
            return genericTypes.Select(Activator.CreateInstance).Cast<IRegistrationVisitor<TRegistration>>();
        }

        private static IEnumerable<Type> GetAllVisitorTypes()
        {
            Assembly assembly = typeof(IRegistrationVisitor).GetTypeInfo().Assembly;

            IEnumerable<Type> visitorAllTypes =
                from t in assembly.GetTypes().Where(typeof(IRegistrationVisitor).GetTypeInfo().IsAssignableFrom)
                let ti = t.GetTypeInfo()
                where ti.IsClass && !ti.IsAbstract
                select t;

            return visitorAllTypes;
        }

        private static Type[] GetNonGenericVisitorTypes()
        {
            return GetAllVisitorTypes()
                .Where(t => !t.GetTypeInfo().ContainsGenericParameters)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        private static Type[] GetGenericVisitorTypes()
        {
            return GetAllVisitorTypes()
                .Where(t => t.GetTypeInfo().ContainsGenericParameters)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        ////private static Type[] GetTypedVisitorTypes()
        ////{
        ////    IEnumerable<Type> types =
        ////        from t in GetAllVisitorTypes()
        ////        let info = t.GetTypeInfo()
        ////        where info.ContainsGenericParameters
        ////              && info.GenericTypeParameters.Length == 1
        ////              && info.GenericTypeParameters[0].Name == "TImplementation"
        ////        orderby t.Name
        ////        select t;

        ////    return types.ToArray();
        ////}

        ////private static Type[] GetVisitorWithContextTypes()
        ////{
        ////    IEnumerable<Type> types =
        ////        from t in GetAllVisitorTypes()
        ////        let info = t.GetTypeInfo()
        ////        where info.ContainsGenericParameters
        ////              && info.GenericTypeParameters.Length == 1
        ////              && info.GenericTypeParameters[0].Name == "TExtra"
        ////        orderby t.Name
        ////        select t;

        ////    return types.ToArray();
        ////}

        ////private static Type[] GetTypedVisitorWithContextTypes()
        ////{
        ////    IEnumerable<Type> types =
        ////        from t in GetAllVisitorTypes()
        ////        let info = t.GetTypeInfo()
        ////        where info.ContainsGenericParameters
        ////              && info.GenericTypeParameters.Length == 2
        ////              && info.GenericTypeParameters[0].Name == "TExtra"
        ////              && info.GenericTypeParameters[0].Name == "TImplementation"
        ////        orderby t.Name
        ////        select t;

        ////    return types.ToArray();
        ////}
    }
}
