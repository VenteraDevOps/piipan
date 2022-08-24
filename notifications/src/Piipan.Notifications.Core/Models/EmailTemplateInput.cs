namespace Piipan.Notifications.Models
{
    public class EmailTemplateInput
    {
        public string Topic { get; set; }
        public object TemplateData { get; set; }
        public string EmailTo { get; set; }
        public string EmailCcTo { get; set; }
        public string EmailBccTo { get; set; }


        public EmailTemplateInput()
        {
        }
    }
}