
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    public class ToDoItem
    {
        public Guid guid {get;set;} = Guid.NewGuid();

        [Key]
        [Column("id")] 
        public int Id { get; set; }

        [Required]
        [Column("ownername")]
        public required string OwnerName  {get;set;}

        [Required(ErrorMessage ="ToDoListName is Required.")]
        [Column("todolistname")]
        public required string ToDoListName {get;set;}

        [Required]
        [Column("task")]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$",ErrorMessage ="Task can be alpanumeric including spaces.")]
        public required string Task { get; set; }

        [Required]
        [Column("status")]
        [Range(1,2,ErrorMessage ="Status must be either 1 Or 2.")]
        public int Status { get; set; }

        [Required(ErrorMessage ="Boolean type is Required.")]
        [Column("isdeleted")]
        public bool IsDeleted {get;set;}

        [Required(ErrorMessage = "CreatedOn is Required.")]
        [Column("createdon")]
        public DateTime CreatedOn {get;set;}
    }
}
