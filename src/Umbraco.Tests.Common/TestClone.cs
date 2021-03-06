﻿using System;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Common
{
    public class TestClone : IDeepCloneable, IEquatable<TestClone>
    {
        public TestClone(Guid id)
        {
            Id = id;
            IsClone = true;
        }

        public TestClone()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public bool IsClone { get; }

        public object DeepClone()
        {
            return new TestClone(Id);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TestClone other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestClone)obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TestClone left, TestClone right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestClone left, TestClone right)
        {
            return Equals(left, right) == false;
        }
    }
}
