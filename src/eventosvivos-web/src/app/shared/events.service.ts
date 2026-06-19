import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateEventRequest, EventModel, OccupancyReport } from './models';

@Injectable({ providedIn: 'root' })
export class EventsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/events`;

  getEvents(filter?: { type?: string; venueId?: number; state?: string; title?: string }): Observable<EventModel[]> {
    let params = new HttpParams();
    if (filter?.type) params = params.set('type', filter.type);
    if (filter?.venueId) params = params.set('venueId', filter.venueId.toString());
    if (filter?.state) params = params.set('state', filter.state);
    if (filter?.title) params = params.set('title', filter.title);
    return this.http.get<EventModel[]>(this.base, { params });
  }

  createEvent(req: CreateEventRequest): Observable<EventModel> {
    return this.http.post<EventModel>(this.base, req);
  }

  getOccupancyReport(eventId: number): Observable<OccupancyReport> {
    return this.http.get<OccupancyReport>(`${this.base}/${eventId}/occupancy`);
  }
}
