/* import { HttpInterceptorFn, HttpResponse} from '@angular/common/http';
import { delay, of } from 'rxjs';
import { Task } from '../Models/task';
import { Employee } from '../Models/employee';
import { Tool } from '../Models/tool';
import { Qualification } from '../Models/qualification';
import { Boat } from '../Models/boat';

let tasks: Task[] = [
  {
    id: 1,
    name: 'Ölwechsel im Getriebe',
    durationHours: 6.0,
    requiredQualifications: [
      {
        id: 2,
        requiredAmount: 2,
      },
      {
        id: 3,
        requiredAmount: 2,
      },
    ],
    requiredTools: [
      {
        id: 1,
        requiredAmount: 1,
      },
      {
        id: 2,
        requiredAmount: 2,
      },
    ],
  },
  {
    id: 2,
    name: 'Reparatur Vogelschlag (Flügel)',
    durationHours: 8.5,
    requiredQualifications: [
      {
        id: 1,
        requiredAmount: 1,
      },
      {
        id: 4,
        requiredAmount: 1,
      },
    ],
    requiredTools: [
      {
        id: 2,
        requiredAmount: 1,
      },
      {
        id: 3,
        requiredAmount: 2,
      },
    ],
  },
];

let people: Employee[] = [
  {
    id: 1,
    firstname: 'Hannes',
    lastname: 'Fiedler',
    qualificationIds: [ 2, 3 ],
  },
  {
    id: 2,
    firstname: 'Sören',
    lastname: 'Smit',
    qualificationIds: [ 1, 2, 4 ],
  },
];

let tools: Tool[] = [
  { id: 1, name: 'Schwerlast-Schlagschrauber', availableStock: 5 },
  { id: 2, name: 'Kletter-Ausrüstung (PSA)', availableStock: 20 },
  { id: 3, name: 'Harz-Reparaturset', availableStock: 10 },
  { id: 4, name: 'Spannungsprüfer', availableStock: 15 },
];

let qualifications: Qualification[] = [
  { id: 1, name: 'Höhenrettung', description: 'Höhenrettung mit Kletter-Ausrüstung' },
  { id: 2, name: 'Überleben auf See', description: 'Überleben auf See mit Harz-Reparaturset' },
  { id: 3, name: 'Elektrotechnik', description: 'Elektrotechnik mit Spannungsprüfer' },
  { id: 4, name: 'Rotorblatt-Check', description: 'Rotorblatt-Check mit Schwerlast-Schlagschrauber' },
];

let schedule: Boat[] = [
  {
    name: 'Service-Kutter Nordsee 1',
    taskItems: [
      {
        id: 1,
        name: 'Ölwechsel im Getriebe',
        durationHours: 6.0,
        requiredQualifications: [
          {
            id: 2,
            requiredAmount: 2,
          },
          {
            id: 3,
            requiredAmount: 2,
          },
        ],
        requiredTools: [
          {
            id: 1,
            requiredAmount: 1,
          },
          {
            id: 2,
            requiredAmount: 2,
          },
        ],
      },
    ],
    persons: [
      {
        id: 1,
        firstname: 'Hannes',
        lastname: 'Fiedler',
        qualificationIds: [2,3],
      },
    ],
    taskTools: [
      { id: 1, requiredAmount: 1},
      { id: 2, requiredAmount: 2},
    ],
  },
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

  //people
  // GET /people
  if (url.endsWith('/people') && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: people })).pipe(delay(100));
  }

  // POST /people
  if (url.endsWith('/people') && method === 'POST') {
    const newEmployee = { ...(body as any), id: Math.floor(Math.random() * 1000) };
    people.push(newEmployee);
    return of(new HttpResponse({ status: 201, body: newEmployee })).pipe(delay(100));
  }

  // PATCH /people/{id}
  if (url.match(/\/people\/\d+$/) && method === 'PATCH') {
    const id = parseInt(url.split('/').pop() || '0');
    people = people.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
    var employee = people.find((t) => t.id === id);
    return of(new HttpResponse({ status: 200, body: employee })).pipe(delay(100));
  }

  // DELETE /people/{id}
  if (url.match(/\/people\/\d+$/) && method === 'DELETE') {
    const id = parseInt(url.split('/').pop() || '0');
    people = people.filter((t) => t.id !== id);
    return of(new HttpResponse({ status: 204 })).pipe(delay(100));
  }

  // GET /people/{id}
  if (url.match(/\/people\/\d+$/) && method === 'GET') {
    const id = parseInt(url.split('/').pop() || '0');
    const employee = people.find((t) => t.id === id);
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
 */
