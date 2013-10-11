using System;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Defines the field info structure.
    /// </summary>
    public struct FieldInfo : IEquatable<FieldInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldStruct" /> struct.
        /// </summary>
        /// <param name="internalName">The field internal Name.</param>
        /// <param name="id">The field id.</param>
        public FieldInfo(string internalName, Guid id)
            : this()
        {
            this.InternalName = internalName;
            this.ID = id;
        }

        /// <summary>
        /// Gets or sets the internal name of the field.
        /// </summary>
        /// <value>
        /// The internal name.
        /// </value>
        public string InternalName { get; private set; }

        /// <summary>
        /// Gets or sets the id of the field.
        /// </summary>
        /// <value>
        /// The id of the field.
        /// </value>
        public Guid ID { get; private set; }

        /// <summary>
        /// ==s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     <c>True</c> if left equals right; otherwise <c>False</c>.
        /// </returns>
        public static bool operator ==(FieldInfo left, FieldInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// !=s the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     <c>True</c> if left does not equal right; otherwise <c>False</c>.
        /// </returns>
        public static bool operator !=(FieldInfo left, FieldInfo right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((FieldInfo)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.InternalName.GetHashCode();
        }

        /// <summary>
        /// Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        ///     <c>True</c> if other is equal to this, else <c>False</c>.
        /// </returns>
        public bool Equals(FieldInfo other)
        {
            if (this.ID != other.ID)
            {
                return false;
            }

            if (this.InternalName != other.InternalName)
            {
                return false;
            }

            return true;
        }
    }
}
