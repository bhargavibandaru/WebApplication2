using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using NuGet.Versioning;
using WebApplication2.Context;
using WebApplication2.Models;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasicToDoController : ControllerBase
{
    private readonly ApplicationDbContext dbContext;
    public BasicToDoController(ApplicationDbContext dbContext)
    {

        this.dbContext = dbContext;
    }
    public static List<ToDoItem> Tasks = new List<ToDoItem>
    {
        new ToDoItem { Id = 1, OwnerName = "Alice", ToDoListName = "Work Tasks", Task = "Finish project report", Status = 1, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-2) },
        new ToDoItem { Id = 2, OwnerName = "Bob", ToDoListName = "Home Chores", Task = "Clean the garage", Status = 2, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-5) },
        new ToDoItem { Id = 3, OwnerName = "Charlie", ToDoListName = "Workout Plan", Task = "Run 5K", Status = 1, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-1) },
        new ToDoItem { Id = 4, OwnerName = "David", ToDoListName = "Shopping List", Task = "Buy groceries", Status = 2, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-3) },
        new ToDoItem { Id = 5, OwnerName = "Emma", ToDoListName = "Learning Goals", Task = "Read 3 chapters of a book", Status = 1, IsDeleted = true, CreatedOn = DateTime.Now.AddDays(-4) },
        new ToDoItem { Id = 6, OwnerName = "Frank", ToDoListName = "Home Chores", Task = "Mow the lawn", Status = 2, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-6) },
        new ToDoItem { Id = 7, OwnerName = "Grace", ToDoListName = "Daily Routine", Task = "Practice meditation", Status = 1, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-7) },
        new ToDoItem { Id = 8, OwnerName = "Hannah", ToDoListName = "Project Plan", Task = "Code review", Status = 1, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-8) },
        new ToDoItem { Id = 9, OwnerName = "Ian", ToDoListName = "Weekend Tasks", Task = "Fix the car", Status = 2, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-9) },
        new ToDoItem { Id = 10, OwnerName = "Julia", ToDoListName = "Social Events", Task = "Plan birthday party", Status = 1, IsDeleted = false, CreatedOn = DateTime.Now.AddDays(-10) }
    };

    [HttpPost("Create")]
    public IActionResult CreateToDoList(ToDoItem Task)
    {
        //Tasks.Id.sort();
        int lastId = 0;
        foreach (var i in Tasks)
        {
            if (i.Id > lastId)
            {
                lastId = i.Id;
            }
        }
        Task.Id = lastId + 1;
        Tasks.Add(Task);
        return Ok(Task);
    }

    [HttpPut("Update")]
    public IActionResult UpdateToDoList(int id, ToDoItem NewTask)
    {
        var existingTask = Tasks.FirstOrDefault(t => t.Id == id); // Find the task

        if (existingTask == null)
        {
            return NotFound("The ID is not present to perform the update."); // Return 404
        }
        return Ok(existingTask.Task = NewTask.Task);

    }

    [HttpDelete("Delete")]
    public IActionResult DeleteToDoList(int id)
    {
        try
        {
            ToDoItem task = Tasks.First(t => t.Id == id);
            task.IsDeleted = true;
        }
        catch (Exception ex)
        {
            return Ok(ex);
        }
        return Ok("Task deleted successfully ");
    }

    [HttpGet("Get")]
    public ActionResult<ToDoItem> GetToDoList(int id)
    {
        //var task =Tasks.Where(t => t.Id == id).Select(t => t.Task);
        var task = Tasks.Where(t => t.Id == id);
        if (task.Any())
        {
            return Ok(task);
        }
        return NotFound("No Task Found");
    }

    [HttpGet("GetFromDB")]
    public IActionResult GetTask(int id)
    {
        // if (id == null)
        // {
        //     return BadRequest("ID should be mentioned.");
        // }
        // var task = dbContext.ToDoItems.FirstOrDefault(t=>t.Id ==id);
        var task = dbContext.ToDoItems.Where(t =>t.Id==id).Select(t => new 
        { 
            t.OwnerName, 
            t.ToDoListName, 
            t.Task, 
            t.Status 
        }).FirstOrDefault();
        if (task == null)
        {
            return NotFound("Task Not Found");
        }
        
        return Ok(task);
    }

    [HttpPost("CreateNewTaskInDB")]
    public IActionResult CreateTask(ToDoItem NewTask)
    {
        dbContext.Add(NewTask);
        dbContext.SaveChanges();

        return Ok("New task added Successfully");
    }


    [HttpPut("UpdateDBTask")]
    public IActionResult UpdateTask([FromBody] ToDoItem NewTask)
    {
        var existingTask = dbContext.ToDoItems.FirstOrDefault(t => t.Id == NewTask.Id);
        if (existingTask == null)
        {
            return BadRequest("You are trying to update the task which is Not existed based on given Id");
        }
       
        existingTask.OwnerName = NewTask.OwnerName;
        existingTask.ToDoListName = NewTask.ToDoListName;
        existingTask.Task = NewTask.Task;
        existingTask.Status = NewTask.Status;
        existingTask.IsDeleted = NewTask.IsDeleted;
        existingTask.CreatedOn = NewTask.CreatedOn == default 
        ? DateTime.UtcNow  // Set current time if missing
        : DateTime.SpecifyKind(NewTask.CreatedOn, DateTimeKind.Utc); 

        dbContext.Update(existingTask); 
        dbContext.SaveChanges();
        return Ok("Task Updated Successfully");
    }


    [HttpDelete("DeleteTask")]
    public IActionResult DeleteTask(int id)
    {
        var existingTaskId = dbContext.ToDoItems.Find(id);
        if (existingTaskId == null)
        {
            return NotFound("Task Not Found! Can't perform Delete Operation");
        }
        existingTaskId.IsDeleted = true;
        dbContext.SaveChanges();
        return Ok("Task marked as deleted");
    }

}