import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../environments/environment';

const ADMIN_PATHS = ['/api/events', '/api/reservations'];
const ADMIN_METHODS = ['POST', 'PUT', 'DELETE'];

export const adminKeyInterceptor: HttpInterceptorFn = (req, next) => {
  const needsAdminKey =
    ADMIN_METHODS.includes(req.method) &&
    ADMIN_PATHS.some(p => req.url.includes(p));

  if (needsAdminKey) {
    return next(req.clone({ setHeaders: { 'X-Admin-Key': environment.adminKey } }));
  }
  return next(req);
};
