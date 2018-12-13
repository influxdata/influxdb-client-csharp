using System;
using System.Text.RegularExpressions;

namespace Platform.Common.Platform
{
    /// <summary>
    /// Functions for parameter validation.
    ///
    /// <para>
    /// Inspiration from InfluxDB java - <a href="https://github.com/influxdata/influxdb-java/">thanks</a>
    /// </para>
    /// </summary>
    public static class Arguments
    {
        private const string DurationPattern = @"([-+]?)([0-9]+(\\.[0-9]*)?[a-z]+)+";

        /**
         * Enforces that the string is {@linkplain String#isEmpty() not empty}.
         *
         * @param value the string to test
         * @param name   variable name for reporting
         * @return {@code string}
         * @throws ArgumentException if the string is empty
         */
        public static string CheckNonEmptyString(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Expecting a non-empty string for " + name);
            }

            return value;
        }

        /**
         * Enforces that the string has exactly one char.
         *
         * @param value the string to test
         * @param name   variable name for reporting
         * @return {@code string}
         * @throws ArgumentException if the string has not one char
         */
        public static string CheckOneCharString(string value, string name)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 1)
            {
                throw new ArgumentException("Expecting a one char string for " + name);
            }

            return value;
        }

        /**
         * Enforces that the string is duration literal.
         *
         * @param value the string to test
         * @param name   variable name for reporting
         * @return {@code string}
         * @throws ArgumentException if the string is not duration literal
         */
        public static string CheckDuration(string value, string name)
        {
            if (string.IsNullOrEmpty(value) || !Regex.Match(value, DurationPattern).Success)
            {
                throw new ArgumentException("Expecting a duration string for " + name + ". But got: " + value);
            }

            return value;
        }

        /**
         * Enforces that the number is larger than 0.
         *
         * @param number the number to test
         * @param name   variable name for reporting
         * @throws ArgumentException if the number is less or equal to 0
         */
        public static void CheckPositiveNumber(int number, string name)
        {
            if (number <= 0)
            {
                throw new ArgumentException("Expecting a positive number for " + name);
            }
        }

        /**
         * Enforces that the number is not negative.
         *
         * @param number the number to test
         * @param name   variable name for reporting
         * @throws ArgumentException if the number is less or equal to 0
         */
        public static void CheckNotNegativeNumber(int number, string name)
        {
            if (number < 0)
            {
                throw new ArgumentException("Expecting a positive or zero number for " + name);
            }
        }

        /**
         * Checks that the specified object reference is not {@code null}.
         *
         * @param obj  the object reference to check for nullity
         * @param name variable name for reporting
         * @throws NullPointerException if the object is {@code null}
         * @see Objects#requireNonNull(Object, String)
         */
        public static void CheckNotNull(object obj, string name)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Expecting a not null reference for " + name);
            }
        }
    }
}