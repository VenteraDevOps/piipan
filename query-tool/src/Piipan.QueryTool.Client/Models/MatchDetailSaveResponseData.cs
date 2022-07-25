namespace Piipan.QueryTool.Client.Models
{
    public class MatchDetailSaveResponseData
    {
        public bool? SaveSuccess { get; set; }

        // The values you were attempting to save when a save fails
        public DispositionModel FailedDispositionModel { get; set; }
    }
}
