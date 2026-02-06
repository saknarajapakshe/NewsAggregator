using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAggregator.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Source { get; set; } = null!;
        public DateTime PublishedAt { get; set; }
        public DateTime CreatedAt { get; set;   }
    }
}