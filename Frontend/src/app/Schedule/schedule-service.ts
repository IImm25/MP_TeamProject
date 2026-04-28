import { Injectable, signal, WritableSignal } from '@angular/core';
import { Boat, PlanRequest } from '../Models/boat';
import { Task } from '../Models/task';

@Injectable({
  providedIn: 'root',
})
export class ScheduleService {
  private plan = signal<Boat[] | null>(null);

  readonly currentPlan: WritableSignal<Boat[] | null> = this.plan;
  readonly selectedRequest: WritableSignal<PlanRequest | null> = signal(null)

  setPlan(plan: Boat[], request: PlanRequest) {
    this.plan.set(plan);
    sessionStorage.setItem('current_plan', JSON.stringify(plan));

    this.selectedRequest.set(request);
    sessionStorage.setItem('current_request', JSON.stringify(request));
  }

  loadBoatsFromStorage(): Boat[] | null {
    const stored = sessionStorage.getItem('current_plan');
    return stored ? JSON.parse(stored) : null;
  }

  loadRequestFromStorage(): PlanRequest | null {
    const stored = sessionStorage.getItem('current_request');
    return stored ? JSON.parse(stored) : null;
  }
}
