using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brat.Models
{
    public partial class MessageAttachment
    {
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public long FileId { get; set; }

        public int MessageId { get; set; }

        // Навигационные свойства
        public virtual FileAsset File { get; set; } = null!;

        public virtual Message Message { get; set; } = null!;

    }

}
