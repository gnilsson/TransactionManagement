﻿using System.ComponentModel.DataAnnotations;

namespace API.Data;

public sealed class User : IIdentifiableEntity, ITemporalEntity, IRowVersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Role { get; set; } = default!;

    public ICollection<Account> Accounts { get; } = [];

    public DateTime CreatedAt { get; }

    public DateTime ModifiedAt { get; set; }

    public long RowVersion { get; }
}
