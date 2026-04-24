using Backend.Data.DTO;

namespace Backend.Web.Repositories;

public interface IPersonRepository
{
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<Person> CreateAsync(PersonCreateDto dto);
    Task<Person?> UpdateAsync(int id, PersonUpdateDto dto);
    Task<Person?> DeleteAsync(int id);
}
