using System.ComponentModel.DataAnnotations;

namespace Intex2.Models
{
    public class Top_Rating
    {
        [Key]
        public int top_rating_ID {  get; set; }
        public int product_ID { get; set; }
        public float rating { get; set; }
        public required string name { get; set; }
    }
}
