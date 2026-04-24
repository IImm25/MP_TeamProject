namespace Backend.Web.Repositories;

public interface IPersonQualificationRepository
{
    Task<List<Qualification>> GetQualificationsByPersonAsync(int personId);
    Task<PersonQualification?> AddAsync(int personId, int qualificationId);
    Task<PersonQualification?> RemoveAsync(int personId, int qualificationId);
}
