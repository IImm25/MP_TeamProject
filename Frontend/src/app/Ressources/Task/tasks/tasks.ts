import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogTask } from '../dialog-task/dialog-task';
import { Task } from '../../../Models/task';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-tasks',
  imports: [TranslatePipe, ButtonModule, TableModule, DialogTask, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css',
})
export class Tasks implements OnInit {
  private http = inject(HttpClient);
  private translate = inject(TranslateService);
  private confirmationService = inject(ConfirmationService);

  type: WritableSignal<'Edit' | 'New' | 'Detail'> = signal('New');
  selectedTask: WritableSignal<Task | null> = signal(null);
  visible: WritableSignal<boolean> = signal(false);
  tasks: WritableSignal<Task[]> = signal([]);

  apiUrl = environment.apiUrl;

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.http.get<Task[]>(`${this.apiUrl}/tasks`).subscribe(tasks => this.tasks.set(tasks));
  }

  deleteTask(task: Task) {
    this.confirmationService.confirm({
      message: this.translate.instant('TASKS.DIALOG.DELETE_MESSAGE'),
      header: this.translate.instant('TASKS.DIALOG.DELETE_TITLE'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('COMMON.DELETE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.http.delete(`${this.apiUrl}/tasks/${task.id}`).subscribe(() => {
          this.tasks.update(tasks => tasks.filter(t => t.id !== task.id));
        });
      }
    });
  }

  formatDuration(decimalHours: number): string {
    if (!decimalHours) return '0 h';

    const hours = Math.floor(decimalHours);
    const minutes = Math.round((decimalHours - hours) * 60);

    if (hours > 0 && minutes > 0) {
      return `${hours} h ${minutes} min`;
    } else if (hours > 0) {
      return `${hours} h`;
    } else {
      return `${minutes} min`;
    }
  }
}
