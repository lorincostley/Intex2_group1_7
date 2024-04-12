using System.ComponentModel.DataAnnotations;

namespace Intex2.Models
{
    public class Customer_Product
    {
        [Key]
        public int top_product_ID { get; set; }
        [Required]
        public string top_product_name { get; set; }
        public int recommendation_1 { get; set; }
        public int recommendation_2 { get; set; }
        public int recommendation_3 { get; set; }
    }
}
