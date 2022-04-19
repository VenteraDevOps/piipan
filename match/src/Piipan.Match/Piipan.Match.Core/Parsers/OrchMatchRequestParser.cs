using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models;
using Newtonsoft.Json;
using FluentValidation;
using System.Linq;
using Piipan.Match.Core.Enums;

namespace Piipan.Match.Core.Parsers
{
    /// <summary>
    /// Parser for deserializing OrchMatchRequest objects from a Stream
    /// </summary>
    public class OrchMatchRequestParser : IStreamParser<OrchMatchRequest>
    {
        private readonly IValidator<OrchMatchRequest> _validator;
        private readonly ILogger<OrchMatchRequestParser> _logger;
        
        /// <summary>
        /// Initializes a new instance of OrchMatchRequestParser
        /// </summary>
        public OrchMatchRequestParser(
            IValidator<OrchMatchRequest> validator,
            ILogger<OrchMatchRequestParser> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Deserializes and validates an OrchMatchRequest from a Stream
        /// </summary>
        /// <remarks>
        /// Throws ValidationException if FluentValidation fails; throws StreamParserException for all other failures
        /// </remarks>
        /// <param name="stream">A Stream from which a serialized OrchMatchRequest can be read</param>
        /// <returns>A validated instance of OrchMatchRequest</returns>
        public async Task<OrchMatchRequest> Parse(Stream stream)
        {
            try
            {
                OrchMatchRequest request = null;

                var reader = new StreamReader(stream);
                var serialized = await reader.ReadToEndAsync();

                request = JsonConvert.DeserializeObject<OrchMatchRequest>(serialized);

                if (request is null)
                {
                    throw new JsonSerializationException("stream must not be empty.");
                }
                
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException("request validation failed", validationResult.Errors);
                }
                ///Checking search_reason for valid reason. If reason given is not 
                ///in allowed list of search reasons then setting reason to null
                for (int i = 0; i < request.Data.Count; i++)
                {
                    if (request.Data[i].SearchReason != null)
                    {
                        request.Data[i].SearchReason = request.Data[i].SearchReason.ToLower();
                        if (!Enum.IsDefined(typeof(ValidSearchReasons), request.Data[i].SearchReason))
                        {
                            var validValues = Enum.GetNames(typeof(ValidSearchReasons));
                            string validOptionsString = string.Join(", ", validValues.Select(n => validValues.Last() == n ? $"or '{n}'" : $"'{n}'"));
                            throw new Exception($"Submitted option for 'search_reason' is not valid. Valid options are {validOptionsString}.");
                        }
                    }
                }

                return request;
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Found validation errors while parsing: {ex.Errors.ToString()}");
                throw ex;
            }
            catch (Exception ex)
            {
                throw new StreamParserException(ex.Message, ex);
            }
        }
    }
}