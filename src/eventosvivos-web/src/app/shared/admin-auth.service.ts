import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AdminAuthService {
  private static readonly STORAGE_KEY = 'ev_admin_key';

  readonly key = signal(sessionStorage.getItem(AdminAuthService.STORAGE_KEY) ?? '');

  setKey(value: string): void {
    const trimmed = value.trim();
    this.key.set(trimmed);
    if (trimmed) sessionStorage.setItem(AdminAuthService.STORAGE_KEY, trimmed);
    else sessionStorage.removeItem(AdminAuthService.STORAGE_KEY);
  }

  getKey(): string | null {
    const k = this.key();
    return k || null;
  }
}
