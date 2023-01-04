namespace BethanysPieShop.Models
{
    public interface IPieRepository
    {
        IEnumerable<Pie> AllPies {get;}
        IEnumerable<Pie> PiesOfTheWeek {get;}
        delegate Pie? GetPieByID(int pieId);
    }
}