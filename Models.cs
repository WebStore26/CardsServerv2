using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardsServer
{
    public class Review
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Key]
        public int? Id { get; set; }
        public string Phone { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class Enterence
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Key]
        public int? Id { get; set; }  

        public int Counter { get; set; }
        public string Text { get; set; } = "";
        public DateTime LastEnetered { get; set; }
    }

    public class EnterDto
    {
        public string? Info { get; set; }
    }

    public class ReviewDto
    {
        public string Phone { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
