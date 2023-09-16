using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class CompetitionListDTO
    {
        public int Id { get; set; }
        public string HostName { get; set; }
        public string CompetitionName { get; set; }
        public string CompetitionStartDate { get; set; }
        public string CompetitionEndDate { get; set;}
        public bool IsFinished { get; set; }
    }
}
