using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Mappers;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class PersonService
    {
        private readonly IPersonRepository persons;
        private readonly IMapper mapper;

        public PersonService(IPersonRepository persons, IMapper mapper)
        {
            this.persons = persons;
            this.mapper = mapper;
        }

        public async Task<PersonDetailDto> CreatePerson(PersonCreateDto create)
        {
            Person person = new Person(create.Firstname, create.Lastname);

            foreach (var qId in create.QualificationIds)
            {
                person.Qualifications.Add(new PersonQualification
                {
                    PersonId = person.Id,
                    QualificationId = qId
                });
            }
            await persons.AddAsync(person);
            var saved = await persons.GetFullByIdAsync(person.Id);
            return mapper.Map<PersonDetailDto>(saved);
        }

        public async Task<List<PersonSummaryDto>> GetAll()
        {
            var p = await persons.GetAllAsync();
            return mapper.Map<List<PersonSummaryDto>>(p);
        }

        public async Task<PersonDetailDto?> GetPerson(int id)
        {
            Person? person = await persons.GetFullByIdAsync(id);
            return mapper.Map<PersonDetailDto?>(person);
        }

        public async Task<PersonDetailDto?> UpdatePerson(int id, PersonUpdateDto update)
        {
            var person = await persons.GetFullByIdAsync(id);

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
