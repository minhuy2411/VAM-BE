using System;

namespace VAM.Entities
{
    public abstract class BaseEntity
    {
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int? UpdatedBy { get; set; }
    }
}
