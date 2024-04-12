using Intex2.Models.ViewModels;
using Intex2.Models;

namespace Intex2.Models.ViewModels
{
  public class ProductRecommendViewModel
{
    public IQueryable<Product> Products { get; set; }
    public IQueryable<Top_Rating> top_Ratings { get; set; }
    public Customer customer { get; set; }
    public IQueryable<User_Recommendation> user_recommendations { get; set; }

}
}
