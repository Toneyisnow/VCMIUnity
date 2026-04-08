// Migrated from VCMI lib/constants/IdentifierBase.h
// Base class for all identifier types in the game

using System;

namespace H3Engine.Core
{
    /// <summary>
    /// Exception thrown when identifier resolution fails.
    /// </summary>
    public class IdentifierResolutionException : Exception
    {
        public string IdentifierName { get; }

        public IdentifierResolutionException(string identifierName)
            : base($"Failed to resolve identifier {identifierName}")
        {
            IdentifierName = identifierName;
        }
    }

    /// <summary>
    /// Base class for all numeric identifiers in the game.
    /// Corresponds to VCMI's IdentifierBase.
    /// </summary>
    public class IdentifierBase : IEquatable<IdentifierBase>, IComparable<IdentifierBase>
    {
        protected int num;

        protected IdentifierBase()
        {
            num = -1;
        }

        protected IdentifierBase(int value)
        {
            num = value;
        }

        public int GetNum() => num;

        public void SetNum(int value)
        {
            num = value;
        }

        public bool HasValue() => num >= 0;

        public void Advance(int change)
        {
            num += change;
        }

        public static int ResolveIdentifier(string entityType, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return -1;

            // This would integrate with a centralized identifier storage system
            // For now, return -1 as placeholder
            throw new IdentifierResolutionException(identifier);
        }

        public override bool Equals(object obj) => obj is IdentifierBase id && Equals(id);

        public bool Equals(IdentifierBase other) => other != null && num == other.num;

        public int CompareTo(IdentifierBase other)
        {
            if (other == null) return 1;
            return num.CompareTo(other.num);
        }

        public override int GetHashCode() => num.GetHashCode();

        public static implicit operator int(IdentifierBase id) => id?.num ?? -1;

        public override string ToString() => num.ToString();
    }

    /// <summary>
    /// Generic strongly-typed identifier to prevent mixing different identifier types.
    /// </summary>
    /// <typeparam name="T">Type of identifier</typeparam>
    public class Identifier<T> : IdentifierBase where T : Identifier<T>
    {
        public Identifier() { }

        public Identifier(int value) : base(value) { }

        public static bool operator ==(Identifier<T> a, Identifier<T> b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.num == b.num;
        }

        public static bool operator !=(Identifier<T> a, Identifier<T> b) => !(a == b);

        public static bool operator <(Identifier<T> a, Identifier<T> b) => a?.num < b?.num;

        public static bool operator <=(Identifier<T> a, Identifier<T> b) => a?.num <= b?.num;

        public static bool operator >(Identifier<T> a, Identifier<T> b) => a?.num > b?.num;

        public static bool operator >=(Identifier<T> a, Identifier<T> b) => a?.num >= b?.num;
    }

    /// <summary>
    /// Static identifier for game objects that don't need entity service lookups.
    /// </summary>
    /// <typeparam name="T">Type of identifier</typeparam>
    public class StaticIdentifier<T> : Identifier<T> where T : StaticIdentifier<T>
    {
        public StaticIdentifier() { }

        public StaticIdentifier(int value) : base(value) { }
    }

    /// <summary>
    /// Entity identifier that can resolve to entity definitions via services.
    /// </summary>
    /// <typeparam name="T">Type of identifier</typeparam>
    public class EntityIdentifier<T> : Identifier<T> where T : EntityIdentifier<T>
    {
        public EntityIdentifier() { }

        public EntityIdentifier(int value) : base(value) { }

        public virtual string EntityType() => "unknown";

        public virtual int Decode(string identifier) => ResolveIdentifier(EntityType(), identifier);

        public virtual string Encode(int index) => index >= 0 ? index.ToString() : "";
    }

    /// <summary>
    /// Entity identifier with associated enum values.
    /// </summary>
    /// <typeparam name="T">Type of identifier</typeparam>
    /// <typeparam name="TEnum">Enum type with predefined values</typeparam>
    public class EntityIdentifierWithEnum<T, TEnum> : EntityIdentifier<T>
        where T : EntityIdentifierWithEnum<T, TEnum>
        where TEnum : class
    {
        public EntityIdentifierWithEnum() { }

        public EntityIdentifierWithEnum(int value) : base(value) { }
    }
}
