using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Dashboard.Pages
{
    public class ParticipantUploadsModel : BasePageModel
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;
        private readonly ILogger<ParticipantUploadsModel> _logger;

        public ParticipantUploadsModel(IParticipantUploadReaderApi participantUploadApi,
            ILogger<ParticipantUploadsModel> logger,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _participantUploadApi = participantUploadApi;
            _logger = logger;
        }
        public string Title = "Most recent upload from each state";
        public List<ParticipantUpload> ParticipantUploadResults { get; private set; } = new List<ParticipantUpload>();
        public ParticipantUploadStatistics UploadStatistics { get; set; } = new();
        public string? PageParams { get; private set; }
        public string? StateQuery { get; private set; }
        public long TotalPages { get; set; }
        public string? RequestError { get; private set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading initial results");

                RequestError = null;

                var query = HttpContext?.Request?.Query;
                if (query?.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(query.ToDictionary(q => q.Key, q => q.Value.ToString()));
                    UploadRequest = JsonConvert.DeserializeObject<ParticipantUploadRequestFilter>(json);
                }

                var response = await _participantUploadApi.GetUploads(UploadRequest);
                await GetUploadStatistics();
                ParticipantUploadResults = response.Data.ToList();
                SetPageLinks(response.Meta);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "There was an error loading data. You may be able to try again. If the problem persists, please contact system maintainers.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "Internal Server Error. Please contact system maintainers.";
            }
        }

        private async Task GetUploadStatistics()
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            ParticipantUploadStatisticsRequest statisticsRequest = new ParticipantUploadStatisticsRequest
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date,
                HoursOffset = (int)offset.TotalHours
            };
            UploadStatistics = await _participantUploadApi.GetUploadStatistics(statisticsRequest);
        }

        [BindProperty]
        public ParticipantUploadRequestFilter UploadRequest { get; set; } = new ParticipantUploadRequestFilter();

        private void SetPageLinks(Meta meta)
        {
            PageParams = meta.PageQueryParams;
            TotalPages = meta.Total;
        }
    }
}
