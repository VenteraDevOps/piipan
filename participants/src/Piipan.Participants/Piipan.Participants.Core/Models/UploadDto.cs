using System;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.Models
{
    public class UploadDto : IUpload
    {
        public UploadDto()
        {

        }

        public UploadDto(IUpload upload)
        {
            Id = upload.Id;
            UploadIdentifier = upload.UploadIdentifier;
            CreatedAt = upload.CreatedAt;
            Publisher = upload.Publisher;
            ParticipantsUploaded = upload.ParticipantsUploaded;
            ErrorMessage = upload.ErrorMessage;
            CompletedAt = upload.CompletedAt;
            Status = upload.Status;
        }

        public long Id { get; set; }
        public string UploadIdentifier { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Publisher { get; set; }
        public long? ParticipantsUploaded { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            UploadDto p = obj as UploadDto;
            if (p == null)
            {
                return false;
            }

            return
                Id == p.Id &&
                UploadIdentifier == p.UploadIdentifier &&
                CreatedAt == p.CreatedAt &&
                CompletedAt == p.CompletedAt &&
                Status == p.Status &&
                Publisher == p.Publisher &&
                ErrorMessage == p.ErrorMessage &&
                ParticipantsUploaded == p.ParticipantsUploaded;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                UploadIdentifier,
                CreatedAt,
                CompletedAt,
                Status,
                Publisher,
                ErrorMessage,
                ParticipantsUploaded
            );

        }
    }
}