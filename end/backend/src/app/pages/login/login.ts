import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '#ui/button/button';
import { CardComponent } from '#ui/card/card';
import { FormComponent } from '#ui/form/form';
import { InputComponent } from '#ui/input/input';
import { isValidEmail, isValidPassword } from '#shared/utils/validation.utils';

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

	get emailInvalid() {
		return this.emailTouched && !this.isValidEmail(this.email);
	}

	get passwordInvalid() {
		return this.passwordTouched && !this.isValidPassword(this.password);
	}

	handleTogglePasswordVisibility() {
		this.inputPasswordType = this.inputPasswordType === "password" ? "text" : "password";
	}

	onSubmit(event: Event) {
		this.isLoading = true;
		this.emailTouched = true;
		this.passwordTouched = true;

		if (this.emailInvalid || this.passwordInvalid) {
			this.isLoading = false;
			// TODO TOAST
			return;
		}

		// TODO
	}
}
