import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import { HttpService } from '../../Services/http-service';
import { PlanRequest, PlanResponse } from '../../Models/boat';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ScheduleService } from '../schedule-service';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { PanelModule } from 'primeng/panel';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-scheduler',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ToastModule,
    PanelModule,
    InputNumberModule,
    DatePickerModule,
    ButtonModule
  ],
  providers: [MessageService],
  templateUrl: './scheduler.html',
  styleUrl: './scheduler.css',
})
export class Scheduler {
  private fb = inject(FormBuilder);
  private http = inject(HttpService);
  private router = inject(Router);
  private planService = inject(ScheduleService);
  private messageService = inject(MessageService);
  private translate = inject(TranslateService);

  schedulerForm = this.fb.group({
    maxTime: new FormControl<number>(8, [Validators.required, Validators.min(1)]),
    boatAmount: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
    date: new FormControl<Date>(new Date(), [Validators.required]),
    speed: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
  });


  generatePlan() {
    if (this.schedulerForm.invalid || !this.validate()) {
      this.schedulerForm.markAllAsTouched();
      return;
    }

    const val = this.schedulerForm.value;
    const request: PlanRequest = {
      maxTime: val.maxTime!,
      boatNumber: val.boatAmount!,
      date: val.date!.toISOString(),
      speed: val.speed!,
    };

    this.http.postPlan(request).subscribe({
      next: (plan: PlanResponse) => {
        this.planService.setPlan(plan, request);
        this.router.navigate(['/schedule-view']);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('MESSAGES.ERROR'),
          detail: this.translate.instant('MESSAGES.ERROR_GENERATING_PLAN')
        });
      }
    });
  }

  validate(): boolean {
        const maxTime = this.schedulerForm.controls.maxTime;
        const boatAmount = this.schedulerForm.controls.boatAmount;
        const date = this.schedulerForm.controls.date;
        const speed = this.schedulerForm.controls.speed;

        maxTime.markAsTouched();
        boatAmount.markAsTouched();
        date.markAsTouched();
        speed.markAsTouched();

        return maxTime.valid && boatAmount.valid && date.valid && speed.valid;
  }
}
