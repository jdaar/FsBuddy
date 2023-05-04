using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionInterface 
{
    public enum SettingType: int
    {
        THREAD_NUMBER,
    }
    
    [Table(name: "system_settings")]
    public class ServiceSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "id")]
        public int Id { get; set; }

        [Required]
        [Column(name: "type")]
        public SettingType Type { get; set; }

        [Required]
        [Column(name: "value")]
        public int? Value { get; set; }

        [Required]
        [Column(name: "created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
