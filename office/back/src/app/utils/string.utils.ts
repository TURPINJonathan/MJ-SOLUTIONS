import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class StringUtils {

  constructor() {}

  firstLetter(string: string): string {
    if (!string) return '';
    return string.charAt(0);
  }
}