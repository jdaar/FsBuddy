using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionInterface 
{
    public enum t_WatcherAction: int
    {
        MOVE,
        RENAME,
        DELETE
    }

    [Table(name: "watchers")]
    public class Watcher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(name: "name")]
        public string? Name { get; set; }

        [Required]
        [StringLength(100)]
        [Column(name: "search_pattern")]
        public string? SearchPattern { get; set; }

        [Required]
        [Column(name: "created_at")]
        public DateTime CreatedAt { get; set; }


        [Required]
        [StringLength(256)]
        [Column(name: "input_path")]
        public string? InputPath { get; set; }

        [Required]
        [StringLength(256)]
        [Column(name: "output_path")]
        public string? OutputPath { get; set; }

        [Required]
        [Column(name: "action")]
        public  t_WatcherAction Action { get; set; }

    }
}
