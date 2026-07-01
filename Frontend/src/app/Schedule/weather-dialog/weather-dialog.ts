import { Component, inject, model } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Button } from 'primeng/button';
import { ScheduleService } from '../schedule-service';
import { HttpService } from '../../Services/http-service';
import { PlanResponse } from '../../Models/boat';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-weather-dialog',
  imports: [DialogModule, TranslateModule, InputNumberModule, ReactiveFormsModule, Button],
  templateUrl: './weather-dialog.html',
  styleUrl: './weather-dialog.css',
})
export class WeatherDialog {
  private formBuilder = inject(FormBuilder);
  private scheduleService = inject(ScheduleService);
  private http = inject(HttpService);
  private router = inject(Router);
  private messageService = inject(MessageService);
  visible = model<boolean>(false);
  date: Date = new Date();

  weatherForm = this.formBuilder.group({
    speed: [36, [Validators.required, Validators.min(1)]],
  });

  saveWeather() {
    let request = this.scheduleService.loadRequestFromStorage();
    if (new Date(request?.date!) === this.date) {
      request!.boatSpeed = this.weatherForm.controls.speed.value ?? 36;
    } else {
      request = {
        maxWorkHours: 8,
        boatNumber: 2,
        boatSpeed: this.weatherForm.controls.speed.value ?? 36,
        date: this.date,
      };
    }

    this.http.postPlan(request!, this.date.toISOString().split('T')[0]).subscribe({
      next: (plan: PlanResponse) => {
        this.scheduleService.setPlan(plan, request!, this.date!);
        this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigate(['/schedule-view']);
        });
        this.visible.set(false);
      },
    });
  }
}
