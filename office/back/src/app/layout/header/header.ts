import { MenuItem } from '#models/menu-item.model';
import { User } from '#models/user.model';
import { AuthService } from '#services/auth/auth.service';
import { selectUser } from '#store/user/user.selectors';
import { SwitchModeComponent } from '#ui/switch-mode/switch-mode';
import { ToastUtils } from '#utils/toast.utils';
import { StringUtils } from "#utils/string.utils"
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-header',
  imports: [SwitchModeComponent],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class HeaderLayout {
  menus: MenuItem[] = [];
  user: User | null = null;

  userActions = [
    { label: 'Profil', action: 'profile', icon: 'person' },
    { label: 'Déconnexion', action: 'logout', icon: 'logout' }
  ];

  subOpen: Record<string, boolean> = {};
  closeTimeouts: Record<string, any> = {};

	constructor(
		private router: Router,
		private authService: AuthService,
		private toast: ToastUtils,
		private stringUtils: StringUtils,
		private store: Store
	) {
		this.menus = this.router.config
			.filter(r => r.data && r.data['menu'] && typeof r.path === 'string')
			.map(r => {
				if (!r.data) r.data = {};
				const base = {
					label: String(r.data['label']),
					key: r.path as string,
					route: '/' + r.path,
					icon: String(r.data['icon'])
				};
				if (Array.isArray(r.data['submenu'])) {
					return {
						...base,
						submenu: r.data['submenu'].map((sub: any) => ({
							label: String(sub.label),
							key: String(sub.path),
							route: `/${r.path}/${sub.path}`,
							icon: String(sub.icon)
						}))
					};
				}
				return base;
			});
			
		this.store.select(selectUser).subscribe(user => {
      this.user = user;
    });
	}

	getUserInitials(): string {
		if (!this.user) return '';
		return this.stringUtils.firstLetter(this.user.firstname).toUpperCase() + this.stringUtils.firstLetter(this.user.lastname).toUpperCase();
	}

  openSub(key: string) {
    clearTimeout(this.closeTimeouts[key]);
    this.subOpen[key] = true;
  }

  closeSub(key: string) {
    this.closeTimeouts[key] = setTimeout(() => {
      this.subOpen[key] = false;
    }, 180);
  }

  toggleSub(key: string) {
    clearTimeout(this.closeTimeouts[key]);
    this.subOpen[key] = !this.subOpen[key];
  }

  handleMenuClick(menu: any, event: Event) {
    if (menu.submenu) {
      this.toggleSub(menu.key);
      event.preventDefault();
    } else if (menu.route) {
      this.router.navigate([menu.route]);
      event.preventDefault();
    }
  }

  handleSubmenuClick(sub: any, event: Event) {
    if (sub.route) {
      this.router.navigate([sub.route]);
      event.preventDefault();
    }
  }

  handleUserAction(action: string, event: Event) {
    event.preventDefault();
    if (action === 'profile') {
      this.router.navigate(['/profile']);
    }
    if (action === 'logout') {
			this.authService.logout().subscribe({
				next: () => {
					this.toast.success('Déconnexion réussie', 'À bientôt');
					this.router.navigate(['/login']);
				},
				error: err => {
					console.error('Erreur lors de la déconnexion', err);
					this.toast.error('Erreur lors de la déconnexion');
				}
			});
    }
  }
}