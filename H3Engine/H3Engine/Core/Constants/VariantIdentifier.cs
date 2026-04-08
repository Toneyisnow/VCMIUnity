// Migrated from VCMI lib/constants/VariantIdentifier.h
// Variant identifier that can hold multiple different identifier types

using System;

namespace H3Engine.Core.Constants
{
    /// <summary>
    /// Represents a field that may contain a value of multiple different identifier types.
    /// Useful for flexible game data that can reference multiple entity types.
    /// Corresponds to VCMI's VariantIdentifier.
    /// </summary>
    /// <typeparam name="T">Types that this variant can hold</typeparam>
    public class VariantIdentifier<T> where T : class
    {
        private object value;
        private Type storedType;

        public VariantIdentifier()
        {
            value = null;
            storedType = null;
        }

        public VariantIdentifier(T identifier)
        {
            value = identifier;
            storedType = identifier?.GetType();
        }

        public VariantIdentifier(object identifier)
        {
            value = identifier;
            storedType = identifier?.GetType();
        }

        /// <summary>Gets the numeric value of the stored identifier.</summary>
        public int GetNum()
        {
            if (value is IdentifierBase id)
                return id.GetNum();
            return -1;
        }

        /// <summary>Encodes the stored identifier to a string.</summary>
        public string ToString(Func<object, string> encoder)
        {
            if (value == null)
                return "";
            return encoder(value);
        }

        /// <summary>Gets the stored value cast to the specified type.</summary>
        public TResult As<TResult>() where TResult : class
        {
            return value as TResult;
        }

        /// <summary>Returns true if this variant has a value.</summary>
        public bool HasValue()
        {
            if (value is IdentifierBase id)
                return id.HasValue();
            return value != null;
        }

        /// <summary>Gets the stored value.</summary>
        public object GetValue() => value;

        /// <summary>Gets the type of the stored value.</summary>
        public Type GetValueType() => storedType;

        public bool Equals(VariantIdentifier<T> other)
        {
            if (other == null)
                return false;
            if (value == null && other.value == null)
                return true;
            if (value == null || other.value == null)
                return false;
            return value.Equals(other.value);
        }

        public override bool Equals(object obj) => obj is VariantIdentifier<T> vi && Equals(vi);

        public override int GetHashCode() => value?.GetHashCode() ?? 0;

        public override string ToString() => value?.ToString() ?? "null";
    }

    /// <summary>
    /// Two-type variant identifier.
    /// </summary>
    public class VariantIdentifier<T1, T2> where T1 : class where T2 : class
    {
        private object value;

        public VariantIdentifier() { }

        public VariantIdentifier(T1 id) => value = id;

        public VariantIdentifier(T2 id) => value = id;

        public int GetNum()
        {
            if (value is IdentifierBase id)
                return id.GetNum();
            return -1;
        }

        public bool HasValue()
        {
            if (value is IdentifierBase id)
                return id.HasValue();
            return value != null;
        }

        public T1 As1() => value as T1;

        public T2 As2() => value as T2;

        public object GetValue() => value;

        public override bool Equals(object obj)
        {
            if (obj is VariantIdentifier<T1, T2> other)
                return Equals(value, other.value);
            return false;
        }

        public override int GetHashCode() => value?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Three-type variant identifier.
    /// </summary>
    public class VariantIdentifier<T1, T2, T3> where T1 : class where T2 : class where T3 : class
    {
        private object value;

        public VariantIdentifier() { }

        public VariantIdentifier(T1 id) => value = id;

        public VariantIdentifier(T2 id) => value = id;

        public VariantIdentifier(T3 id) => value = id;

        public int GetNum()
        {
            if (value is IdentifierBase id)
                return id.GetNum();
            return -1;
        }

        public bool HasValue()
        {
            if (value is IdentifierBase id)
                return id.HasValue();
            return value != null;
        }

        public T1 As1() => value as T1;

        public T2 As2() => value as T2;

        public T3 As3() => value as T3;

        public object GetValue() => value;

        public override bool Equals(object obj)
        {
            if (obj is VariantIdentifier<T1, T2, T3> other)
                return Equals(value, other.value);
            return false;
        }

        public override int GetHashCode() => value?.GetHashCode() ?? 0;
    }
}
