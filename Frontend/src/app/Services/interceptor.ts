import { HttpInterceptorFn, HttpResponse, HttpRequest } from '@angular/common/http';
import { delay, of } from 'rxjs';
import { Task } from '../Models/task';
import { Employees } from '../Models/employees';
import { Tool } from '../Models/tool';
import { Qualification } from '../Models/qualification';
import { Boat } from '../Models/boat';

let tasks: Task[] = [
  {
    id: 1,
    name: 'Ölwechsel im Getriebe',
    durationHours: 6.0,
    taskQualifications: [
      { id: 2, name: 'Überleben auf See' },
      { id: 3, name: 'Elektrotechnik' }
    ],
    tasktools: [
      { id: 2, name: 'Kletter-Ausrüstung (PSA)', count: 2 },
      { id: 1, name: 'Schwerlast-Schlagschrauber', count: 1 }
    ],
  },
  {
    id: 2,
    name: 'Reparatur Vogelschlag (Flügel)',
    durationHours: 8.0,
    taskQualifications: [
      { id: 1, name: 'Höhenrettung' },
      { id: 4, name: 'Rotorblatt-Check' }
    ],
    tasktools: [
      { id: 2, name: 'Kletter-Ausrüstung (PSA)', count: 3 },
      { id: 3, name: 'Harz-Reparaturset', count: 1 }
    ],
  }
];

let employees: Employees[] = [
  {
    id: 1,
    firstname: 'Hannes',
    lastname: 'Fiedler',
    qualifications: [
      { id: 2, name: 'Überleben auf See' },
      { id: 3, name: 'Elektrotechnik' }
    ],
  },
  {
    id: 2,
    firstname: 'Sören',
    lastname: 'Smit',
    qualifications: [
      { id: 1, name: 'Höhenrettung' },
      { id: 2, name: 'Überleben auf See' },
      { id: 4, name: 'Rotorblatt-Check' }
    ],
  }
];

let tools: Tool[] = [
  { id: 1, name: 'Schwerlast-Schlagschrauber', stock: 5 },
  { id: 2, name: 'Kletter-Ausrüstung (PSA)', stock: 20 },
  { id: 3, name: 'Harz-Reparaturset', stock: 10 },
  { id: 4, name: 'Spannungsprüfer', stock: 15 }
];

let qualifications: Qualification[] = [
  { id: 1, name: 'Höhenrettung' },
  { id: 2, name: 'Überleben auf See' },
  { id: 3, name: 'Elektrotechnik' },
  { id: 4, name: 'Rotorblatt-Check' },
];

let schedule: Boat[] = [
  {
    name: 'Service-Kutter Nordsee 1',
    taskItems: [
      {
        id: 1,
        name: 'Wartung Turbine 04',
        durationHours: 6.0,
        taskQualifications: [{ id: 3, name: 'Elektrotechnik' }],
        tasktools: [{ id: 1, name: 'Schwerlast-Schlagschrauber', count: 1 }],
      },
    ],
    persons: [
      {
        id: 1,
        firstname: 'Hannes',
        lastname: 'Fiedler',
        qualifications: [{ id: 3, name: 'Elektrotechnik' }],
      },
    ],
    taskTools: [{ id: 2, name: 'Kletter-Ausrüstung (PSA)', count: 2 }],
  }
];

