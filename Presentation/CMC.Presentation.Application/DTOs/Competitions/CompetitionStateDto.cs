using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class CompetitionStateDto
    {
        public int Id { get; set; }
        public string CityMallPlayed { get; set; }
        public string OtherTeamPlayed { get; set; }
        public bool IsBattled { get; set; }
        public int cityMallSelectedId { get; set; }
        public int OtherTeamSelectedId { get; set; }
        public int QuestionId { get; set; }
        public int CagegoryId { get; set; }
        public string CurrentStep { get; set; }
        public int CurrentRound { get; set; }
    }
}
