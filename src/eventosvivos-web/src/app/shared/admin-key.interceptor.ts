import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AdminAuthService } from './admin-auth.service';

const ADMIN_PATHS = ['/api/events', '/api/reservations'];

export const adminKeyInterceptor: HttpInterceptorFn = (req, next) => {
  const key = inject(AdminAuthService).getKey();
  if (!key) return next(req);

  const isAdminPath = ADMIN_PATHS.some(p => req.url.includes(p));
  if (!isAdminPath) return next(req);

  return next(req.clone({ setHeaders: { 'X-Admin-Key': key } }));
};
