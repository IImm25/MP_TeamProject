import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { delay, of } from 'rxjs';
import { Task, TaskSummary, TaskTool } from '../Models/task';
import { Employee, EmployeeSummary } from '../Models/employee';
import { Tool } from '../Models/tool';
import { Qualification } from '../Models/qualification';
import { Boat, PlanRequest, PlanResponse } from '../Models/boat';

// --- IN-MEMORY DATABASE ---
let tasks: Task[] = [
  {
    id: 1,
    name: 'Getriebewartung',
    durationHours: 4.5,
    requiredQualifications: [{ qualificationId: 1, requiredAmount: 1 }],
    requiredTools: [{ toolId: 1, requiredAmount: 1 }],
  },
  {
    id: 2,
    name: 'Rotorblatt Reparatur',
    durationHours: 8,
    requiredQualifications: [{ qualificationId: 2, requiredAmount: 2 }],
    requiredTools: [{ toolId: 2, requiredAmount: 1 }],
  },
];

let people: Employee[] = [
  {
    id: 1,
    firstname: 'Hannes',
    lastname: 'Fiedler',
    qualifications: [{ id: 1, name: 'Mechaniker', description: '' }],
  },
  {
    id: 2,
    firstname: 'Sören',
    lastname: 'Smit',
    qualifications: [{ id: 2, name: 'Kletterer', description: '' }],
  },
];

let tools: Tool[] = [
  { id: 1, name: 'Schlagschrauber', availableStock: 5 },
  { id: 2, name: 'PSA Ausrüstung', availableStock: 20 },
];

let qualifications: Qualification[] = [
  { id: 1, name: 'Mechaniker', description: 'Basis' },
  { id: 2, name: 'Kletterer', description: 'Höhenarbeit' },
];

export const httpMockInterceptor: HttpInterceptorFn = (req, next) => {
  const { url, method, body } = req;

  const getId = () => parseInt(url.split('/').pop() || '0');
  const generateId = () => Math.floor(Math.random() * 1000) + 10;

  // --- QUALIFICATIONS ---
  if (url.endsWith('/qualifications')) {
    if (method === 'GET')
      return of(new HttpResponse({ status: 200, body: qualifications })).pipe(delay(100));
    if (method === 'POST') {
      const newQual = { ...(body as any), id: generateId() };
      qualifications.push(newQual);
      return of(new HttpResponse({ status: 201, body: newQual }));
    }
  }
  if (url.match(/\/qualifications\/\d+$/)) {
    const id = getId();
    if (method === 'PATCH') {
      qualifications = qualifications.map((q) => (q.id === id ? { ...q, ...(body as any) } : q));
      return of(new HttpResponse({ status: 200, body: qualifications.find((q) => q.id === id) }));
    }
    if (method === 'DELETE') {
      qualifications = qualifications.filter((q) => q.id !== id);
      return of(new HttpResponse({ status: 204 }));
    }
  }

  // --- TOOLS ---
  if (url.endsWith('/tools')) {
    if (method === 'GET') return of(new HttpResponse({ status: 200, body: tools }));
    if (method === 'POST') {
      const newTool = { ...(body as any), id: generateId() };
      tools.push(newTool);
      return of(new HttpResponse({ status: 201, body: newTool }));
    }
  }
  if (url.match(/\/tools\/\d+$/)) {
    const id = getId();
    if (method === 'PATCH') {
      tools = tools.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
      return of(new HttpResponse({ status: 200, body: tools.find((t) => t.id === id) }));
    }
    if (method === 'DELETE') {
      tools = tools.filter((t) => t.id !== id);
      return of(new HttpResponse({ status: 204 }));
    }
  }

  // --- PERSONS (EMPLOYEES) ---
  if (url.endsWith('/persons')) {
    if (method === 'GET') {
      const summaries: EmployeeSummary[] = people.map((p) => ({
        id: p.id,
        firstname: p.firstname,
        lastname: p.lastname,
      }));
      return of(new HttpResponse({ status: 200, body: summaries }));
    }
    if (method === 'POST') {
      const b = body as any;
      const newPerson: Employee = {
        id: generateId(),
        firstname: b.firstname,
        lastname: b.lastname,
        qualifications: qualifications.filter((q) => b.qualificationIds?.includes(q.id)),
      };
      people.push(newPerson);
      return of(new HttpResponse({ status: 201, body: newPerson }));
    }
  }
  if (url.match(/\/persons\/\d+$/)) {
    const id = getId();
    if (method === 'GET')
      return of(new HttpResponse({ status: 200, body: people.find((p) => p.id === id) }));
    if (method === 'PATCH') {
      const b = body as any;
      people = people.map((p) =>
        p.id === id
          ? {
              ...p,
              ...b,
              qualifications: b.qualificationIds
                ? qualifications.filter((q) => b.qualificationIds.includes(q.id))
                : p.qualifications,
            }
          : p,
      );
      return of(new HttpResponse({ status: 200, body: people.find((p) => p.id === id) }));
    }
    if (method === 'DELETE') {
      people = people.filter((p) => p.id !== id);
      return of(new HttpResponse({ status: 204 }));
    }
  }

  // --- TASKS ---
  if (url.endsWith('/tasks')) {
    if (method === 'GET') {
      const summaries: TaskSummary[] = tasks.map((t) => ({
        id: t.id,
        name: t.name,
        durationHours: t.durationHours,
      }));
      return of(new HttpResponse({ status: 200, body: summaries }));
    }
    if (method === 'POST') {
      const newTask = { ...(body as any), id: generateId() };
      tasks.push(newTask);
      return of(new HttpResponse({ status: 201, body: newTask }));
    }
  }
  if (url.match(/\/tasks\/\d+$/)) {
    const id = getId();
    if (method === 'GET')
      return of(new HttpResponse({ status: 200, body: tasks.find((t) => t.id === id) }));
    if (method === 'PATCH') {
      tasks = tasks.map((t) => (t.id === id ? { ...t, ...(body as any) } : t));
      return of(new HttpResponse({ status: 200, body: tasks.find((t) => t.id === id) }));
    }
    if (method === 'DELETE') {
      tasks = tasks.filter((t) => t.id !== id);
      return of(new HttpResponse({ status: 204 }));
    }
  }

  // --- PLAN ---
  if (url.endsWith('/plan') && method === 'POST') {
    const body = req.body as PlanRequest;
    const mockSchedule: PlanResponse =
      {
        totalTime: 0,
        boats: [
         /* {
            taskItems: tasks.filter((t) => (body as any).taskItemIds.includes(t.id)) as TaskSummary[],
            persons: people.filter((p) => (body as any).personIds.includes(p.id)) as EmployeeSummary[],
            tools: tools
        .filter((t) => body.toolIds.includes(t.id))
        .map(t => ({
          toolId: t.id,
          requiredAmount: 1
        }))
          }*/
        ],
        qualificationDiff: [
          {
            id: 1,
            required: 2,
            available: 1
          },
          {
            id: 2,
            required: 1,
            available: 5
          }
        ],
        toolDiff: [
          {
            id: 1,
            required: 2,
            available: 1
          },
          {
            id: 2,
            required: 1,
            available: 5
          }
        ]
      };
    return of(new HttpResponse({ status: 400, body: mockSchedule })).pipe(delay(500));
  }

  return next(req);
};
