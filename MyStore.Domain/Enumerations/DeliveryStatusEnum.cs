namespace MyStore.Domain.Enumerations
{
    public enum DeliveryStatusEnum
    {
        Processing,
        Confirmed,
        AwaitingPickup,
        Shipping,
        BeingDelivered,
        Received,
        Canceled,
    }
}
