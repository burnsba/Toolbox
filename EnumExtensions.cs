    /// <summary>
    /// Custom attribute to expose the type id of the object.
    /// </summary>
    public class TypeId : Attribute
    {
        #region Fields

        private Guid _type;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the guid for the TypeId.
        /// </summary>
        public Guid Id
        {
            get
            {
                return _type;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeId" /> class.
        /// </summary>
        /// <param name="type">Initial value to set.</param>
        public TypeId(Guid type)
        {
            _type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeId" /> class.
        /// </summary>
        /// <param name="type">Initial value to set.</param>
        public TypeId(string type)
        {
            Guid g = Guid.Empty;
            Guid.TryParse(type, out g);
            _type = g;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods to convert between various types: string <-> enum <-> guid
    /// </summary>
    public static partial class EnumExtensions
    {
        /// <summary>
        /// Attempts to convert an emum to its associated guid value. The enum and guid values
        /// are created from values in the database. If an enum is lacking the TypeId attribute,
        /// an empty guid is returned.
        /// </summary>
        /// <param name="e">Enum to use to find guid identity.</param>
        /// <returns>The database identity of the enum, or Guid.Empty.</returns>
        public static Guid ToGuid(this System.Enum e)
        {
            try
            {
                Type type = e.GetType();
                var memInfo = type.GetMember(e.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(TypeId), false);
                Guid id = ((TypeId)attributes[0]).Id;

                return id;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Attempts to convert a string to its associated enum value. The enum and guid values
        /// are created from values in the database.
        /// </summary>
        /// <typeparam name="T">Base enum to use.</typeparam>
        /// <param name="enumValue">Value of enum to attempt to convert.</param>
        /// <returns>Type of enum, or throws an exception.</returns>
        public static T ToEnum<T>(this string enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            try
            {
                T value = (T)System.Enum.Parse(typeof(T), enumValue, true);
                return value;
            }
            catch
            {
                throw new ArgumentException(String.Format("Could not convert value {0} to type {1}.", enumValue, typeof(T).Name));
            }
        }

        /// <summary>
        /// Attempts to convert a string to its associated guid value. The enum and guid values
        /// are created from values in the database. If an enum is lacking the TypeId attribute,
        /// an empty guid is returned.
        /// </summary>
        /// <typeparam name="T">Base enum to use.</typeparam>
        /// <param name="enumValue">Value of enum to attempt to convert.</param>
        /// <returns>The database identity of the enum, or Guid.Empty.</returns>
        public static Guid ToEnumTypeGuid<T>(this string enumValue) where T : struct, IConvertible
        {
            try
            {
                if (!typeof(T).IsEnum)
                {
                    return Guid.Empty;
                }

                T value = (T)System.Enum.Parse(typeof(T), enumValue, true);

                Type type = value.GetType();
                var memInfo = type.GetMember(value.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(TypeId), false);
                Guid id = ((TypeId)attributes[0]).Id;

                return id;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Attempts to convert a guid to its associated enum value. The enum and guid values
        /// are created from values in the database.
        /// </summary>
        /// <typeparam name="T">Base enum to use.</typeparam>
        /// <param name="typeId">Identifier of type stored in database.</param>
        /// <returns>Type of enum, or throws an exception.</returns>
        public static T ToEnum<T>(this Guid typeId) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            try
            {
                Type type = typeof(T);
                foreach (var field in type.GetFields())
                {
                    var attribute = Attribute.GetCustomAttribute(field, typeof(TypeId)) as TypeId;
                    if (attribute != null)
                    {
                        if (((TypeId)attribute).Id == typeId)
                            return (T)field.GetValue(null);
                    }
                }
                throw new ArgumentException("Not found.", "typeId");
                // or return default(T);
            }
            catch
            {
                throw new ArgumentException(String.Format("Could not convert value {0} to type {1}.", typeId, typeof(T).Name));
            }
        }
    }
