using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models.Response
{
    public class ToDoResponse
    {
        public string? OwnerName {get; set;}

        public string?  ToDoListName {get; set;}
        public List<TaskResponse> ListOfTasks  {get; set;}     
    
    }
}