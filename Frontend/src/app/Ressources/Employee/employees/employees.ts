import { Component, inject, OnInit, signal, ViewChild, WritableSignal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogEmployee } from '../dialog-employee/dialog-employee';
import { environment } from '../../../../environments/environment';
import { Qualification } from '../../../Models/qualification';
import { Employee as EmployeeModel, EmployeeSummary } from '../../../Models/employee';
import { HttpService } from '../../../Services/http-service';
import { ChipModule } from 'primeng/chip';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [TranslatePipe, ButtonModule, TableModule, ConfirmDialogModule, DialogEmployee,ChipModule],
  providers: [ConfirmationService],
  templateUrl: './employees.html',
  styleUrl: './employees.css',
})
export class Employees implements OnInit {


  http = inject(HttpService);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);

  employees = signal<EmployeeModel[]>([]);
  qualifications = signal<Qualification[]>([]);

  dialogVisible: WritableSignal<boolean> = signal(false);
  dialogType: WritableSignal<'Edit' | 'New' | 'Detail'> =  signal('New');
  selectedEmployee: WritableSignal<EmployeeModel | null> = signal(null);

  ngOnInit() {
    this.http.getQualifications().subscribe((data) => this.qualifications.set(data));
    this.loadEmployees();
  }


loadEmployees() {
  this.http.getEmployees().subscribe((summaries) => {
    const employeeList = summaries as EmployeeModel[];
    this.employees.set(employeeList);

    employeeList.forEach(employee => {
      this.http.getEmployeeById(employee.id).subscribe(fullDetails => {
        this.employees.update(current =>
          current.map(e => e.id === fullDetails.id ? fullDetails : e)
        );
      });
    });
  });
}

  deleteEmployee(employee: EmployeeModel) {
    this.confirmationService.confirm({
      message: this.translate.instant('EMPLOYEES.DIALOG.DELETE_MESSAGE', {
        firstname: employee.firstname,
        lastname: employee.lastname,
      }),
      header: this.translate.instant('EMPLOYEES.DIALOG.DELETE_TITLE'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('COMMON.DELETE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.http.deleteEmployee(employee.id).subscribe(() => {
          this.employees.update((employees) => employees.filter((e) => e.id !== employee.id));
        });
      },
    });
  }

  getTaskQualifications(id: number) {
    this.http.getEmployees().subscribe((data) => {
      this.employees.set(data as unknown as EmployeeModel[]);
    });
  }
}
