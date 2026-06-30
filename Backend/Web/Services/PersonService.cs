using AutoMapper;
using Backend.Data.DTO.Person;
using Backend.Data.Entitites;
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
                    QualificationId = qId
                });
            }

            int id = await persons.AddAsync(person);
            return mapper.Map<PersonDetailDto>(await persons.GetFullByIdAsync(id));
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

            if (update.QualificationIds!.Count != 0) {
                person.Qualifications.Clear();

                foreach (var qId in update.QualificationIds)
                {
                    person.Qualifications.Add(new PersonQualification
                    {
                        PersonId = person.Id,
                        QualificationId = qId
                    });
                }
            }
       
            await persons.UpdateAsync(person);
            return mapper.Map<PersonDetailDto>(await persons.GetFullByIdAsync(id));
        }

        public async Task<bool> DeletePerson(int id)
        {
            return await persons.DeleteAsync(id);
        } 
    }
}
