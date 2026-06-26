import { Component, inject, model } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { DatePicker } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Button } from "primeng/button";

@Component({
  selector: 'app-weather-dialog',
  imports: [DialogModule, TranslateModule, DatePicker, InputNumberModule, ReactiveFormsModule, Button],
  templateUrl: './weather-dialog.html',
  styleUrl: './weather-dialog.css',
})
export class WeatherDialog {
saveWeather() {
throw new Error('Method not implemented.');
}
  private formBuilder = inject(FormBuilder);
  visible = model<boolean>(false);

  weatherForm = this.formBuilder.group({
    date: [new Date(), Validators.required],
    speed: [36, [Validators.required, Validators.min(1)]],
  });
}
