import { Routes } from '@angular/router';
import { LoginPage } from '#pages/login/login';
import { DashboardPage } from '#pages/dashboard/dashboard';

export const routes: Routes = [
	{ path: 'login', component: LoginPage },
	{ path: 'dashboard', component: DashboardPage }
];
