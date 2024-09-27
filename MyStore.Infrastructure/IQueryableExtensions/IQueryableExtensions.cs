using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;

namespace MyStore.Infrastructure.IQueryableExtensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int currentPage, int pageSize) 
            => query.Skip((currentPage - 1) * pageSize).Take(pageSize);

        //public static string GetVietnameseStatus(this DeliveryStatusEnum status)
        //{
        //    return status switch
        //    {
        //        DeliveryStatusEnum.Processing => "Đang xử lý",
        //        DeliveryStatusEnum.Confirmed => "Đã xác nhận",
        //        DeliveryStatusEnum.AwaitingPickup => "Chờ lấy hàng",
        //        DeliveryStatusEnum.Shipping => "Đang vận chuyển",
        //        DeliveryStatusEnum.BeingDelivered => "Đang giao hàng đến bạn",
        //        DeliveryStatusEnum.Received => "Đã nhận hàng",
        //        DeliveryStatusEnum.Canceled => "Đã hủy",
        //        _ => "Không xác định"
        //    };
        //}
    }
}
