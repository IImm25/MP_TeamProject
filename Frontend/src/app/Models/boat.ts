import { Employee } from "./employee";
import { Task, TaskTool } from "./task";

export interface Boat {
  id: number;
  taskItems: Task[];
  persons: Employee[];
  taskTools: TaskTool[];
}

export interface PlanRequest {
  maxTime: number;
  taskItemIds: number[];
  personIds: number[];
}
