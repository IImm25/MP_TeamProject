import { Qualification } from "./qualification";

export interface Employees {
  id: number;
  firstname: string;
  lastname: string;
  qualifications: Qualification[];
}
