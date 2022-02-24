using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using InfluxDB.Client.Core;

namespace InfluxDB.Client.Writes
{
    /// <summary>
    /// The setting for store data point: default values, threshold, ...
    /// </summary>
    public class PointSettings
    {
        private readonly SortedDictionary<string, string> _defaultTags =
            new SortedDictionary<string, string>(StringComparer.Ordinal);

        private static readonly Regex AppSettingsRegex = new Regex("^(\\${)(?<Value>.+)(})$",
            RegexOptions.ExplicitCapture |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.RightToLeft);

        private static readonly Regex EnvVariableRegex = new Regex("^(\\${env.)(?<Value>.+)(})$",
            RegexOptions.ExplicitCapture |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.RightToLeft);

        /// <summary>
        /// Add default tag. 
        /// </summary>
        /// <param name="key">the tag name</param>
        /// <param name="expression">the tag value expression</param>
        /// <returns>this</returns>
        public PointSettings AddDefaultTag(string key, string expression)
        {
            Arguments.CheckNotNull(key, "tagName");
            _defaultTags[key] = expression;
            return this;
        }

        /// <summary>
        /// Get default tags with evaluated expressions.
        /// </summary>
        /// <returns>evaluated default tags</returns>
        internal IDictionary<string, string> GetDefaultTags()
        {
            if (_defaultTags.Count == 0)
            {
                return ImmutableDictionary<string, string>.Empty;
            }

            string Evaluation(string expression)
            {
                if (string.IsNullOrEmpty(expression))
                {
                    return null;
                }

                var matcher = EnvVariableRegex.Match(expression);
                if (matcher.Success)
                {
                    return Environment.GetEnvironmentVariable(matcher.Groups["Value"].Value);
                }

                matcher = AppSettingsRegex.Match(expression);
                if (matcher.Success)
                {
                    return ConfigurationManager.AppSettings[matcher.Groups["Value"].Value];
                }

                return expression;
            }

            return _defaultTags
                .Select(it => new KeyValuePair<string, string>(it.Key, Evaluation(it.Value)))
                .Where(it => !string.IsNullOrEmpty(it.Value))
                .ToDictionary(it => it.Key, it => it.Value, StringComparer.Ordinal);
        }
    }
}