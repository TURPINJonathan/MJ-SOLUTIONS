import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonComponent } from '#ui/button/button';
import { CardComponent } from '#ui/card/card';
import { FormComponent } from '#ui/form/form';
import { InputComponent } from '#ui/input/input';
import { isValidEmail, isValidPassword } from '#shared/utils/validation.utils';
import { AuthService } from '#services/auth/auth.service';
import { ToastrService } from 'ngx-toastr';
import { ToastUtils } from '#utils/toast.utils';
import { Store } from '@ngrx/store';
import { setUser } from '#store/user/user.actions';
import { User } from '#models/user.model';

@Component({
  selector: 'app-login',
	standalone: true,
  imports: [
		ButtonComponent,
		CardComponent,
		FormComponent,
		InputComponent,
		FormsModule
	],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginPage {
	isLoading = false;
	email = '';
	emailTouched = false;
	password = '';
	passwordTouched = false;
	inputPasswordType = "password";

	isValidEmail = isValidEmail;
	isValidPassword = isValidPassword;

	constructor(
		private authService: AuthService,
		private router: Router,
		private toast: ToastUtils,
		private store: Store
	) {}

	get emailInvalid(): boolean {
		return this.emailTouched && !this.isValidEmail(this.email);
	}

	get passwordInvalid(): boolean {
		return this.passwordTouched && !this.isValidPassword(this.password);
	}

	handleTogglePasswordVisibility(): void {
		this.inputPasswordType = this.inputPasswordType === "password" ? "text" : "password";
	}

	onSubmit(event: Event): void {
		this.isLoading = true;
		this.emailTouched = true;
		this.passwordTouched = true;

		if (this.emailInvalid || this.passwordInvalid) {
			this.toast.error('Veuillez vérifier vos informations', 'Erreur lors de la connexion');
			return;
		}

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
				this.authService.checkSession().subscribe((user: User | null) => {
					if (user) {
						this.isLoading = false;
						this.toast.success('Connexion réussie', 'Bienvenue !');
						this.store.dispatch(setUser({ user }));
						this.router.navigate(['/dashboard']);
					} else {
						this.toast.error('Erreur lors de la connexion', 'Identifiants invalides');
					}
        });
      },
      error: (err) => {
        this.isLoading = false;
				if (err.status === 429) {
					this.toast.warning('Trop de tentatives de connexion', 'Veuillez réessayer plus tard');
				} else {
					this.toast.error('Erreur lors de la connexion', 'Identifiants invalides');
				}
      }
    });
	}
}
