using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brat.Models
{
public partial class FileAsset
{
        public long Id { get; set; }

        public string File { get; set; } = null!;

        public string Kind { get; set; } = null!;

        public string Mime { get; set; } = null!;

        public ulong Size { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? UploaderId { get; set; }

        // Навигационное свойство на пользователя (если есть)
        public virtual User? Uploader { get; set; }

        // Навигационные свойства на MessageFiles
        public virtual ICollection<MessageAttachment> MessageFiles { get; set; } = new List<MessageAttachment>();
    }
}
