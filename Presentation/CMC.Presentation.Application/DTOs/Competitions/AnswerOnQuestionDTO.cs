using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class AnswerOnQuestionDTO
    {
        public int PlayerId { get; set; }
        public bool IsCityMallPlayer { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public bool? IsCorrectAnswer { get; set; }
        public int? Points { get; set; }
        public bool TimerFinished { get; set; }
        public int Trails { get; set; }
    }
}
