using System;

namespace RegistrDisconnection.MyClasses
{
    /// <summary>
    /// пагінація сторінок
    /// </summary>
    public class PageViewModel
    {
        public int PageNumber { get; private set; }     //номер текучої сторінки
        public int TotalPages { get; private set; }     //загальна к-ть сторінок

        public PageViewModel(int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}
