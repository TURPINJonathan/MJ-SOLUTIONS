import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { HeaderLayout } from '#layout/header/header';
import { FooterLayout } from '#layout/footer/footer';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [
		CommonModule,
		RouterOutlet,
		HeaderLayout,
		FooterLayout
	],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
	constructor(
		private router: Router
	) {}

  protected title = 'Back Office';

  isLoginPage(): boolean {
    return this.router.url === '/login';
  }
}
