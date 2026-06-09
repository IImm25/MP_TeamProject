import { Component, EventEmitter, inject, Input, model, Output, signal } from '@angular/core';
import { CreateTurbine, Turbine } from '../../../Models/turbine';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpService } from '../../../Services/http-service';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-dialog-turbine',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,],
  templateUrl: './dialog-turbine.html',
  styleUrl: './dialog-turbine.css',
})
export class DialogTurbine {
  private fb = inject(FormBuilder);
  private httpService = inject(HttpService);

  @Input({required: true}) type: 'Edit' | 'New' | 'Detail' = 'New';
  @Input() set selectedTurbine (turbine: Turbine | null) {
    this.currentTurbine = turbine;
    if (turbine) {
      this.turbineForm.patchValue({
        name: turbine.name,
        latitude: turbine.latitude,
        longitude: turbine.longitude,
      });
      if (this.type === 'Detail') {
        this.turbineForm.disable();
      } else {
        this.turbineForm.enable();
      }
    } else {
      this.turbineForm.reset({
        name: '',
        latitude: null,
        longitude: null
      });
      if (this.type !== 'Detail') {
        this.turbineForm.enable();
      }
    }
  };
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  currentTurbine: Turbine | null = null;

  turbineForm = this.fb.group({
    name: ['', Validators.required],
    latitude: [null as number | null, [Validators.required, Validators.min(-90), Validators.max(90)]],
    longitude: [null as number | null, [Validators.required, Validators.min(-180), Validators.max(180)]]
  });

  save() {
    if (this.turbineForm.invalid) return;

    const payload: CreateTurbine = {
      name: this.turbineForm.value.name || '',
      latitude: this.turbineForm.value.latitude || 0,
      longitude: this.turbineForm.value.longitude || 0,
    };
    const request =
      this.type === 'Edit' && this.currentTurbine
        ? this.httpService.updateTurbine(this.currentTurbine.id, payload)
        : this.httpService.createTurbine(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.visible.set(false);
    });
  }
}
