using Microsoft.Extensions.Logging;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Deidentification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Piipan.Participants.Core.Services
{
    public class ParticipantService : IParticipantApi
    {
        private readonly IParticipantDao _participantDao;
        private readonly IUploadDao _uploadDao;
        private readonly IStateService _stateService;
        private readonly IRedactionService _redactionService;
        private readonly ILogger<ParticipantService> _logger;

        public ParticipantService(
            IParticipantDao participantDao,
            IUploadDao uploadDao,
            IStateService stateService,
            IRedactionService redactionService,
            ILogger<ParticipantService> logger)
        {
            _participantDao = participantDao;
            _uploadDao = uploadDao;
            _stateService = stateService;
            _redactionService = redactionService;
            _logger = logger;
        }

        public async Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash)
        {
            try
            {
                var upload = await _uploadDao.GetLatestUpload(state);
                var participants = await _participantDao.GetParticipants(state, ldsHash, upload.Id);

                // Set the participant State before returning
                return participants.Select(p => new ParticipantDto(p) { State = state });
            }
            catch (InvalidOperationException)
            {
                return Enumerable.Empty<IParticipant>();
            }
        }

        public async Task AddParticipants(IEnumerable<IParticipant> participants, string uploadIdentifier, string fileName)
        {
            DateTime uploadTime = DateTime.UtcNow;
            // Large participant uploads can be long-running processes and require
            // an increased time out duration to avoid System.TimeoutException
            var upload = await _uploadDao.AddUpload(uploadIdentifier);
            var participantList = participants.ToList();
            try
            {
                using (TransactionScope scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    TimeSpan.FromSeconds(600),
                    TransactionScopeAsyncFlowOption.Enabled))
                {


                    var participantDbos = participantList.Select(p => new ParticipantDbo(p)
                    {
                        UploadId = upload.Id
                    });

                    await _participantDao.AddParticipants(participantDbos);
                    await _uploadDao.UpdateUploadStatus(upload, UploadStatuses.COMPLETE.ToString());
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                await _uploadDao.UpdateUploadStatus(upload, UploadStatuses.FAILED.ToString());

                // Log the redacted error string as an error.
                List<string> redactedStrings = new List<string>();
                foreach (var participant in participantList)
                {
                    redactedStrings.Add(participant.LdsHash);
                    redactedStrings.Add(participant.ParticipantId);
                    redactedStrings.Add(participant.CaseId);
                }
                string state = Environment.GetEnvironmentVariable("State");
                var endTime = DateTime.UtcNow;
                var uploadErrorDetails = new ParticipantUploadErrorDetails(state, uploadTime, endTime, ex, fileName);

                // Since ParticipantUploadErrorDetails is a record, ToString outputs JSON
                string uploadErrorString = uploadErrorDetails.ToString();
                uploadErrorString = _redactionService.Redact(uploadErrorString, redactedStrings);
                _logger.LogError($"Error uploading participants: {uploadErrorString}");
            }
        }

        public async Task<IEnumerable<string>> GetStates()
        {
            return await _stateService.GetStates();
        }
        public async Task DeleteOldParticpants(string state = null)
        {

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                TimeSpan.FromSeconds(600),
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var upload = await _uploadDao.GetLatestUpload(state);
                await _participantDao.DeleteOldParticipantsExcept(state, upload.Id);
                scope.Complete();
            }
        }
    }
}
