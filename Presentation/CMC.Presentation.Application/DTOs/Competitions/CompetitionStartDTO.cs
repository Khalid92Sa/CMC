using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Questions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class CompetitionStartDTO
    {
        public int Id { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public List<CompetitionsPlayerDTO> TeamCityMall { get; set; } = new List<CompetitionsPlayerDTO>();
        public List<CompetitionsPlayerDTO> OtherTeam { get; set; } = new List<CompetitionsPlayerDTO>();
        public List<LookupModel> Categories { get; set; } = new List<LookupModel>();
        public List<QuestionVM> Questions { get; set; } = new List<QuestionVM>();
        public List<QuestionVM> TotalCurrentCompetitionQuestions { get; set; } = new List<QuestionVM>();
        public QuestionVM CurrentQuestion { get; set; } = new QuestionVM();
        public int TotalQuestion { get; set; }
        public int TotalRound { get; set; }
        public bool IsFinalCompetition { get; set; }
        public bool IsQuestionsTypeIsRound { get; set; }
        public int QuestionPerPlayer { get; set; }
        public int CurrentRound { get; set; }
        public int RoundTime { get; set; }
        public int RoundPoints { get; set; }
    }

    public class CompetitionsPlayerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public double Time { get; set; }
        public bool IsVSPlayer { get; set; }
        public bool IsStarting { get; set; }
        public List<CompetitonQuestions> competitonQuestions { get; set; } = new List<CompetitonQuestions>();
    }

    public class CompetitonQuestions
    {
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionImg { get; set; }
        public bool IsQuestionImg { get; set; }
        public string AnswerText { get; set; }
        public string AnswerImg { get; set; }
        public bool IsAnswerImg { get; set; }
        public bool? IsCorrectAnswer { get; set; }
        public int? Points { get; set; }
        public double? Time { get; set; }
    }
}
