using System.ComponentModel.DataAnnotations;

namespace Intex2.Models
{
    public class Recommendation
    {
        [Key]
        public int recommendation_ID {  get; set; }
        public int primary_product_ID { get; set; }
        public int recommendation_1 { get; set; }
        public int recommendation_2 { get; set; }
        public int recommendation_3 { get; set; }
        public int recommendation_4 { get; set; }
        public int recommendation_5 { get; set; }

    }
}
