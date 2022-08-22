using Antlr4.StringTemplate;
using Piipan.Match.Core.Services;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationPublish _notificationPublish;

        public NotificationService(INotificationPublish notificationPublish)
        {
            _notificationPublish = notificationPublish;

        }
        public async Task<bool> CreateMessageFromTemplate(EmailTemplateInput emailTemplateInput)
        {
            return await MessageFromTemplate(emailTemplateInput.Topic, emailTemplateInput.TemplateData, emailTemplateInput.EmailTo, emailTemplateInput.EmailCcTo, emailTemplateInput.EmailCcTo, emailTemplateInput.EmailBccTo, '$', '$');
        }

        public async Task<bool> MessageFromTemplate(string topic, object templateData,
         string EmailTo, string emailFrom, string emailCcTo = null, string emailBccTo = null,
         char templateDelimiterStartChar = '$', char templateDelimiterStopChar = '$')
        {

            string emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
            string emailSubjectIS = "Duplicate Match $data.MatchId$ Found in $data.InitState$ (Initiating State)";
            string emailSubjectMS = "Duplicate Match $data.MatchId$ Found in $data.MatchingState$ (Matching State)";

            if (topic == "UPDATE_MATCH_RES")
            {
                emailBody = " Test Email for #$data.MatchId$  IS is $data.InitState$ <br>   MS is $data.MatchingState$ <br>   MS is $data.MatchingUrl$";
                emailSubjectIS = "Duplicate Match $data.MatchId$ is updated in $data.InitState$ (Initiating State)";
                emailSubjectMS = "Duplicate Match $data.MatchId$ is updated in $data.MatchingState$ (Matching State)";

            }

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

            Template bodyTemplate = new Template(g, emailBody);
            bodyTemplate.Add("data", templateData);

            Template subjectTemplateIS = new Template(g, emailSubjectIS);
            subjectTemplateIS.Add("data", templateData);

            Template subjectTemplateMS = new Template(g, emailSubjectMS);
            subjectTemplateMS.Add("data", templateData);

            try
            {
                var emailModel = new EmailModel
                {
                    ToList = EmailTo.Split(',').ToList(),
                    ToCCList = emailCcTo?.Split(',').ToList(),
                    ToBCCList = emailBccTo?.Split(',').ToList(),
                    Body = bodyTemplate.Render(),
                    Subject = subjectTemplateIS.Render(),
                    From = string.Empty,
                };
                // Publish to the queue.
                await _notificationPublish.PublishEmail(emailModel);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}