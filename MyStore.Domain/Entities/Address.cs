using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Address : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public string UserId { get; set; }
        public User User { get; set; }

        //public bool IsDeFault { get; set; } = false;

        //public int Province_id { get; set; }
        //public string Province_name { get; set; }

        //public int District_id { get; set; }
        //public string District_name { get; set; }

        //public int Ward_id { get; set; }
        //public string Ward_name { get; set; }


        //[MaxLength(100)]
        //public string Detail { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
