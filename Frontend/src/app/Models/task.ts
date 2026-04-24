import { Qualification } from "./qualification";
import { TaskQualification } from "./task-qualification";
import { TaskTool } from "./task-tool";

export interface Task {
  id: number;
  name: string;
  durationHours: number;
  taskQualifications: TaskQualification[];
  tasktools: TaskTool[];
}
