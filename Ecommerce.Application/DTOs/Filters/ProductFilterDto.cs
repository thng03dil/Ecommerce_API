using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Filters
{
    public class ProductFilterDto
    {
        public string? Keyword { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; } = "Id";

        public string? SortOrder { get; set; } = "asc";

    }
}
