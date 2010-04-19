namespace LanLordz.SiteTools
{
    using System;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Provides functionality for cleanly converting and parsing values.
    /// </summary>
    /// <typeparam name="T">The type to which to convert.</typeparam>
    public sealed class Converter
    {
        /// <summary>
        /// Prevents a default instance of the Converter class from being created.
        /// </summary>
        private Converter()
        {
        }

        /// <summary>
        /// Converts any compatible object to an instance of T.
        /// </summary>
        /// <typeparam name="T">The type to which to convert.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "The double cast is necessary, since the 'as' operator loses information on whether the object is actually null or not.")]
        public static T Convert<T>(object value)
        {
            if (value is T)
            {
                return (T)value;
            }

            Type t = typeof(T);

            if (t == typeof(string))
            {
                if (value == null || value is DBNull)
                {
                    return (T)(object)null;
                }
                else
                {
                    return (T)(object)value.ToString();
                }
            }
            else
            {
                if (value == null || value is DBNull)
                {
                    return default(T);
                }

                if (value is string && string.IsNullOrEmpty((string)value))
                {
                    return default(T);
                }

                try
                {
                    return (T)value;
                }
                catch (InvalidCastException)
                {
                }

                if (Nullable.GetUnderlyingType(t) != null)
                {
                    t = Nullable.GetUnderlyingType(t);
                }

                MethodInfo parse = t.GetMethod("Parse", new Type[] { typeof(string) });

                if (parse != null)
                {
                    try
                    {
                        object parsed = parse.Invoke(null, new object[] { value.ToString() });
                        return (T)parsed;
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
                else if (t.IsSubclassOf(typeof(Enum)))
                {
                    try
                    {
                        return (T)Enum.Parse(t, value.ToString());
                    }
                    catch (ArgumentException)
                    {
                    }

                    try
                    {
                        return (T)Enum.Parse(t, value.ToString(), true);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                decimal result = 0;

                if (decimal.TryParse(value.ToString(), out result))
                {
                    try
                    {
                        return (T)System.Convert.ChangeType(result, t, CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException)
                    {
                    }
                }

                throw new InvalidOperationException("The value you specified is not a valid " + typeof(T).ToString());
            }
        }

        /// <summary>
        /// Converts any compatible object to an instance of T.
        /// </summary>
        /// <typeparam name="T">The type to which to convert.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The default value to return if object is equal to null or is DBNull, or cannot be coverted.</param>
        /// <returns>The converted value or the defaultValue.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "The double cast is necessary, since the 'as' operator loses information on whether the object is actually null or not.")]
        public static T Convert<T>(object value, T defaultValue)
        {
            if (value is T)
            {
                return (T)value;
            }

            Type t = typeof(T);

            if (t == typeof(string))
            {
                if (value == null || value is DBNull)
                {
                    return defaultValue;
                }
                else
                {
                    return (T)(object)value.ToString();
                }
            }
            else
            {
                if (value == null || value is DBNull)
                {
                    return defaultValue;
                }

                if (value is string && string.IsNullOrEmpty((string)value))
                {
                    return defaultValue;
                }

                try
                {
                    return (T)value;
                }
                catch (InvalidCastException)
                {
                }

                if (Nullable.GetUnderlyingType(t) != null)
                {
                    t = Nullable.GetUnderlyingType(t);
                }

                MethodInfo parse = t.GetMethod("Parse", new Type[] { typeof(string) });

                if (parse != null)
                {
                    try
                    {
                        object parsed = parse.Invoke(null, new object[] { value.ToString() });
                        return (T)parsed;
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
                else if (t.IsSubclassOf(typeof(Enum)))
                {
                    try
                    {
                        return (T)Enum.Parse(t, value.ToString());
                    }
                    catch (ArgumentException)
                    {
                    }

                    try
                    {
                        return (T)Enum.Parse(t, value.ToString(), true);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                decimal result = 0;

                if (decimal.TryParse(value.ToString(), out result))
                {
                    try
                    {
                        return (T)System.Convert.ChangeType(result, t, CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException)
                    {
                    }
                }

                return defaultValue;
            }
        }
    }
}
