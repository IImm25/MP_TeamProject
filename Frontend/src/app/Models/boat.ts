import { EmployeeSummary } from "./employee";
import { TaskSummary, TaskTool } from "./task";

export interface PlanRequest {
  maxTime: number;
  boatNumber: number;
  date: string; //DateOnly
  speed: number;
}

export interface PlanResponse {
  date: string; //DateOnly
  createdAt: string; //DateTime
  boats: Boat[];
}

export interface Boat {
  persons: EmployeeSummary[];
  tools: TaskTool[];
  taskSchedules: TaskSchedule[];
  boatSchedules: BoatSchedule[];
}

export interface BoatSchedule {
  departure: string; //TimeOnly
  arrival: string; //TimeOnly
}

export interface TaskSchedule {
  task: TaskSummary;
  startTime: string;
}

export interface RequirementDiff {
  id: number;
  required: number;
  available: number;
}
