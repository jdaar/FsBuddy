﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionInterface 
{
    public enum WatcherAction: int
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
        [Column(name: "executed_at")]
        public DateTime ExecutedAt { get; set; }

        [Required]
        [Column(name: "modified_files")]
        public int ModifiedFiles { get; set; }


        [Required]
        [StringLength(256)]
        [Column(name: "input_path")]
        public string? InputPath { get; set; }

        [Required]
        [StringLength(256)]
        [Column(name: "output_path")]
        public string? OutputPath { get; set; }

        [Column(name: "is_enabled")]
        public bool? IsEnabled { get; set; } = true;

        [Required]
        [Column(name: "action")]
        public  WatcherAction Action { get; set; }

    }
}
