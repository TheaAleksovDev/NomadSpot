using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Author { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime Date { get; set; }
    }
}