export const httpMockInterceptor: HttpInterceptorFn = (req, next) => {
  const { url, method, body } = req;

  //Tasks
  // GET /tasks
  if (url.endsWith('/tasks') && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: tasks })).pipe(delay(100));
  }

  // POST /tasks
  if (url.endsWith('/tasks') && method === 'POST') {
    const newTask = { ...(body as any), id: Math.floor(Math.random() * 1000) };
    tasks.push(newTask);
    return of(new HttpResponse({ status: 201, body: newTask })).pipe(delay(100));
  }

  // PATCH /tasks/{id}
  if (url.match(/\/tasks\/\d+$/) && method === 'PATCH') {
    const id = parseInt(url.split('/').pop() || '0');
    tasks = tasks.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
    var task = tasks.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: task })).pipe(delay(100));
  }

  // DELETE /tasks/{id}
  if (url.match(/\/tasks\/\d+$/) && method === 'DELETE') {
    const id = parseInt(url.split('/').pop() || '0');
    tasks = tasks.filter((t) => t.id !== id);
    return of(new HttpResponse({ status: 204 })).pipe(delay(100));
  }

  // GET /tasks/{id}
  if (url.match(/\/tasks\/\d+$/) && method === 'GET') {
    const id = parseInt(url.split('/').pop() || '0');
    const task = tasks.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: task })).pipe(delay(100));
  }

  //Employees
  // GET /employees
  if (url.endsWith('/employees') && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: employees })).pipe(delay(100));
  }

  // POST /employees
  if (url.endsWith('/employees') && method === 'POST') {
    const newEmployee = { ...(body as any), id: Math.floor(Math.random() * 1000) };
    employees.push(newEmployee);
    return of(new HttpResponse({ status: 201, body: newEmployee })).pipe(delay(100));
  }

  // PATCH /employees/{id}
  if (url.match(/\/employees\/\d+$/) && method === 'PATCH') {
    const id = parseInt(url.split('/').pop() || '0');
    employees = employees.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
    var employee = employees.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: employee })).pipe(delay(100));
  }

  // DELETE /employees/{id}
  if (url.match(/\/employees\/\d+$/) && method === 'DELETE') {
    const id = parseInt(url.split('/').pop() || '0');
    employees = employees.filter((t) => t.id !== id);
    return of(new HttpResponse({ status: 204 })).pipe(delay(100));
  }

  // GET /employees/{id}
  if (url.match(/\/employees\/\d+$/) && method === 'GET') {
    const id = parseInt(url.split('/').pop() || '0');
    const employee = employees.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: employee })).pipe(delay(100));
  }

  //Qualifications
  // GET /qualifications
  if (url.endsWith('/qualifications') && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: qualifications })).pipe(delay(100));
  }

  // POST /qualifications
  if (url.endsWith('/qualifications') && method === 'POST') {
    const newQualification = { ...(body as any), id: Math.floor(Math.random() * 1000) };
    qualifications.push(newQualification);
    return of(new HttpResponse({ status: 201, body: newQualification })).pipe(delay(100));
  }

  // PATCH /qualifications/{id}
  if (url.match(/\/qualifications\/\d+$/) && method === 'PATCH') {
    const id = parseInt(url.split('/').pop() || '0');
    qualifications = qualifications.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
    var qualification = qualifications.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: qualification })).pipe(delay(100));
  }

  // DELETE /qualifications/{id}
  if (url.match(/\/qualifications\/\d+$/) && method === 'DELETE') {
    const id = parseInt(url.split('/').pop() || '0');
    qualifications = qualifications.filter((t) => t.id !== id);
    return of(new HttpResponse({ status: 204 })).pipe(delay(100));
  }

  // GET /qualifications/{id}
  if (url.match(/\/qualifications\/\d+$/) && method === 'GET') {
    const id = parseInt(url.split('/').pop() || '0');
    const qualification = qualifications.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: qualification })).pipe(delay(100));
  }

  //Tools
  // GET /tools
  if (url.endsWith('/tools') && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: tools })).pipe(delay(100));
  }

  // POST /tools
  if (url.endsWith('/tools') && method === 'POST') {
    const newTool = { ...(body as any), id: Math.floor(Math.random() * 1000) };
    tools.push(newTool);
    return of(new HttpResponse({ status: 201, body: newTool })).pipe(delay(100));
  }

  // PATCH /tools/{id}
  if (url.match(/\/tools\/\d+$/) && method === 'PATCH') {
    const id = parseInt(url.split('/').pop() || '0');
    tools = tools.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
    var tool = tools.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: tool })).pipe(delay(100));
  }

  // DELETE /tools/{id}
  if (url.match(/\/tools\/\d+$/) && method === 'DELETE') {
    const id = parseInt(url.split('/').pop() || '0');
    tools = tools.filter((t) => t.id !== id);
    return of(new HttpResponse({ status: 204 })).pipe(delay(100));
  }

  // GET /tools/{id}
  if (url.match(/\/tools\/\d+$/) && method === 'GET') {
    const id = parseInt(url.split('/').pop() || '0');
    const tool = tools.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: tool })).pipe(delay(100));
  }

  //Plan
  // Post /plan
  if (url.endsWith('/plan') && method === 'POST') {
    return of(new HttpResponse({ status: 200, body: schedule })).pipe(delay(100));
  }

  // Falls keine Route matcht, Anfrage normal durchlassen (oder Fehler werfen)
  return next(req);
};
