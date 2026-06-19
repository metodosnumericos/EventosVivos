import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Venue } from './models';

@Injectable({ providedIn: 'root' })
export class VenuesService {
  private readonly http = inject(HttpClient);

  getVenues(): Observable<Venue[]> {
    return this.http.get<Venue[]>(`${environment.apiUrl}/api/venues`);
  }
}
