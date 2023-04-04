
namespace BethanysPieShop.Models
{
    public class Category
    {
        public int CategoryID {get;set;}
        public string CategoryName {get;set;} = string.Empty;
        public string? Description {get;set;}
        public List<Pie>? Pies {get;set;}
    }
}