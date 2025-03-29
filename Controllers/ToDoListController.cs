using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
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
    public class ToDoListController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public ToDoListController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("Create")]

        public IActionResult CreateNewtask([FromBody] NewToDoItem NewTask)
        {
            var newTask = new NewToDoItem
            {
                NewToDoItemId = NewTask.NewToDoItemId,
                OwnerName = NewTask.OwnerName,
                ToDoListName = NewTask.ToDoListName,
                IsDeleted = NewTask.IsDeleted,
                CreatedOn = NewTask.CreatedOn,
                ListOfTasks = new List<Tasks>()

            };
            foreach (var t in NewTask.ListOfTasks)
            {
                var subTask = new Tasks
                {
                    TaskId = t.TaskId,
                    NewToDoItemId = NewTask.NewToDoItemId,
                    Task = t.Task,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    isTaskDeleted = t.isTaskDeleted
                };
                newTask.ListOfTasks.Add(subTask);
            }


            dbContext.Add(newTask);
            dbContext.SaveChanges();

            return Ok("New ToDo Item added Successfully");
        }

        [HttpPut("Update")]

        public IActionResult UpdateTask([FromBody] NewToDoItem NewTask)
        {
            IEnumerable<int> newTaskIds = NewTask.ListOfTasks.Select(t => t.TaskId);
            IEnumerable<int> dbTaskIds = Enumerable.Empty<int>();

            IActionResult result = GetById(NewTask.NewToDoItemId);
            if (result is JsonResult jsonResult)
            {
                var jsonString = JsonSerializer.Serialize(jsonResult.Value); //convert obj to json string
                if (!string.IsNullOrEmpty(jsonString))
                {
                    var newTask = JsonSerializer.Deserialize<NewToDoItem>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    dbTaskIds = newTask?.ListOfTasks?.Select(t => t.TaskId) ?? Enumerable.Empty<int>();
                }

            }

            var existingTask = dbContext.NewToDoItem.Include(t => t.ListOfTasks).FirstOrDefault(t => t.NewToDoItemId == NewTask.NewToDoItemId);

            if (existingTask != null && CheckTasksId(NewTask.NewToDoItemId))
            {
                existingTask.OwnerName = NewTask.OwnerName;
                existingTask.ToDoListName = NewTask.ToDoListName;
                existingTask.IsDeleted = NewTask.IsDeleted;
                existingTask.CreatedOn = NewTask.CreatedOn;

                foreach (var subTask in NewTask.ListOfTasks)
                {
                    var existing_Task = existingTask.ListOfTasks.FirstOrDefault(t => t.TaskId == subTask.TaskId);
                    if (existing_Task != null)
                    {
                        existing_Task.Task = subTask.Task;
                        existing_Task.Status = subTask.Status;
                        existing_Task.DueDate = subTask.DueDate;
                        existing_Task.isTaskDeleted = subTask.isTaskDeleted;
                        dbContext.Update(existing_Task);

                    }
                    else if (existing_Task == null)
                    {

                        var newsubtask = new Tasks
                        {
                            TaskId = subTask.TaskId,
                            NewToDoItemId = NewTask.NewToDoItemId,
                            Task = subTask.Task,
                            Status = subTask.Status,
                            DueDate = subTask.DueDate,
                            isTaskDeleted = subTask.isTaskDeleted
                        };
                        dbContext.Add(newsubtask);

                    }
                }
                List<int> toDeleteTaskIds = dbTaskIds.Except(newTaskIds).ToList();

                foreach (var d in toDeleteTaskIds)
                {
                    var id = NewTask.ListOfTasks.FirstOrDefault(t => t.TaskId == d);
                    if (id != null)
                    {
                        id.isTaskDeleted = true;
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
            var existingId = dbContext.Tasks.FirstOrDefault(t => t.NewToDoItemId == newToDoItemId);
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

        [HttpGet("GetById")]
        public IActionResult GetById(int id)
        {
            var exsistingtask = dbContext.NewToDoItem.Where(t => id == t.NewToDoItemId && t.IsDeleted == false).
                        Select(t => new
                        {
                            t.OwnerName,
                            t.ToDoListName,
                            ListOfTasks = t.ListOfTasks.Select(t => new
                            {
                                t.TaskId,
                                t.Task,
                                t.Status
                            })
                        }
                        );
            if (!exsistingtask.Any())
            {
                return NotFound("Task not found");
            }

            return Ok(exsistingtask);
        }


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var alltasks = dbContext.NewToDoItem.Where(t => t.IsDeleted == false).
            Select(t => new { t.OwnerName, t.ToDoListName, ListOfTasks = t.ListOfTasks.Select(t => new { t.Task, t.Status }) });
            return Ok(alltasks);
        }
    }

}