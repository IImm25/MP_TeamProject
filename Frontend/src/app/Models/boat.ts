import { Employees } from "./employees";
import { Task } from "./task";
import { TaskTool } from "./task-tool";

export interface Boat {
    name: string,
    taskItems: Task[],
    persons?: Employees[],
    taskTools?: TaskTool[]
}
