import { Component, EventEmitter, Input, Output, inject, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { environment } from '../../../../environments/environment';
import { EmployeeCreateUpdate, Employee as EmployeeModel } from '../../../Models/employee';
import { Qualification } from '../../../Models/qualification';
import { HttpService } from '../../../Services/http-service';

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
  private httpService = inject(HttpService);

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

    const payload: EmployeeCreateUpdate = {
       firstname: this.employeeForm.value.firstname || '',
       lastname: this.employeeForm.value.lastname || '',
       qualificationIds: this.employeeForm.value.qualifications?.map(q => q.id) || [],
    };
    const request = this.type === 'Edit' && this.selectedId
      ? this.httpService.updateEmployee(this.selectedId, payload)
      : this.httpService.createEmployee(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.close();
    });
  }
}
