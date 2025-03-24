using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.Context;
using WebApplication2.Enums;
using WebApplication2.Models;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using WebApplication2.Models.Response;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToDoListProcsController : ControllerBase
    {

        private readonly IConfiguration configuration;
        public ToDoListProcsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }



        [HttpPost("Create")]

        public IActionResult CreateNewToDo(NewToDoItem newToDoItem)
        {
            string? pgsqlConnection = configuration.GetConnectionString("DefaultConnection");
            // NpgsqlConnection npgsqlConnection=new();
            // NpgsqlCommand cmd = new();


            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                int newToDoItemId;
                using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call CreateNewToDoItem(:p_ownerName ,
                                                                                 :p_toDoListName ,
                                                                                 :p_isDeleted ,
                                                                                 :p_createdOn,
                                                                                 :n_id)", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("p_ownerName", NpgsqlDbType.Varchar, newToDoItem.OwnerName);
                    cmd.Parameters.AddWithValue("p_toDoListName", NpgsqlDbType.Varchar, newToDoItem.ToDoListName);
                    cmd.Parameters.AddWithValue("p_isDeleted", NpgsqlDbType.Boolean, newToDoItem.IsDeleted);
                    cmd.Parameters.AddWithValue("p_createdOn", NpgsqlDbType.Timestamp, newToDoItem.CreatedOn == default ? DateTime.Now : newToDoItem.CreatedOn.ToLocalTime());
                    var id = new NpgsqlParameter("n_id", NpgsqlDbType.Integer)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = DBNull.Value
                    };
                    cmd.Parameters.Add(id);

                    cmd.ExecuteNonQuery();

                    // int newToDoItemId = (int)id.Value; 
                    newToDoItemId = id.Value != DBNull.Value ? Convert.ToInt32(id.Value) : 0;


                }

                foreach (var t in newToDoItem.ListOfTasks)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call CreateTasks(:p_newToDoItemId,
                                                                          :p_Task,
                                                                          :p_status,
                                                                          :p_DueDate,
                                                                          :p_isTaskDeleted)", npgsqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_newToDoItemId", NpgsqlDbType.Integer, newToDoItemId);
                        cmd.Parameters.AddWithValue("p_Task", NpgsqlDbType.Varchar, t.Task ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("p_status", NpgsqlDbType.Integer, t.Status);
                        cmd.Parameters.AddWithValue("p_DueDate", NpgsqlDbType.Timestamp, t.DueDate.HasValue ? t.DueDate.Value.ToLocalTime() : DateTime.Now);
                        cmd.Parameters.AddWithValue("p_isTaskDeleted", NpgsqlDbType.Boolean, t.isTaskDeleted);
                        cmd.ExecuteNonQuery();
                    }
                }

                npgsqlConnection.Close();

            }
            return Ok("ToDoItem Added Succesfully");
        }

        [HttpPut("Update")]
        public IActionResult UpdateToDo([FromBody] NewToDoItem newToDoItem)
        {
            int newToDoItemId;
            string? pgsqlConnection = configuration.GetConnectionString("DefaultConnection");
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call UpdateToDoItem(:p_newToDoItemId,
                                                                                    :p_ownerName,
                                                                                    :p_toDoListName,
                                                                                    :p_isDeleted,
                                                                                    :p_createdOn,
                                                                                    :n_id )", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("p_newToDoItemId",NpgsqlDbType.Integer,newToDoItem.NewToDoItemId);
                    cmd.Parameters.AddWithValue("p_ownerName", NpgsqlDbType.Varchar, newToDoItem.OwnerName);
                    cmd.Parameters.AddWithValue("p_toDoListName", NpgsqlDbType.Varchar, newToDoItem.ToDoListName);
                    cmd.Parameters.AddWithValue("p_isDeleted", NpgsqlDbType.Boolean, newToDoItem.IsDeleted);
                    cmd.Parameters.AddWithValue("p_createdOn", NpgsqlDbType.Timestamp, newToDoItem.CreatedOn == default ? DateTime.Now : newToDoItem.CreatedOn .ToLocalTime());
                    var id = new NpgsqlParameter("n_id", NpgsqlDbType.Integer)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = newToDoItem.NewToDoItemId
                    };
                    cmd.Parameters.Add(id);
                    cmd.ExecuteNonQuery();
                    newToDoItemId = id.Value != DBNull.Value ? Convert.ToInt32(id.Value) : 0;
                }
                //Getting DB task ids

                IActionResult result = GetToDoTasks(newToDoItemId);
                string json = "";

                if (result is OkObjectResult okResult)
                {
                    json = JsonConvert.SerializeObject(okResult.Value);
                }

                List<int> dBTaskIds = new List<int>();

                if (!string.IsNullOrEmpty(json))
                {
                    ToDoResponse? toDoList = JsonConvert.DeserializeObject<ToDoResponse>(json);
                    if (toDoList?.ListOfTasks != null)
                    {
                        dBTaskIds = toDoList.ListOfTasks.Select( t => t.TaskId).ToList();
                    }
                }

                //Now get the given Task ids

                List<int> newTaskIds = newToDoItem.ListOfTasks.Select(t => t.TaskId).ToList();

                foreach (var t in newToDoItem.ListOfTasks)
                {
                    if (dBTaskIds.Contains(t.TaskId))
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call UpdateTasks(:p_taskId,
                                                                             :n_id,
                                                                             :p_Task,
                                                                             :p_status,
                                                                             :p_DueDate,
                                                                             :p_isTaskDeleted)", npgsqlConnection))

                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("p_taskId", NpgsqlDbType.Integer, t.TaskId);
                            cmd.Parameters.AddWithValue("n_id", NpgsqlDbType.Integer, newToDoItemId);
                            cmd.Parameters.AddWithValue("p_Task", NpgsqlDbType.Varchar, t.Task ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("p_status", NpgsqlDbType.Integer, t.Status);
                            cmd.Parameters.AddWithValue("p_DueDate", NpgsqlDbType.Timestamp, t.DueDate.HasValue ? t.DueDate.Value.ToLocalTime() : DateTime.Now);
                            cmd.Parameters.AddWithValue("p_isTaskDeleted", NpgsqlDbType.Boolean, t.isTaskDeleted);

                            cmd.ExecuteNonQuery();

                        }
                    }
                    else if (t.TaskId==0)
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call CreateTasks(:p_newToDoItemId,
                                                                                    :p_Task,
                                                                                    :p_status,
                                                                                    :p_DueDate,
                                                                                    :p_isTaskDeleted)", npgsqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("p_newToDoItemId", NpgsqlDbType.Integer, newToDoItemId);
                            cmd.Parameters.AddWithValue("p_Task", NpgsqlDbType.Varchar, t.Task ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("p_status", NpgsqlDbType.Integer, t.Status);
                            cmd.Parameters.AddWithValue("p_DueDate", NpgsqlDbType.Timestamp, t.DueDate.HasValue ? t.DueDate.Value.ToLocalTime() : DateTime.Now);
                            cmd.Parameters.AddWithValue("p_isTaskDeleted", NpgsqlDbType.Boolean, t.isTaskDeleted);

                            cmd.ExecuteNonQuery();

                        }
                    }
                }

                //Fetching the Ids That are present in DB But not in NewToDo

                List<int> toDeleteIds = dBTaskIds.Except(newTaskIds).ToList();

                foreach (var id in toDeleteIds)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call DeleteTask(@p_taskId)", npgsqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_taskId", NpgsqlDbType.Integer, id);

                        cmd.ExecuteNonQuery();

                    }
                }
                npgsqlConnection.Close();
            }
            return Ok("Task Updated Suessfully");
        }

        [HttpGet("Get")]

        public IActionResult GetToDoTasks(int id)
        {
            string? pgsqlConnection = configuration.GetConnectionString("DefaultConnection");
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(pgsqlConnection))
            {
                npgsqlConnection.Open();
                string? OwnerName, ToDoListName;
                //  var ownername=;

                using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call GetNewToDoItem(:p_newToDoItemId,
                                                                            :p_ownerName,
                                                                            :p_toDoListName)", npgsqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("p_newToDoItemId", NpgsqlDbType.Integer, id);
                    var ownername = new NpgsqlParameter("p_ownerName", NpgsqlDbType.Varchar)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = DBNull.Value
                    };
                    cmd.Parameters.Add(ownername);
                    var todolistname = new NpgsqlParameter("p_toDoListName", NpgsqlDbType.Varchar)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = DBNull.Value

                    };
                    cmd.Parameters.Add(todolistname);

                    cmd.ExecuteNonQuery();
                    OwnerName = ownername.Value.ToString();
                    ToDoListName = todolistname.Value.ToString();

                    if (OwnerName == "")
                    {
                        return BadRequest("You are trying to Fetch Deleted Item Or The Id is not found");
                    }
                }

                using (NpgsqlTransaction transaction = npgsqlConnection.BeginTransaction())
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"Call GetTasks(:p_newToDoItemId,:Ref_cursor)", npgsqlConnection, transaction))
                    {
                        cmd.Parameters.AddWithValue("p_newToDoItemId", NpgsqlDbType.Integer, id);
                        var taskcursor = new NpgsqlParameter("Ref_cursor", NpgsqlDbType.Refcursor)
                        {
                            Direction = ParameterDirection.InputOutput,
                            Value = "task_cursor"
                        };
                        cmd.Parameters.Add(taskcursor);

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "Fetch all in task_cursor";
                        cmd.CommandType = CommandType.Text;
                        NpgsqlDataReader dr = cmd.ExecuteReader();

                        var resultTasks = new List<TaskResponse>();

                        while (dr.Read())
                        {
                            resultTasks.Add(new TaskResponse
                            {
                                TaskId = dr.GetInt32(0),
                                Task = dr.GetString(1),
                                Status = dr.GetInt32(2),
                                DueDate = dr.GetDateTime(3)
                            }
                            );
                        }
                        dr.Close();

                        transaction.Commit();
                        npgsqlConnection.Close();
                        var ToDoItem = new ToDoResponse
                        {
                            OwnerName = OwnerName,
                            ToDoListName = ToDoListName,
                            ListOfTasks = resultTasks
                        };

                        return Ok(ToDoItem);
                        // return Ok(new { OwnerName, ToDoListName, resultTasks });

                    }
                }

            }

        }

        [HttpDelete("Delete")]

        public IActionResult DeleteToDoItem(int id)
        {
            string? pgsqlConnection= configuration.GetConnectionString("DefaultConnection");

            using (NpgsqlConnection npgsqlConnection =new NpgsqlConnection(pgsqlConnection))
            {

                npgsqlConnection.Open();
               
                using (NpgsqlCommand cmd=new NpgsqlCommand(@"Call DeleteToDoItem(:p_newToDoItemId)",npgsqlConnection))
                {
                    cmd.CommandType= CommandType.Text;

                    cmd.Parameters.AddWithValue("p_newToDoItemId",NpgsqlDbType.Integer,id);

                    cmd.ExecuteNonQuery();
 
                }
                npgsqlConnection.Close();
            }
            return Ok("ToDo Item marked as deleted ");
        }

    }
}