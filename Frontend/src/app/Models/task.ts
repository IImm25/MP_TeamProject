import { Qualification } from "./qualification";
import { TaskTool } from "./task-tool";

export interface Task {
  id: number;
  name: string;
  durationHours: number;
  taskQualifications: Qualification[];
  tasktools: TaskTool[];
}
