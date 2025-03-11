using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Context;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewToDoListController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public NewToDoListController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("Create")]

        public IActionResult CreateNewtask([FromBody] NewToDoItem NewTask)
        {
            dbContext.Add(NewTask);
            dbContext.SaveChanges();

            return Ok("New Task added Successfully");
        }

       [HttpPut("Update")]

        public IActionResult UpdateTask([FromBody] NewToDoItem NewTask)
        {
            var existingTask = dbContext.NewToDoItem.Include( t => t.ListOfTasks).
                                FirstOrDefault(t => t.NewToDoItemId == NewTask.NewToDoItemId);

            if (existingTask != null &&  CheckTasksId(NewTask.NewToDoItemId))
            {
                existingTask.OwnerName = NewTask.OwnerName;
                existingTask.ToDoListName = NewTask.ToDoListName;
                existingTask.IsDeleted = NewTask.IsDeleted;
                existingTask.CreatedOn =NewTask.CreatedOn;
      
                foreach( var t in NewTask.ListOfTasks)
                {
                var existing_Task =existingTask.ListOfTasks.FirstOrDefault(t => t.TaskId==t.TaskId);
                if(existing_Task !=null)
                {
                    existing_Task.Task=t.Task;
                    existing_Task.Status=t.Status;
                    existing_Task.DueDate=t.DueDate;
                }  
                else
                {
                    return NotFound("Id Not Found in ListOfTasks");
                }     
                }
                dbContext.Update(existingTask);
                dbContext.SaveChanges();
            }
            else
            {
                return NotFound("Task Not found");
            }
            
            return Ok(NewTask);       
        }

        [NonAction]
         //private Tasks UpdateTasks(Tasks NewTask) --By making private method is another way instead of NonAction
       public bool CheckTasksId(int newToDoItemId)
        {
           var  existingId = dbContext.Tasks.FirstOrDefault(t => t.NewToDoItemId == newToDoItemId);
            if (existingId == null)
            {
                return false;
            }
            return true;
        } 

        [HttpDelete("Delete")]
        public IActionResult DeleteTask(int id)
        {
            var existingTaskId = dbContext.NewToDoItem.Find(id);

            if (existingTaskId == null)
            {
                return NotFound();
            }
            existingTaskId.IsDeleted = true;
            dbContext.SaveChanges();
            return Ok("Task marked as Deleted");
        }

        [HttpGet("Get")]

        public IActionResult GetTask(int id)
        {
            var existingTask = (from NewToDoItem in dbContext.NewToDoItem
                                join Tasks in dbContext.Tasks on
                                NewToDoItem.NewToDoItemId equals Tasks.NewToDoItemId
                                where id == NewToDoItem.NewToDoItemId
                                select new
                                {
                                    OwnerName = NewToDoItem.OwnerName,
                                    ToDoListName = NewToDoItem.ToDoListName,
                                    Task = Tasks.Task,
                                    Status = Tasks.Status
                                });
            if (existingTask.Any())
            {
                return Ok(existingTask);
            }
            return NotFound("No Tasks Found ");
        }

         [HttpGet("GetAll")]
        public IActionResult GetAll( )
        {
            var alltasks=dbContext.NewToDoItem.Include( t => t.ListOfTasks).ToList();
            return Ok(alltasks);
        }
    }

}