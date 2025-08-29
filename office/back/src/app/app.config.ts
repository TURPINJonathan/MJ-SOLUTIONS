import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { CommonModule } from '@angular/common';
import { provideHttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideToastr, ToastNoAnimationModule } from 'ngx-toastr';
import { reducers } from 'src/app/store';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideToastr(),
    provideStore(reducers),
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    importProvidersFrom(CommonModule, FormsModule, 
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
    })),
		provideStoreDevtools({ maxAge: 25, logOnly: false }),
]
};
