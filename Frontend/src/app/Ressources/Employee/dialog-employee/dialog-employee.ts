import { Component, EventEmitter, Input, Output, inject, signal, WritableSignal, model, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Employees as EmployeeModel } from '../../../Models/employees';
import { Qualification } from '../../../Models/qualification';

@Component({
  selector: 'app-dialog-employee',
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, TranslateModule,
    DialogModule, ButtonModule, InputTextModule, MultiSelectModule
  ],
  templateUrl: './dialog-employee.html',
  styleUrl: './dialog-employee.css',
})
export class DialogEmployee {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  apiUrl = environment.apiUrl;

  @Input() type: 'Edit' | 'New' = 'New';
  @Input() allQualifications: Qualification[] = [];
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  selectedId: number | null = null;

  employeeForm = this.fb.group({
    firstname: ['', Validators.required],
    lastname: ['', Validators.required],
    qualifications: [[] as Qualification[], Validators.required]
  });


  patchForm(employee: EmployeeModel | null) {
    if (employee) {
      this.selectedId = employee.id;
      this.employeeForm.patchValue({
        firstname: employee.firstname,
        lastname: employee.lastname,
        qualifications: employee.qualifications
      });
    } else {
      this.selectedId = null;
      this.employeeForm.reset();
    }
  }

  close() {
    this.visible.set(false);
  }

  save() {
    if (this.employeeForm.invalid) return;

    const payload = this.employeeForm.value;
    const request = this.type === 'Edit' && this.selectedId
      ? this.http.patch(`${this.apiUrl}/employees/${this.selectedId}`, payload)
      : this.http.post(`${this.apiUrl}/employees`, payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.close();
    });
  }
}
