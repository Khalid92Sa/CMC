using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class RoundScoresModalViewModel
    {
        public int CurrentRound { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public int TotalCompletedQuestions { get; set; }
        public bool IsFinalCompetition { get; set; }
        public List<RoundScoresPlayerViewModel> TeamCityMall { get; set; } = new List<RoundScoresPlayerViewModel>();
        public List<RoundScoresPlayerViewModel> OtherTeam { get; set; } = new List<RoundScoresPlayerViewModel>();
    }

    public class RoundScoresPlayerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfilePicture { get; set; }
        public int Points { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double Time { get; set; }
    }
}
