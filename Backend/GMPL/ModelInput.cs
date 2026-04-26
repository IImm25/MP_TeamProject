using System.Collections.Generic;

namespace Backend.GMPL;

public record ModelInput(
    int Boats,
    float MaxTime,
    IEnumerable<Person> Persons,
    IEnumerable<TaskItem> Tasks,
    IEnumerable<Tool> Tools
);