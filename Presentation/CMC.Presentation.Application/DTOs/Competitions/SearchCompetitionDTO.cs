using CMC.Kernel.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class SearchCompetitionDTO : PagedRequest
    {
        public string CompetitionName { get; set; }
        public DateTime? CompetitonStartDate { get; set; }
        public int? HostId { get; set; }
    }
}
