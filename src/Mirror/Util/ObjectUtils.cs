#region Imports

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;

#endregion

namespace Mirror.Util
{
    /// <summary>
    /// Helper methods with regard to objects, types, properties, etc.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Not intended to be used directly by applications.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    public sealed class ObjectUtils
    {
        #region Constants

        /// <summary>
        /// An empty object array.
        /// </summary>
        public static readonly object[] EmptyObjects = new object[] { };

        private static MethodInfo GetHashCodeMethodInfo = null;

        #endregion

        static ObjectUtils()
		{
			Type type = typeof(object);
			GetHashCodeMethodInfo = type.GetMethod("GetHashCode");
		}
        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the <see cref="Mirror.Util.ObjectUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        private ObjectUtils()
        {
        }

        // CLOVER:ON

        #endregion

        /// <summary>
        /// Checks whether the supplied <paramref name="instance"/> is not a transparent proxy and is
        /// assignable to the supplied <paramref name="type"/>. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// Neccessary when dealing with server-activated remote objects, because the
        /// object is of the type TransparentProxy and regular <c>is</c> testing for assignable
        /// types does not work.
        /// </p>
        /// <p>
        /// Transparent proxy instances always return <see langword="true"/> when tested
        /// with the <c>'is'</c> operator (C#). This method only checks if the object
        /// is assignable to the type if it is not a transparent proxy.
        /// </p>
        /// </remarks>
        /// <param name="type">The target <see cref="System.Type"/> to be checked.</param>
        /// <param name="instance">The value that should be assigned to the type.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="instance"/> is not a
        /// transparent proxy and is assignable to the supplied <paramref name="type"/>.
        /// </returns>
        public static bool IsAssignableAndNotTransparentProxy(Type type, object instance)
        {
            if (!RemotingServices.IsTransparentProxy(instance))
            {
                return IsAssignable(type, instance);
            }
            return false;
        }

        /// <summary>
        /// Determine if the given <see cref="System.Type"/> is assignable from the
        /// given value, assuming setting by reflection and taking care of transparent proxies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Considers primitive wrapper classes as assignable to the
        /// corresponding primitive types.
        /// </p>
        /// <p>
        /// For example used in an object factory's constructor resolution.
        /// </p>
        /// </remarks>
        /// <param name="type">The target <see cref="System.Type"/>.</param>
        /// <param name="obj">The value that should be assigned to the type.</param>
        /// <returns>True if the type is assignable from the value.</returns>
        public static bool IsAssignable(Type type, object obj)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            if (!type.IsPrimitive && obj == null)
            {
                return true;
            }

            if (RemotingServices.IsTransparentProxy(obj))
            {
                RealProxy rp = RemotingServices.GetRealProxy(obj);
                if (rp is IRemotingTypeInfo)
                {
                    return ((IRemotingTypeInfo) rp).CanCastTo(type, obj);
                }
                else if (rp != null)
                {
                    type = rp.GetProxiedType();
                }

                if (type == null)
                {
                    // cannot decide
                    return false;
                }
            }

            return (type.IsInstanceOfType(obj) ||
                    (type.Equals(typeof(bool)) && obj is Boolean) ||
                    (type.Equals(typeof(byte)) && obj is Byte) ||
                    (type.Equals(typeof(char)) && obj is Char) ||
                    (type.Equals(typeof(sbyte)) && obj is SByte) ||
                    (type.Equals(typeof(int)) && obj is Int32) ||
                    (type.Equals(typeof(short)) && obj is Int16) ||
                    (type.Equals(typeof(long)) && obj is Int64) ||
                    (type.Equals(typeof(float)) && obj is Single) ||
                    (type.Equals(typeof(double)) && obj is Double));
        }

        /// <summary>
        /// Check if the given <see cref="System.Type"/> represents a
        /// "simple" property,
        /// i.e. a primitive, a <see cref="System.String"/>, a
        /// <see cref="System.Type"/>, or a corresponding array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Used to determine properties to check for a "simple" dependency-check.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check.
        /// </param>
        public static bool IsSimpleProperty(Type type)
        {
            return type.IsPrimitive
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(string[]))
                   || IsPrimitiveArray(type)
                   || type.Equals(typeof(Type))
                   || type.Equals(typeof(Type[]));
        }

        /// <summary>
        /// Check if the given class represents a primitive array,
        /// i.e. boolean, byte, char, short, int, long, float, or double.
        /// </summary>
        public static bool IsPrimitiveArray(Type type)
        {
            return typeof(bool[]).Equals(type)
                   || typeof(sbyte[]).Equals(type)
                   || typeof(char[]).Equals(type)
                   || typeof(short[]).Equals(type)
                   || typeof(int[]).Equals(type)
                   || typeof(long[]).Equals(type)
                   || typeof(float[]).Equals(type)
                   || typeof(double[]).Equals(type);
        }


        /// <summary>
        /// Determines whether the specified array is null or empty.
        /// </summary>
        /// <param name="array">The array to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified array is null empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(object[] array)
        {
            return (array == null || array.Length == 0);
        }
        /// <summary>
        /// Determine if the given objects are equal, returning <see langword="true"/>
        /// if both are <see langword="null"/> respectively <see langword="false"/>
        /// if only one is <see langword="null"/>.
        /// </summary>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the given objects are equal.
        /// </returns>
        public static bool NullSafeEquals(object o1, object o2)
        {
            return (o1 == o2 || (o1 != null && o1.Equals(o2)));
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerator"/> did not have any elements.
        /// </exception>
        public static object EnumerateFirstElement(IEnumerator enumerator)
        {
            return ObjectUtils.EnumerateElementAtIndex(enumerator, 0);
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerable"/> did not have any elements.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static object EnumerateFirstElement(IEnumerable enumerable)
        {
            AssertUtils.ArgumentNotNull(enumerable, "enumerable");
            return ObjectUtils.EnumerateElementAtIndex(enumerable.GetEnumerator(), 0);
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerator"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        public static object EnumerateElementAtIndex(IEnumerator enumerator, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            object element = null;
            int i = 0;
            while (enumerator.MoveNext())
            {
                element = enumerator.Current;
                if (++i > index)
                {
                    break;
                }
            }
            if (i < index)
            {
                throw new ArgumentOutOfRangeException();
            }
            return element;
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerable"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static object EnumerateElementAtIndex(IEnumerable enumerable, int index)
        {
            AssertUtils.ArgumentNotNull(enumerable, "enumerable");
            return ObjectUtils.EnumerateElementAtIndex(enumerable.GetEnumerator(), index);
        }

        /// <summary>
        /// Gets the qualified name of the given method, consisting of 
        /// fully qualified interface/class name + "." method name.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>qualified name of the method.</returns>
        public static string GetQualifiedMethodName(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "method", "MethodInfo must not be null");
            return method.DeclaringType.FullName + "." + method.Name;
        }

        /// <summary>
        /// Return a String representation of an object's overall identity.
        /// </summary>
        /// <param name="obj">The object (may be <code>null</code>).</param>
        /// <returns>The object's identity as String representation,
        /// or an empty String if the object was <code>null</code>
        /// </returns>
        public static object IdentityToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return obj.GetType().FullName + "@" + GetIdentityHexString(obj);
        }

        /// <summary>
        /// Gets a hex String form of an object's identity hash code.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The object's identity code in hex notation</returns>
        public static string GetIdentityHexString(object obj)
        {
            int hashcode = (int)GetHashCodeMethodInfo.Invoke(obj, null);
            return hashcode.ToString("X6");
        }
    }
}
