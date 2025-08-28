import { Routes } from '@angular/router';
import { LoginPage } from '#pages/login/login';
import { DashboardPage } from '#pages/dashboard/dashboard';
import { AuthGuard } from '#guard/auth.guard';
import { ProjectPage } from '#pages/project/project';

export const routes: Routes = [
  { path: 'login', component: LoginPage },
  {
    path: 'dashboard',
    component: DashboardPage,
    canActivate: [AuthGuard],
    data: { label: 'Dashboard', icon: 'home', menu: true }
  },
	{
		path: 'project',
		component: ProjectPage,
		canActivate: [AuthGuard],
		data: { label: 'Projets', icon: 'folder', menu: true }
	}
  // {
  //   path: 'projets',
  //   data: {
  //     label: 'Projets',
  //     icon: 'folder',
  //     menu: true,
  //     submenu: [
  //       { path: 'all', label: 'Tous les projets', icon: 'list' },
  //       { path: 'new', label: 'Nouveau projet', icon: 'add_circle' },
  //       { path: 'archives', label: 'Archives', icon: 'archive' }
  //     ]
  //   },
  //   children: [
  //     { path: 'all', component: /* ... */, data: { label: 'Tous les projets', icon: 'list' } },
  //     { path: 'new', component: /* ... */, data: { label: 'Nouveau projet', icon: 'add_circle' } },
  //     { path: 'archives', component: /* ... */, data: { label: 'Archives', icon: 'archive' } }
  //   ]
  // },
  // ...
];