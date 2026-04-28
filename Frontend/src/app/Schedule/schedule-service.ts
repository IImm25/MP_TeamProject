import { Injectable, signal } from '@angular/core';
import { Boat, PlanRequest } from '../Models/boat';

@Injectable({
  providedIn: 'root',
})
export class ScheduleService {
  private plan = signal<Boat[] | null>(null);

  readonly currentPlan = this.plan.asReadonly();

  setPlan(plan: Boat[]) {
    this.plan.set(plan);
    sessionStorage.setItem('current_plan', JSON.stringify(plan));
  }

  private loadFromStorage(): Boat[] | null {
    const stored = sessionStorage.getItem('current_plan');
    return stored ? JSON.parse(stored) : null;
  }
}
