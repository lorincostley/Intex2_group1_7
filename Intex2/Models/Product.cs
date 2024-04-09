using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Intex2.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
    }
}
