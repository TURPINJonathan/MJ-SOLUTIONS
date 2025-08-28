import { environment } from "#env/environment";
import { LoginResponse, LogoutResponse, RefreshResponse } from "#models/auth.model";
import { isValidEmail, isValidPassword } from "#shared/utils/validation.utils";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { catchError, map, Observable, of, throwError } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  constructor(
		private http: HttpClient
	) {}

  login(email: string, password: string): Observable<LoginResponse> {
    if (!isValidEmail(email)) {
      return throwError(() => new Error('Invalid email format'));
    }
    if (!isValidPassword(password)) {
      return throwError(() => new Error('Invalid password format'));
    }
    const url = `${environment.apiUrl}/auth/login`;
    return this.http.post<LoginResponse>(url, { email, password }, { withCredentials: true });
  }

	logout(refreshToken: string | null): Observable<LogoutResponse> {
		if (!refreshToken) {
			return throwError(() => new Error('No refresh token provided'));
		}
		const url = `${environment.apiUrl}/auth/logout`;
		return this.http.post<LogoutResponse>(url, {refreshToken : refreshToken});
	}

	refreshUser(refreshToken: string): Observable<RefreshResponse> {
		return this.http.post<RefreshResponse>(`${environment.apiUrl}/auth/refresh-token`, { refreshToken });
	}

	checkSession(): Observable<boolean> {
		return this.http.get(`${environment.apiUrl}/auth/check`, { withCredentials: true }).pipe(
			map(() => true),
			catchError(() => of(false))
		);
	}

}