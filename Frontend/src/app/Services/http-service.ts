import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Qualification } from '../Models/qualification';
import { Tool } from '../Models/tool';
import { Employee, EmployeeSummary, EmployeeCreateUpdate } from '../Models/employee';
import { Task, TaskSummary } from '../Models/task';
import { Boat, PlanRequest } from '../Models/boat';

@Injectable({
  providedIn: 'root',
})
export class HttpService {
  private http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // --- Qualifications ---
  getQualifications(): Observable<Qualification[]> {
    return this.http.get<Qualification[]>(`${this.apiUrl}/qualifications`);
  }

  getQualificationById(id: number): Observable<Qualification> {
    return this.http.get<Qualification>(`${this.apiUrl}/qualifications/${id}`);
  }

  createQualification(data: Partial<Qualification>): Observable<Qualification> {
    return this.http.post<Qualification>(`${this.apiUrl}/qualifications`, data);
  }

  updateQualification(id: number, data: Partial<Qualification>): Observable<Qualification> {
    return this.http.patch<Qualification>(`${this.apiUrl}/qualifications/${id}`, data);
  }

  deleteQualification(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/qualifications/${id}`);
  }

  // --- Tools ---
  getTools(): Observable<Tool[]> {
    return this.http.get<Tool[]>(`${this.apiUrl}/tools`);
  }

  getToolById(id: number): Observable<Tool> {
    return this.http.get<Tool>(`${this.apiUrl}/tools/${id}`);
  }

  createTool(data: Partial<Tool>): Observable<Tool> {
    return this.http.post<Tool>(`${this.apiUrl}/tools`, data);
  }

  updateTool(id: number, data: Partial<Tool>): Observable<Tool> {
    return this.http.patch<Tool>(`${this.apiUrl}/tools/${id}`, data);
  }

  deleteTool(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tools/${id}`);
  }

  // --- Persons (Employees) ---
  getEmployees(): Observable<EmployeeSummary[]> {
    return this.http.get<EmployeeSummary[]>(`${this.apiUrl}/persons`);
  }

  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/persons/${id}`);
  }

  createEmployee(data: EmployeeCreateUpdate): Observable<Employee> {
    return this.http.post<Employee>(`${this.apiUrl}/persons`, data);
  }

  updateEmployee(id: number, data: Partial<EmployeeCreateUpdate>): Observable<Employee> {
    return this.http.patch<Employee>(`${this.apiUrl}/persons/${id}`, data);
  }

  deleteEmployee(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/persons/${id}`);
  }

  // --- Tasks ---
  getTasks(): Observable<TaskSummary[]> {
    return this.http.get<TaskSummary[]>(`${this.apiUrl}/tasks`);
  }

  getTaskById(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.apiUrl}/tasks/${id}`);
  }

  createTask(data: Partial<Task>): Observable<Task> {
    return this.http.post<Task>(`${this.apiUrl}/tasks`, data);
  }

  updateTask(id: number, data: Partial<Task>): Observable<Task> {
    return this.http.patch<Task>(`${this.apiUrl}/tasks/${id}`, data);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tasks/${id}`);
  }

  // --- Scheduler ---
  postPlan(planData: PlanRequest): Observable<Boat[]> {
    return this.http.post<Boat[]>(`${this.apiUrl}/plan`, planData);
  }
}
