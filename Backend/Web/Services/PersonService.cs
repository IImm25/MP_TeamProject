using AutoMapper;
using Backend.Data.DTO.Create;
using Backend.Data.DTO.Read;
using Backend.Data.DTO.Update;
using Backend.Data.Mappers;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class PersonService
    {
        private readonly IRepository<Person> persons;
        private readonly IMapper mapper;

        public PersonService(IRepository<Person> persons, IMapper mapper)
        {
            this.persons = persons;
            this.mapper = mapper;
        }

        public async Task<PersonDetailDto> CreatePerson(PersonCreateDto create)
        {
            Person person = new Person(create.Firstname, create.Lastname);
            return mapper.Map<PersonDetailDto>(await persons.AddAsync(person));
        }

        public async Task<List<PersonSummaryDto>> GetAll()
        {
            var p = await persons.GetAllAsync();
            return mapper.Map<List<PersonSummaryDto>>(p);
        }

        public async Task<PersonDetailDto?> GetPerson(int id)
        {
            Person? person = await persons.GetByIdAsync(id);
            return mapper.Map<PersonDetailDto?>(person);
        }

        public async Task<PersonDetailDto?> UpdatePerson(int id, PersonUpdateDto update)
        {
            var person = await persons.GetByIdAsync(id);

            if (person == null)
                return null;

            // update simple fields
            if (update.Firstname != null) person.Firstname = update.Firstname;
            if (update.Lastname != null) person.Lastname = update.Lastname;

            // clear old relations
            person.Qualifications.Clear();

            // add new relations
            foreach (var qId in update.QualificationIds)
            {
                person.Qualifications.Add(new PersonQualification
                {
                    PersonId = person.Id,
                    QualificationId = qId
                });
            }

            await persons.UpdateAsync(person);

            return mapper.Map<PersonDetailDto>(person);
        }

        public async Task<bool> DeletePerson(int id)
        {
            return await persons.DeleteAsync(id);
        } 
    }
}
