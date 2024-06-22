namespace MyStore.Domain.Entities
{
    public interface IBaseEntity
    {
        DateTime? CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
