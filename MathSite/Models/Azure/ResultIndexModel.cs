using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathSite.Models.Azure
{
    public class ResultIndexModel
    {
        [SearchableField]
        public string Id { get; set; }

        [SearchableField]
        public string TaskName { get; set; }

        [SearchableField]
        public string Condition { get; set; }

        [SearchableField]
        public string Type { get; set; }

        [SearchableField]
        public bool isDeleted { get; set; }

        [SearchableField]
        public int TaskId { get; set; }
    }
}
