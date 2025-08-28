import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from '#services/auth/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(
		private authService: AuthService,
		private router: Router
	) {}

  canActivate(): Observable<boolean | UrlTree> {
    return this.authService.checkSession().pipe(
      map((isAuthenticated) => isAuthenticated ? true : this.router.parseUrl('/login')),
      catchError(() => of(this.router.parseUrl('/login')))
    );
  }
}