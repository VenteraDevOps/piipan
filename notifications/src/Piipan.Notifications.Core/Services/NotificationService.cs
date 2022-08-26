using Antlr4.StringTemplate;
using Microsoft.Extensions.Logging;
using Piipan.Match.Core.Services;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationPublish _notificationPublish;
        public ILogger<NotificationService> _logger;

        public NotificationService(INotificationPublish notificationPublish, ILogger<NotificationService> logger)
        {
            _notificationPublish = notificationPublish;
            _logger = logger;
        }

        public async Task<bool> PublishMessageFromTemplate(EmailTemplateInput emailTemplateInput)
        {
            return await TransformMessageAndPublishEmail(emailTemplateInput.Topic, emailTemplateInput.TemplateData, emailTemplateInput.EmailTo, emailTemplateInput.EmailCcTo, emailTemplateInput.EmailCcTo, emailTemplateInput.EmailBccTo, '$', '$');
        }

        public async Task<bool> TransformMessageAndPublishEmail(string topic, object templateData,
         string EmailTo, string emailFrom, string emailCcTo = null, string emailBccTo = null,
         char templateDelimiterStartChar = '$', char templateDelimiterStopChar = '$')
        {
            TemplateGroup g = new TemplateGroup(templateDelimiterStartChar, templateDelimiterStopChar);
            g.RegisterRenderer(typeof(string), new StringRenderer());
            g.RegisterRenderer(typeof(double), new NumberRenderer());
            g.RegisterRenderer(typeof(decimal), new NumberRenderer());
            g.RegisterRenderer(typeof(float), new NumberRenderer());
            g.RegisterRenderer(typeof(double), new NumberRenderer());
            g.RegisterRenderer(typeof(long), new NumberRenderer());
            g.RegisterRenderer(typeof(int), new NumberRenderer());
            g.RegisterRenderer(typeof(DateTime), new DateRenderer());
            g.RegisterRenderer(typeof(DateTimeOffset), new DateRenderer());

            //Below code needs to be refactored based on where the templates are stored.
            string emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
            string emailSubject = "Duplicate Match $data.MatchId$ Found in $data.InitState$ (Initiating State)";
            if (topic == "UPDATE_MATCH_RES_IS")
            {
                emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
                emailSubject = "Duplicate Match $data.MatchId$ is updated in $data.InitState$ (Initiating State)";
            }
            else if (topic == "UPDATE_MATCH_RES_MS")
            {
                emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
                emailSubject = "Duplicate Match $data.MatchId$ is updated in $data.MatchingState$ (Matching State)";
            }
            else if (topic == "CREATE_MATCH_MS")
            {
                emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
                emailSubject = "Duplicate Match $data.MatchId$ Found in $data.MatchingState$ (Matching State)";
            }
            else if (topic == "CREATE_MATCH_IS")
            {
                emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
                emailSubject = "Duplicate Match $data.MatchId$ Found in $data.InitState$ (Initiating State)";
            }

            Template bodyTemplate = new Template(g, emailBody);
            bodyTemplate.Add("data", templateData);

            Template subjectTemplate = new Template(g, emailSubject);
            subjectTemplate.Add("data", templateData);

            try
            {
                var emailModel = new EmailModel
                {
                    ToList = EmailTo.Split(',').ToList(),
                    ToCCList = emailCcTo?.Split(',').ToList(),
                    ToBCCList = emailBccTo?.Split(',').ToList(),
                    Body = bodyTemplate.Render(),
                    Subject = subjectTemplate.Render(),
                    From = string.Empty,
                };
                // Publish to the queue.
                await _notificationPublish.PublishEmail(emailModel);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Participant Search metrics event to EventGrid.");
                throw;
            }
        }
    }
}