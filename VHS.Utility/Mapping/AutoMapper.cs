using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace VHS.Utility.Mapping
{
    public static class AutoMapper
    {
        private static readonly IReadOnlySet<Type> _doNotTraverseTypes = new HashSet<Type>
        {
            typeof(string)
        };

        /// <summary>
        /// Maps an object of an arbitrary type into another. Does not map value types.
        /// </summary>
        /// <typeparam name="T">The output type</typeparam>
        /// <param name="value">An instance of an arbitrary type</param>
        /// <returns>An instance of the output type that contains the same property values as the input</returns>
        public static T Map<T>(object value) => (T)Map(value.GetType(), typeof(T), value);

        /// <inheritdoc cref="Map{T}(object)"/>
        /// <typeparam name="TIn">The input type</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        public static TOut Map<TIn, TOut>(TIn value) => (TOut)Map(typeof(TIn), typeof(TOut), value);

        private static object Map(Type tin, Type tout, object inValue)
        {
            object outValue;
            PropertyInfo[] props;

            if (inValue is null)
                return null;

            outValue    = Activator.CreateInstance(tout);
            props       = tin.GetProperties();

            foreach (PropertyInfo inProp in props)
            {
                PropertyInfo outProp    = tout.GetProperty(inProp.Name);

                // Do not set property value if it does not have a setter
                if (inProp.SetMethod is null)
                    continue;

                // TODO: Map collection types
                if (inProp.PropertyType != typeof(string) && inProp.PropertyType.GetInterface(nameof(IEnumerable)) is not null)
                    throw new NotSupportedException("Collection type mapping is not yet supported");

                if (_doNotTraverseTypes.Contains(inProp.PropertyType) || inProp.PropertyType.IsPrimitive || inProp.PropertyType.IsValueType)
                {
                    if (outProp is null)
                        continue;
                    outProp.SetValue(outValue, inProp.GetValue(inValue));
                    continue;
                }

                outProp.SetValue(outValue, Map(inProp.PropertyType, outProp.PropertyType, inProp.GetValue(inValue)));
            }

            return outValue;
        }
    }
}
