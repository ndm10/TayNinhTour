namespace TayNinhTourApi.BusinessLogicLayer.DTOs
{
    public class GenericResponseDto<T>
    {
        public T? Data { get; set; }
        public int PageSize { get; set; } = 0;
        public int PageCount { get; set; } = 0;
    }
}
