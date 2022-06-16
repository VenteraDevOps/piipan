using System.Collections.Generic;

namespace Piipan.Shared.Deidentification
{
    public interface IRedactionService
    {
        /// <summary>
        /// Return a string with redactions
        /// </summary>
        /// <param name="originalValue">The input string to check for redactions</param>
        /// <param name="redactStrings">All of the values that need to be redacted</param>
        /// <returns></returns>
        public string Redact(string originalValue, IEnumerable<string> redactStrings);
    }
}
