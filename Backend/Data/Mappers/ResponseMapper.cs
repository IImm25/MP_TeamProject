using AutoMapper;
using Backend.Data.DTO;
using Backend.GMPL;

namespace Backend.Data.Mappers
{
    public class ResponseMapper : Profile
    {
        public static List<PlanResponseDto> MapToResponse(GmplResults results)
        {
            return results.TaskOnBoat.Keys.Select(boatId => new PlanResponseDto(
                BoatID: boatId,
                TaskItems: MapTaskItems(results.TaskOnBoat, boatId),
                People: MapPeople(results.PersonOnBoat, boatId),
                Tools: MapTools(results.ToolOnBoat, boatId)
            )).ToList();
        }

        private static List<TaskItemSummaryDto> MapTaskItems(
    Dictionary<int, Dictionary<string, int>> taskOnBoat, int boatId)
        {
            if (!taskOnBoat.TryGetValue(boatId, out var tasks)) return [];

            return tasks
                .Select(t => new TaskItemSummaryDto
                {
                    Name = t.Key,
                    DurationHours = t.Value
                })
                .ToList();
        }

        private static List<PersonDetailDto> MapPeople(
    Dictionary<int, Dictionary<string, int>> personOnBoat, int boatId)
        {
            if (!personOnBoat.TryGetValue(boatId, out var persons)) return [];

            return persons
                .Select(p => new PersonDetailDto
                {
                    Firstname = p.Key,  // oder aufsplitten falls "Vorname Nachname"
                })
                .ToList();
        }

        private static List<TaskToolDto> MapTools(
            Dictionary<int, Dictionary<string, int>> toolOnBoat, int boatId)
        {
            if (!toolOnBoat.TryGetValue(boatId, out var tools)) return [];

            return tools
                .Select(t => new TaskToolDto
                {
                    RequiredAmount = t.Value
                })
                .ToList();
        }
    }
}
