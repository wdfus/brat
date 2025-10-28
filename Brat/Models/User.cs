using System;
using System.Collections.Generic;

namespace Brat.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? SecondName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateOnly? Birthday { get; set; }
    public string? AboutSelf { get; set; }

    public virtual ICollection<Chat> ChatUserId1Navigations { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatUserId2Navigations { get; set; } = new List<Chat>();

    public virtual ICollection<Message> MessageFromUsers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageUsers { get; set; } = new List<Message>();
    public virtual ICollection<FileAsset> Files { get; set; } = new List<FileAsset>();

}
