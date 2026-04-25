namespace Backend.Data.Repositories
{
    public interface IPersonRepository : IRepository<Person>
    {
        Task<Person?> GetFullByIdAsync(int id);
        Task<List<Person>> GetAllFullAsync();
    }
}
