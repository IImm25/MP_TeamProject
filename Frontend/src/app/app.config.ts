import {
  ApplicationConfig,
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { FormBuilder } from '@angular/forms';
import { httpMockInterceptor } from './Services/interceptor';
import { definePreset } from '@primeuix/themes';

const BluePreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#f0f7ff',
      100: '#e0effe',
      200: '#bae0fd',
      300: '#7ccbf9',
      400: '#36b1f2',
      500: '#007bff',
      600: '#0056b3',
      700: '#024282',
      800: '#053668',
      900: '#0a2d54',
      950: '#071d38'
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withFetch(), withInterceptors([httpMockInterceptor])),
    provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: './i18n/',
        suffix: '.json',
      }),
      fallbackLang: 'de',
    }),
    providePrimeNG({
      theme: {
        preset: BluePreset,
        options: {
          prefix: 'p',
          darkModeSelector: '.my-dark-theme',
        },
      },
    }),
    importProvidersFrom([FormBuilder]),
  ],
};
