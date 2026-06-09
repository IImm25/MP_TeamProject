import { ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { Subscription } from 'rxjs';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-navbar',
  imports: [MenubarModule, RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit, OnDestroy {
  private subscription?: Subscription;
  private translate = inject(TranslateService);
  private cd = inject(ChangeDetectorRef);

  items: MenuItem[] = [];
  currentLang = signal<string>('de');

  ngOnInit() {
    const initLang = this.translate.currentLang || this.translate.defaultLang || 'de';
    this.currentLang.set(initLang);

    this.subscription = this.translate
      .stream([
        'MENU.ADMINISTRATION',
        'MENU.TASKS',
        'MENU.EMPLOYEES',
        'MENU.QUALIFICATIONS',
        'MENU.TOOLS',
        'MENU.TURBINES',
        'MENU.SCHEDULER',
      ])
      .subscribe({
        next: (translations) => {
          this.items = [
            {
              label: translations['MENU.ADMINISTRATION'],
              icon: 'pi pi-cog',
              items: [
                { label: translations['MENU.QUALIFICATIONS'], icon: 'pi pi-id-card', routerLink: ['/qualifications'] },
                { label: translations['MENU.TOOLS'], icon: 'pi pi-wrench', routerLink: ['/tools'] },
                { label: translations['MENU.EMPLOYEES'], icon: 'pi pi-users', routerLink: ['/employees'] },
                { label: translations['MENU.TASKS'], icon: 'pi pi-list', routerLink: ['/tasks'] },
                { label: translations['MENU.TURBINES'], icon: 'pi pi-map-marker', routerLink: ['/turbines'] },
              ],
            },
            {
              label: translations['MENU.SCHEDULER'],
              icon: 'pi pi-calendar',
              routerLink: ['/scheduler'],
            },
          ];
          this.cd.detectChanges();
        },
      });
  }

  changeLang(lang: string) {
    this.translate.use(lang).subscribe(() => {
      this.currentLang.set(lang);
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }
}
