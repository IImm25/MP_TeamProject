import { Employee, EmployeeSummary } from "./employee";
import { Task, TaskSummary, TaskTool } from "./task";

export interface Boat {
  taskItems: TaskSummary[];
  persons: EmployeeSummary[];
  tools: TaskTool[];
}

export interface PlanResponse {
  totalTime: number;
  boats: Boat[];
}

export interface PlanRequest {
  maxTime: number;
  boatNumber: number;
  taskItemIds: number[];
  personIds: number[];
  toolIds: number[];
}
