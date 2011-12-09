﻿#region License

// --------------------------------------------------
// Copyright © 2003-2011 OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NHtmlUnit
{
    public class ObjectWrapper : IObjectWrapper
    {
        private static readonly Dictionary<Type, Func<object, object>> creatorDict =
            new Dictionary<Type, Func<object, object>>();

        private readonly object wrappedObject;


        static ObjectWrapper()
        {
            RunObjectWrapperClassConstructors();
        }


        protected ObjectWrapper(object wrappedObject)
        {
            this.wrappedObject = wrappedObject;
        }


        public object WrappedObject
        {
            get { return this.wrappedObject; }
        }


        public static T CreateWrapper<T>(object wrappedObject)
        {
            if (wrappedObject == null)
                return (T)wrappedObject;

            Func<object, object> creator = null;
            var fromType = wrappedObject.GetType();

            // Search up type hierche until we find a matching creator
            do
            {
                if (!creatorDict.TryGetValue(fromType, out creator))
                    fromType = fromType.BaseType;
            } while (creator == null && fromType != null);

            if (creator == null)
            {
                Console.WriteLine("No creator found for " + wrappedObject.GetType().FullName);
                throw new InvalidOperationException();
            }

            return (T)creator(wrappedObject);
        }


        public override bool Equals(object obj)
        {
            var otherObjectWrapper = obj as ObjectWrapper;

            return otherObjectWrapper != null
                       ? WrappedObject.Equals(otherObjectWrapper.WrappedObject)
                       : false;
        }


        public override int GetHashCode()
        {
            return WrappedObject.GetHashCode();
        }


        public override string ToString()
        {
            return WrappedObject.ToString();
        }


        internal static void RegisterWrapperCreator<T>(Func<T, ObjectWrapper> wrapperCreator)
        {
            creatorDict.Add(typeof(T), o => wrapperCreator((T)o));
        }


        private static void RunObjectWrapperClassConstructors()
        {
            Type objectWrapperType = typeof(ObjectWrapper);

            IEnumerable<Type> types = objectWrapperType.Assembly
                .GetExportedTypes()
                .Where(type => type.IsSubclassOf(objectWrapperType));

            foreach (Type type in types)
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
}