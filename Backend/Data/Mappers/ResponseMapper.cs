using Backend.Data.DTO;
using Backend.GMPL;

namespace Backend.Data.Mappers
{
    public class ResponseMapper
    {
        public static List<PlanResponseDto> MapToResponse(
            GmplResults results,
            List<TaskItem> tasks,
            List<Person> people,
            List<Tool> tools,
            List<PersonQualification> personQualifications)
        {
            // Erstelle ein Dictionary für schnellen Zugriff: PersonId -> List<Qualification>
            var personQualificationsLookup = personQualifications
                .GroupBy(pq => pq.PersonId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(pq => pq.Qualification).ToList()
                );

            // Erstelle Lookups für Index-basierte Suche (weil Schlüssel wie "p_1", "ta_2" etc. sind)
            var taskLookup = tasks.ToDictionary(t => $"ta_{t.Id}");
            var personLookup = people.ToDictionary(p => $"p_{p.Id}");
            var toolLookup = tools.ToDictionary(t => $"to_{t.Id}");

            return results.TaskOnBoat.Keys.Select(boatId => new PlanResponseDto(
                BoatID: boatId,
                TaskItems: MapTaskItems(results.TaskOnBoat, boatId, taskLookup),
                People: MapPeople(results.PersonOnBoat, boatId, personLookup, personQualificationsLookup),
                Tools: MapTools(results.ToolOnBoat, boatId, toolLookup)
            )).ToList();
        }

        private static List<TaskItemSummaryDto> MapTaskItems(
            Dictionary<int, Dictionary<string, float>> taskOnBoat,
            int boatId,
            Dictionary<string, TaskItem> taskLookup)
        {
            if (!taskOnBoat.TryGetValue(boatId, out var tasks))
                return [];

            var result = new List<TaskItemSummaryDto>();

            foreach (var taskEntry in tasks)
            {
                string taskKey = taskEntry.Key;  // z.B. "ta_1"
                float duration = taskEntry.Value;

                var task = taskLookup.GetValueOrDefault(taskKey);

                result.Add(new TaskItemSummaryDto
                {
                    Id = task?.Id ?? 0,
                    Name = task?.Name ?? $"Task {taskKey}",
                    DurationHours = duration
                });
            }

            return result;
        }

        private static List<PersonDetailDto> MapPeople(
            Dictionary<int, Dictionary<string, int>> personOnBoat,
            int boatId,
            Dictionary<string, Person> personLookup,
            Dictionary<int, List<Qualification>> personQualificationsLookup)
        {
            if (!personOnBoat.TryGetValue(boatId, out var persons))
                return [];

            var result = new List<PersonDetailDto>();

            foreach (var personEntry in persons)
            {
                string personKey = personEntry.Key;  // z.B. "p_1"

                var person = personLookup.GetValueOrDefault(personKey);

                if (person != null)
                {
                    var qualifications = personQualificationsLookup.TryGetValue(person.Id, out var quals)
                        ? quals
                        : [];

                    result.Add(new PersonDetailDto
                    {
                        Id = person.Id,
                        Firstname = person.Firstname,
                        Lastname = person.Lastname,
                        Qualifications = qualifications
                            .Select(q => new QualificationResponseDto
                            {
                                Id = q.Id,
                                Name = q.Name,
                                Description = q.Description
                            })
                            .ToList()
                    });
                }
                else
                {
                    // Fallback: Person nicht gefunden
                    result.Add(new PersonDetailDto
                    {
                        Id = 0,
                        Firstname = personKey,
                        Lastname = "",
                        Qualifications = []
                    });
                }
            }

            return result;
        }

        private static List<TaskToolDto> MapTools(
            Dictionary<int, Dictionary<string, int>> toolOnBoat,
            int boatId,
            Dictionary<string, Tool> toolLookup)
        {
            if (!toolOnBoat.TryGetValue(boatId, out var toolEntries))
                return [];

            var result = new List<TaskToolDto>();

            foreach (var toolEntry in toolEntries)
            {
                string toolKey = toolEntry.Key;  // z.B. "to_1"
                int requiredAmount = toolEntry.Value;

                var tool = toolLookup.GetValueOrDefault(toolKey);

                result.Add(new TaskToolDto
                {
                    ToolId = tool?.Id ?? 0,
                    RequiredAmount = requiredAmount
                });
            }

            return result;
        }
    }
}