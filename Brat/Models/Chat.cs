using System;
using System.Collections.Generic;

namespace Brat.Models;

public partial class Chat
{
    public int ChatId { get; set; }

    public int UserId1 { get; set; }

    public int UserId2 { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User UserId1Navigation { get; set; } = null!;

    public virtual User UserId2Navigation { get; set; } = null!;
}
