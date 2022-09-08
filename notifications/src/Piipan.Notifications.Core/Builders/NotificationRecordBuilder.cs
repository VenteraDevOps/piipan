using Piipan.Notification.Common.Models;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Builders
{
    public class NotificationRecordBuilder : INotificationRecordBuilder
    {
        private NotificationRecord _record = new NotificationRecord();
        /// <summary>
        /// Initializes a new instance of ActiveMatchRecordBuilder
        /// </summary>
        public NotificationRecordBuilder()
        {
            this.Reset();
        }

        /// <summary>
        /// Get the built record and reset internal record reference.
        /// </summary>
        /// <returns>Current NotificationRecord instance</returns>
        public NotificationRecord GetRecord()
        {
            NotificationRecord record = this._record;

            this.Reset();

            return record;
        }

        /// <summary>
        /// Reset the builder's internal record reference
        /// </summary>
        public void Reset()
        {
            this._record = new NotificationRecord();
        }
        /// <summary>
        /// Set the Disposition model's for Type 2 notifications
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="dispositionModel">Constructed DispositionModel model from the trigger</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public INotificationRecordBuilder SetDispositionModel(DispositionModel dispositionModel)
        {
            this._record.MatchResEvent = new DispositionModel();
            this._record.MatchResEvent = dispositionModel;
            return this;
        }
        /// <summary>
        /// Set the Notification record with Email Address 
        /// </summary>
        /// <param name="emailTo">Email Address To</param>
        /// <param name="emailCC">CC Email Address</param>
        /// <param name="emailBCC">BCC Email Address</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public INotificationRecordBuilder SetEmailToModel(string emailTo, string emailCC = null, string emailBCC = null)
        {
            this._record.EmailToRecord = new EmailToModel();
            this._record.EmailToRecord.EmailTo = emailTo;
            return this;
        }
        /// <summary>
        /// Set the Notification record with Email Address of Matcing State
        /// </summary>
        /// <param name="emailTo">Email Address To</param>
        /// <param name="emailCC">CC Email Address</param>
        /// <param name="emailBCC">BCC Email Address</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public INotificationRecordBuilder SetEmailMatchingStateModel(string emailTo, string emailCC = null, string emailBCC = null)
        {
            this._record.EmailToRecordMS = new EmailToModel();
            this._record.EmailToRecordMS.EmailTo = emailTo;
            return this;
        }
        /// <summary>
        /// Set the match model's match-related fields (MatchId, Iniatiating State, Matching State and Match URL)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="matchModel">Constructed Match model from the trigger</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public INotificationRecordBuilder SetMatchModel(MatchModel matchModel)
        {
            this._record.MatchRecord = new MatchModel();
            this._record.MatchRecord = matchModel;
            return this;
        }
    }
}
