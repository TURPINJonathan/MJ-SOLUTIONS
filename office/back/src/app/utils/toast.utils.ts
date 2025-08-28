import { Injectable } from '@angular/core';
import { ToastrService, IndividualConfig } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class ToastUtils {

  constructor(
    private toastr: ToastrService,
  ) {}

  success(message: string, title?: string, config?: Partial<IndividualConfig>): void {
    this.toastr.success(
      message,
      title ?? 'Succès',
      { ...config }
    );
  }

  error(message: string, title?: string, config?: Partial<IndividualConfig>): void {
    this.toastr.error(
      message,
      title ?? 'Erreur',
      { ...config }
    );
  }

  info(message: string, title?: string, config?: Partial<IndividualConfig>): void {
    this.toastr.info(
      message,
      title ?? 'Information',
      { ...config }
    );
  }

  warning(message: string, title?: string, config?: Partial<IndividualConfig>): void {
    this.toastr.warning(
      message,
      title ?? 'Avertissement',
      { ...config }
    );
  }
}