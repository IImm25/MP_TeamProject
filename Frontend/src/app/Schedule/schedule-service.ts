import { Injectable, signal, WritableSignal } from '@angular/core';
import { Boat, PlanRequest, PlanResponse } from '../Models/boat';
import { Task } from '../Models/task';

@Injectable({
  providedIn: 'root',
})
export class ScheduleService {
  private plan = signal<PlanResponse | null>(null);

  readonly currentPlan: WritableSignal<PlanResponse | null> = this.plan;
  readonly selectedRequest: WritableSignal<PlanRequest | null> = signal(null)
  readonly selectedDate: WritableSignal<Date> = signal(new Date());

  setPlan(plan: PlanResponse, request: PlanRequest, date: Date) {
    this.plan.set(plan);
    sessionStorage.removeItem('current_plan');
    sessionStorage.setItem('current_plan', JSON.stringify(plan));

    this.selectedDate.set(date);
    sessionStorage.removeItem('current_date');
    sessionStorage.setItem('current_date', JSON.stringify(date));

    this.selectedRequest.set(request);
    sessionStorage.removeItem('current_request');
    sessionStorage.setItem('current_request', JSON.stringify(request));
  }

  loadBoatsFromStorage(): PlanResponse| null {
    const stored = sessionStorage.getItem('current_plan');
    return stored ? JSON.parse(stored) : null;
  }

  loadRequestFromStorage(): PlanRequest | null {
    const stored = sessionStorage.getItem('current_request');
    const date = sessionStorage.getItem('current_date');
    let request: PlanRequest | null = null;
    if (stored && date) {
      request = JSON.parse(stored);
      request!.date = new Date(date);
    }
    return stored ? JSON.parse(stored) : null;
  }
}
