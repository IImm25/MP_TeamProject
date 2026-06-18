import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { Turbine } from '../../../Models/turbine';
import { HttpService } from '../../../Services/http-service';
import { DialogTurbine } from '../dialog-turbine/dialog-turbine';

// PrimeNG Imports
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-turbines',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    CardModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    ToastModule,
    TooltipModule,
    TranslatePipe,
    DialogTurbine,
    ConfirmDialogModule
],
  providers: [MessageService, ConfirmationService],
  templateUrl: './turbines.html',
  styleUrl: './turbines.css',
})
export class Turbines implements OnInit {
    private http = inject(HttpService);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);

  type: WritableSignal<'Edit' | 'New' | 'Detail'> = signal('New');
  selectedTurbine: WritableSignal<Turbine | null> = signal(null);
  visible: WritableSignal<boolean> = signal(false);
  turbines: WritableSignal<Turbine[]> = signal([]);

  ngOnInit() {
    this.loadTurbines();
  }

  loadTurbines() {
    this.http.getAllTurbines().subscribe((turbines) => this.turbines.set(turbines));
  }

  openEditOrDetail(turbine: Turbine, mode: 'Edit' | 'Detail') {
    this.type.set(mode);
    this.selectedTurbine.set(turbine);
    this.visible.set(true);
  }

  deleteTurbine(turbine: Turbine) {
    this.confirmationService.confirm({
      message: this.translate.instant('TURBINES.DIALOG.DELETE_MESSAGE', {
        name: turbine.name,
      }),
      header: this.translate.instant('TURBINES.DIALOG.DELETE_TITLE'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('COMMON.DELETE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.http.deleteTurbine(turbine.id).subscribe(() => {
          this.turbines.update((turbines) => turbines.filter((t) => t.id !== turbine.id));
        });
      },
    });
  }
}
