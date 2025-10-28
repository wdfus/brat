using System;
using System.Collections.Generic;

namespace Brat.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ChatId { get; set; }

    public int FromUserId { get; set; }

    public int UserId { get; set; }

    public string MessageText { get; set; } = null!;
/*    public string CaptionPath { get; set; } = null!;
    public string MessageType { get; set; } = null!;*/
    public string Status { get; set; } = null!;
    public DateTime? SentTime { get; set; } = null!;

    public virtual Chat Chat { get; set; } = null!;

    public virtual User FromUser { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<MessageAttachment> MessageFiles { get; set; } = new List<MessageAttachment>();
}
