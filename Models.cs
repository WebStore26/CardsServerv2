namespace CardsServer
{
    public class Review
    {
        public int Id { get; set; }
        public string Phone { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class Enterence
    {
        public int Id { get; set; }  

        public int Counter { get; set; }
        public string Text { get; set; } = "";
        public DateTime LastEnetered { get; set; }
    }

    public class ReviewDto
    {
        public string Phone { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
