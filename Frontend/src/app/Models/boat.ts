import { Employee } from "./employee";
import { Task, TaskTool } from "./task";
import { Tool } from "./tool";

export interface Boat {
  id: number;
  taskItems: Task[];
  people: Employee[];
  tools: TaskTool[];
}

export interface PlanRequest {
  maxTime: number;
  boatNumber: number;
  taskItemIds: number[];
  personIds: number[];
  toolIds: number[];
}
