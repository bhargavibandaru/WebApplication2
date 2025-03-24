using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Protocol.Plugins;

namespace WebApplication2.Models
{
    public class NewToDoItem
    {
        [Key]
        [Column("newtodoitemid")]
        public int NewToDoItemId {get; set;}

        [Column("ownername")]
        public  string OwnerName {get; set;}

        [Column("todolistname")]
        public  string ToDoListName {get; set;}

        [Column("isdeleted")]
        public bool IsDeleted {get; set;}

        [Column("createdon")]
        public DateTime CreatedOn {get;set;}

        [Column("tasks")]
        public List<Tasks> ListOfTasks  {get; set;}

         
        
    }

    
}