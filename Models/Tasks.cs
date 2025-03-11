using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class Tasks
    {
        [Key]
        [Column("taskid")]
        public int TaskId {get; set;}

        [Column("newtodoitemid")]
        public int NewToDoItemId {get; set;}

        [Column("task")]
        public string? Task {get; set;}

        [Column("status")]
        public int Status {get;set;}

        [Column("duedate")]
        public DateTime? DueDate {get;set;}



    }
}