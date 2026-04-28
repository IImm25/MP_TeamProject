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
            var personQualificationsLookup = personQualifications
                .GroupBy(pq => pq.PersonId)
                .ToDictionary(g => g.Key, g => g.Select(pq => pq.Qualification).ToList());

            var taskLookup = tasks.ToDictionary(t => $"ta_{t.Id}");
            var personLookup = people.ToDictionary(p => $"p_{p.Id}");
            var toolLookup = tools.ToDictionary(t => $"to_{t.Id}");

            // 2. Ergebnisliste vorbereiten
            var result = new List<PlanResponseDto>();

            // Über ALLE Boote von 1 bis totalBoats iterieren
            for (int boatId = 1; boatId <= results.TaskOnBoat.Count; boatId++)
            {
                // Tasks für dieses Boot (nur Werte != 0)
                var taskItems = new List<TaskItemSummaryDto>();
                if (results.TaskOnBoat.TryGetValue(boatId, out var tasksOnThisBoat))
                {
                    foreach (var kvp in tasksOnThisBoat)
                    {
                        float duration = kvp.Value;
                        if (duration != 0)
                        {
                            string taskKey = kvp.Key;
                            if (taskLookup.TryGetValue(taskKey, out var task))
                            {
                                taskItems.Add(new TaskItemSummaryDto
                                {
                                    Id = task.Id,
                                    Name = task.Name,
                                    DurationHours = duration
                                });
                            }
                        }
                    }
                }

                // Personen für dieses Boot (nur Werte != 0)
                var peopleOnBoat = new List<PersonDetailDto>();
                if (results.PersonOnBoat.TryGetValue(boatId, out var personsOnThisBoat))
                {
                    foreach (var kvp in personsOnThisBoat)
                    {
                        int presence = kvp.Value;
                        if (presence != 0)
                        {
                            string personKey = kvp.Key;
                            if (personLookup.TryGetValue(personKey, out var person))
                            {
                                var qualifications = personQualificationsLookup.TryGetValue(person.Id, out var quals)
                                    ? quals
                                    : new List<Qualification>();

                                peopleOnBoat.Add(new PersonDetailDto
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
                        }
                    }
                }

                // Tools für dieses Boot (nur Werte != 0)
                var toolsOnBoat = new List<TaskToolDto>();
                if (results.ToolOnBoat.TryGetValue(boatId, out var toolsOnThisBoat))
                {
                    foreach (var kvp in toolsOnThisBoat)
                    {
                        int requiredAmount = kvp.Value;
                        if (requiredAmount != 0)
                        {
                            string toolKey = kvp.Key;
                            if (toolLookup.TryGetValue(toolKey, out var tool))
                            {
                                toolsOnBoat.Add(new TaskToolDto
                                {
                                    ToolId = tool.Id,
                                    RequiredAmount = requiredAmount
                                });
                            }
                        }
                    }
                }

                // DTO hinzufügen (auch wenn alle Listen leer sind)
                result.Add(new PlanResponseDto(
                    BoatID: boatId,
                    TaskItems: taskItems,
                    People: peopleOnBoat,
                    Tools: toolsOnBoat
                ));
            }

            return result;
        }

        private static List<TaskItemSummaryDto> MapTaskItems(
            Dictionary<int, Dictionary<string, int>> taskOnBoat,  // Achtung: float, nicht int!
            int boatId,
            Dictionary<string, TaskItem> taskLookup,List<TaskItem> taskItems)
        {
            if (!taskOnBoat.TryGetValue(boatId, out var tasks))
                return [];

            var result = new List<TaskItemSummaryDto>();
            foreach (var taskEntry in tasks)
            {
                string taskKey = taskEntry.Key;   // "ta_1"
                float duration = taskEntry.Value; // kommt direkt aus dem Ergebnis

                var task = taskLookup.GetValueOrDefault(taskKey);
                if (task == null) continue; // oder Fallback, je nach Anforderung

                result.Add(new TaskItemSummaryDto
                {
                    Id = task.Id,
                    Name = task.Name,
                    DurationHours = taskItems.Where(x => x.Id == task.Id).First().DurationHours
                });
            }
            return result;
        }

        // MapPeople und MapTools bleiben wie gehabt (funktionieren)

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