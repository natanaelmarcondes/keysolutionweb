namespace KeySolution.Api.Models
{
    public sealed class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public long TotalItems { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0)
                    return 0;

                return (int)Math.Ceiling(TotalItems / (double)PageSize);
            }
        }
    }
}