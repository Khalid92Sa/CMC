using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Identity
{
    public class OneTimePasswordDTO
    {
        public int Id { get; set; }
        public string SecurityCode { get; set; }
        public int NoOfTrials { get; set; }
        public int NoOfGenerations { get; set; }
        public string MobileNumber { get; set; }
        public string HashedMobileNumber { get; set; }
        public string ModalMessage { get; set; }
        public DateTime? UnlockedDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int MaxNumberOfTrials { get; set; }
        public int CodeExpiryInMinutes { get; set; }
        public int NumberOfDigits { get; set; }
        public int NumberOfTrials { get; set; }
        public int ElapsedTime { get; set; }
        public bool isBlocked { get; set; }
        public int BackPage { get; set; }


        public bool SecurityCodeExpired { set; get; }
        public bool SecurityCodeMatch { set; get; }
        public bool MaxNumberOfTrailsExceeded { set; get; }

    }
}
