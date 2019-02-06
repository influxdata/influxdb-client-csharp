using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace InfluxDB.Client.Core
{
    /// <summary>
    /// Functions for parameter validation.
    ///
    /// <para>
    /// Inspiration from InfluxDB java - <a href="https://github.com/influxdata/influxdb-java/">thanks</a>
    /// </para>
    /// </summary>
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
    public static class Arguments
    {
        private const string DurationPattern = @"([-+]?)([0-9]+(\\.[0-9]*)?[a-z]+)+";

        /// <summary>
        /// Enforces that the string is not empty.
        /// </summary>
        /// <param name="value">the string to test</param>
        /// <param name="name">the variable name for reporting</param>
        /// <exception cref="ArgumentException">if the string is empty</exception>
        public static void CheckNonEmptyString(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Expecting a non-empty string for " + name);
            }
        }

        /// <summary>
        /// Enforces that the string is duration literal.
        /// </summary>
        /// <param name="value">the string to test</param>
        /// <param name="name">the variable name for reporting</param>
        /// <exception cref="ArgumentException">if the string is not duration literal</exception>
        public static void CheckDuration(string value, string name)
        {
            if (string.IsNullOrEmpty(value) || !Regex.Match(value, DurationPattern).Success)
            {
                throw new ArgumentException("Expecting a duration string for " + name + ". But got: " + value);
            }
        }

        /// <summary>
        /// Enforces that the number is larger than 0.
        /// </summary>
        /// <param name="number">the number to test</param>
        /// <param name="name">the variable name for reporting</param>
        /// <exception cref="ArgumentException">if the number is less or equal to 0</exception>
        public static void CheckPositiveNumber(int number, string name)
        {
            if (number <= 0)
            {
                throw new ArgumentException("Expecting a positive number for " + name);
            }
        }

        /// <summary>
        /// Enforces that the number is not negative.
        /// </summary>
        /// <param name="number">the number to test</param>
        /// <param name="name">the variable name for reporting</param>
        /// <exception cref="ArgumentException">if the number is less or equal to 0</exception>
        public static void CheckNotNegativeNumber(int number, string name)
        {
            if (number < 0)
            {
                throw new ArgumentException("Expecting a positive or zero number for " + name);
            }
        }

        /// <summary>
        /// Checks that the specified object reference is not null.
        /// </summary>
        /// <param name="obj">the object to test</param>
        /// <param name="name">the variable name for reporting</param>
        /// <exception cref="NullReferenceException">if the object is null</exception>
        public static void CheckNotNull(object obj, string name)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Expecting a not null reference for " + name);
            }
        }
    }
}