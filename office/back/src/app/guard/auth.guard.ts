import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { AuthService } from '#services/auth/auth.service';
import { Store } from '@ngrx/store';
import { setUser, clearUser } from '#store/user/user.actions';
import { User } from '#models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private store: Store
  ) {}

  canActivate(): Observable<boolean | UrlTree> {
    return this.authService.checkSession().pipe(
      tap((user: User | null) => {
        if (user) {
          this.store.dispatch(setUser({ user }));
        } else {
          this.store.dispatch(clearUser());
        }
      }),
      map((user: User | null) => user ? true : this.router.parseUrl('/login')),
      catchError(() => of(this.router.parseUrl('/login')))
    );
  }
}