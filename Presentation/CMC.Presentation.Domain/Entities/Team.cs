using CMC.Kernel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class Team : FullAuditableEntity<int>
    {
        public virtual int? Player1Id { get; set; }
        public virtual Player Player1 { get; set; }

        public virtual int? Player2Id { get; set; }
        public virtual Player Player2 { get; set; }

        public virtual int? Player3Id { get; set; }
        public virtual Player Player3 { get; set; }

        public virtual int? Player4Id { get; set; }
        public virtual Player Player4 { get; set; }
    }
}
