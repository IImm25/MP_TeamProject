import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { HttpClient } from '@angular/common/http';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogEmployee } from '../dialog-employee/dialog-employee';
import { environment } from '../../../../environments/environment';
import { Qualification } from '../../../Models/qualification';
import { Employees as EmployeeModel } from '../../../Models/employees';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [TranslatePipe, ButtonModule, TableModule, ConfirmDialogModule, DialogEmployee],
  providers: [ConfirmationService],
  templateUrl: './employees.html',
  styleUrl: './employees.css'
})
export class Employees implements OnInit {
  @ViewChild(DialogEmployee) dialogComp!: DialogEmployee;

  private http = inject(HttpClient);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);
  apiUrl = environment.apiUrl;

  employees = signal<EmployeeModel[]>([]);
  qualifications = signal<Qualification[]>([]);

  dialogVisible = false;
  dialogType: 'Edit' | 'New' = 'New';

  ngOnInit() {
    this.loadEmployees();
    this.loadQualifications();
  }

  loadEmployees() {
    this.http.get<EmployeeModel[]>(`${this.apiUrl}/employees`).subscribe(data => this.employees.set(data));
  }

  loadQualifications() {
    this.http.get<Qualification[]>(`${this.apiUrl}/qualifications`).subscribe(data => this.qualifications.set(data));
  }

  openNew() {
    this.dialogType = 'New';
    this.dialogComp.patchForm(null);
    this.dialogVisible = true;
  }

  openEdit(employee: EmployeeModel) {
    this.dialogType = 'Edit';
    this.dialogComp.patchForm(employee);
    this.dialogVisible = true;
  }

  deleteEmployee(employee: EmployeeModel) {
    this.confirmationService.confirm({
      message: this.translate.instant('EMPLOYEES.DIALOG.DELETE_MESSAGE', { firstname: employee.firstname, lastname: employee.lastname }),
      header: this.translate.instant('EMPLOYEES.DIALOG.DELETE_TITLE'),
      accept: () => {
        this.http.delete(`${this.apiUrl}/employees/${employee.id}`).subscribe(() => this.loadEmployees());
      }
    });
  }
}
