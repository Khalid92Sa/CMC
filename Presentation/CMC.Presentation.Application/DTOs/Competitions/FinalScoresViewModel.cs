using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class FinalScoresViewModel
    {
        public int Id { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public bool IsFinalCompetition { get; set; }
        public List<CompetitionsPlayerDTO> TeamCityMall { get; set; }
        public List<CompetitionsPlayerDTO> OtherTeam { get; set; }
        public string CompetitionName { get; set; }
        public DateTime? CompetitionDate { get; set; }
        public int TotalQuestions { get; set; }
        public TimeSpan TotalDuration { get; set; }
    }
}
