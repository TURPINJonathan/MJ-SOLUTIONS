import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import { provideToastr, ToastNoAnimationModule, ToastrModule } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export const appConfig: ApplicationConfig = {
  providers: [
		provideHttpClient(),
		provideToastr(),
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
		importProvidersFrom(
			CommonModule,
			FormsModule,
    	// ToastNoAnimationModule.forRoot(),
			ToastNoAnimationModule.forRoot({
        tapToDismiss: true,
        closeButton: true,
        newestOnTop: true,
        progressBar: true,
        progressAnimation: 'increasing',
        timeOut: 5000,
        extendedTimeOut: 1000,
        disableTimeOut: false,
        maxOpened: 4,
        easeTime: 300,
        positionClass: 'toast-top-right'
			}),
		)
  ]
};
