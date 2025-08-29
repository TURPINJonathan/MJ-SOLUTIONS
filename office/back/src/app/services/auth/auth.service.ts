import { environment } from "#env/environment";
import { LoginResponse, LogoutResponse, RefreshResponse } from "#models/auth.model";
import { User } from "#models/user.model";
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

  logout(): Observable<LogoutResponse> {
    const url = `${environment.apiUrl}/auth/logout`;
    return this.http.post<LogoutResponse>(url, {}, { withCredentials: true });
  }

  refreshUser(): Observable<RefreshResponse> {
    const url = `${environment.apiUrl}/auth/refresh`;
    return this.http.post<RefreshResponse>(url, {}, { withCredentials: true });
  }

	checkSession(): Observable<User | null> {
		return this.http.get<User>(`${environment.apiUrl}/auth/check`, { withCredentials: true }).pipe(
			catchError(() => of(null))
		);
	}
}