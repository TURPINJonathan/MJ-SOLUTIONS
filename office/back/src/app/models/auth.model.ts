export interface LoginResponse {
  message: string;
}

export interface LogoutResponse {
  message: string;
}

export interface RefreshResponse {
  message: string;
}

export interface AuthSession {
  Id: number;
  Email: string;
  Role: string;
  Permissions: string[];
}