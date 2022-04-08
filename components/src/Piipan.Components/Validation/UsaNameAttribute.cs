using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piipan.Components.Validation
{
    public class UsaNameAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var stringValue = value?.ToString();

            // If the name is required, we'll pick it up with a UsaRequired attribute. Don't flag it here
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            // Loud failure for non-ascii chars
            string nonasciirgx = @"[^\x00-\x7F]";
            MatchCollection matches = Regex.Matches(stringValue, nonasciirgx, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                List<string> invalidValues = new List<string>();
                foreach (Match match in matches)
                {
                    if (!invalidValues.Contains(match.Value))
                    {
                        invalidValues.Add(match.Value);
                    }
                }
                ErrorMessage = string.Format(ValidationConstants.InvalidCharacterInNameMessage, string.Join(',', invalidValues), stringValue);
                return false;
            }
            // Convert to lower case
            string result = stringValue.ToLower();
            // Replace hyphens with a space
            result = result.Replace("-", " ");
            // Replace multiple spaces with one space
            result = Regex.Replace(result, @"\s{2,}", " ");
            // Trim any spaces at the start and end of the last name
            char[] charsToTrim = { ' ' };
            result = result.Trim(charsToTrim);
            // Remove suffixes: roman numerals i-ix, variations of junior/senior
            result = Regex.Replace(result, @"(\s(?:ix|iv|v?i{0,3}|junior|jr\.|jr|jnr|senior|sr\.|sr|snr)$)", "");
            // Remove any character not an ASCII space(0x20) or not in range[a - z]
            result = Regex.Replace(result, @"[^a-z|\s]", "");
            // Validate that the resulting value is at least one ASCII character in length
            if (result.Length < 1) // not at least one char
            {
                ErrorMessage = ValidationConstants.NormalizedNameTooShortMessage;
                return false;
            }
            return true;
        }
    }
}
