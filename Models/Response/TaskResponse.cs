using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class TaskResponse
    {
        public int TaskId {get;set;}

        //public int NewToDoItemId {get; set;}

        public string? Task {get; set;}

        public int Status {get;set;}

        public DateTime? DueDate {get;set;}

    }

}