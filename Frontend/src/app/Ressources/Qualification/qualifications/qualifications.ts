import { Component, inject, OnInit, signal, ViewChild, WritableSignal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { HttpService } from '../../../Services/http-service';
import { Qualification } from '../../../Models/qualification';
import { DialogModule } from 'primeng/dialog';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';
import { DialogQualification } from "../dialog-qualification/dialog-qualification";

@Component({
  selector: 'app-qualifications',
  standalone: true,
  imports: [
    CommonModule,
    TranslatePipe,
    ButtonModule,
    TableModule,
    ConfirmDialogModule,
    DialogModule,
    FormsModule,
    ReactiveFormsModule,
    InputTextModule,
    DialogQualification
],
  providers: [ConfirmationService],
  templateUrl: './qualifications.html',
  styleUrl: './qualifications.css',
})
export class Qualifications implements OnInit {
  @ViewChild(DialogQualification) dialogComp!: DialogQualification;
  http = inject(HttpService);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);

  qualifications = signal<Qualification[]>([]);

  dialogVisible: WritableSignal<boolean> = signal(false);
  dialogType: WritableSignal<'Edit' | 'New' | 'Detail'> = signal('New');
  selectedQualification: WritableSignal<Qualification | null> = signal(null);

  ngOnInit() {
    this.loadQualifications();
  }

  loadQualifications() {
    this.http.getQualifications().subscribe((data) => this.qualifications.set(data));
  }

  deleteQualification(qualification: Qualification) {
    this.confirmationService.confirm({
      message: this.translate.instant('QUALIFICATIONS.DIALOG.DELETE_MESSAGE', {name: qualification.name}),
      header: this.translate.instant('QUALIFICATIONS.DIALOG.DELETE_TITLE'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('COMMON.DELETE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.http.deleteQualification(qualification.id).subscribe(() => {
          this.qualifications.update((qualificationList) =>
            qualificationList.filter((q) => q.id !== qualification.id),
          );
        });
      },
    });
  }
}
