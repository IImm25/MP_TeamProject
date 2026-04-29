import { Component, EventEmitter, Input, OnInit, Output, WritableSignal, inject, model, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { environment } from '../../../../environments/environment';
import { EmployeeCreateUpdate, Employee } from '../../../Models/employee';
import { Qualification } from '../../../Models/qualification';
import { HttpService } from '../../../Services/http-service';

@Component({
  selector: 'app-dialog-employee',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    MultiSelectModule,
  ],
  templateUrl: './dialog-employee.html',
  styleUrl: './dialog-employee.css',
})
export class DialogEmployee implements OnInit {
  private fb = inject(FormBuilder);
  private httpService = inject(HttpService);

  @Input({required: true}) type: 'Edit' | 'New' | 'Detail' = 'New';
  @Input() set selectedEmployee (emp: Employee | null) {
    this.currentEmployee = emp;
    if (emp) {
      this.employeeForm.patchValue({
        firstname: emp.firstname,
        lastname: emp.lastname,
        qualifications: emp.qualifications || [],
      });
      if (this.type === 'Detail') {
        this.employeeForm.disable();
      } else {
        this.employeeForm.enable();
      }
    } else {
      this.employeeForm.reset({
        firstname: '',
        lastname: '',
        qualifications: [],
      });
      if (this.type !== 'Detail') {
        this.employeeForm.enable();
      }
    }
  };
  visible = model<boolean>(false);

  @Output() onSave = new EventEmitter<void>();

  currentEmployee: Employee | null = null;
  allQualifications: WritableSignal<Qualification[]> = signal([]);

  employeeForm = this.fb.group({
    firstname: ['', Validators.required],
    lastname: ['', Validators.required],
    qualifications: [[] as Qualification[], Validators.required],
  });

  ngOnInit(): void {
    this.httpService.getQualifications().subscribe((qualifications) => {
      this.allQualifications.set(qualifications);
    });
  }

  save() {
    if (this.employeeForm.invalid) return;

    const payload: EmployeeCreateUpdate = {
      firstname: this.employeeForm.value.firstname || '',
      lastname: this.employeeForm.value.lastname || '',
      qualificationIds: this.employeeForm.value.qualifications?.map((q) => q.id) || [],
    };
    const request =
      this.type === 'Edit' && this.currentEmployee
        ? this.httpService.updateEmployee(this.currentEmployee.id, payload)
        : this.httpService.createEmployee(payload);

    request.subscribe(() => {
      this.onSave.emit();
      this.visible.set(false);
    });
  }
}
