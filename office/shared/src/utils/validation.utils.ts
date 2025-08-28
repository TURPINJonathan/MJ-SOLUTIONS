
// import { jwtDecode, JwtPayload } from 'jwt-decode';

export function isValidEmail(email: string): boolean {
  const emailRegex =
    /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;
  return emailRegex.test(email);
}

export function isValidPassword(password: string): boolean {
  const passwordRegex = /^(?=.*\d)(?=.*[!@#$%^&*()_+\-=[{}\];':"\\|,.<>/?]).{8,}$/;
  return passwordRegex.test(password);
}

export function isValidWebsite(url: string): boolean {
  return /^(https?:\/\/)?([\w-]+\.)+[\w-]{2,}(\/\S*)?$/.test(url);
}