import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateReservationRequest, Reservation } from './models';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/reservations`;
  private readonly adminHeaders = { 'X-Admin-Key': environment.adminKey };

  getReservations(eventId: number, state?: string): Observable<Reservation[]> {
    let params = new HttpParams().set('eventId', eventId.toString());
    if (state) params = params.set('state', state);
    return this.http.get<Reservation[]>(this.base, {
      params,
      headers: this.adminHeaders
    });
  }

  createReservation(req: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(this.base, req);
  }

  confirmPayment(reservationId: number): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.base}/${reservationId}/confirm`, null,
      { headers: this.adminHeaders }
    );
  }

  adminCancel(reservationId: number): Observable<void> {
    return this.http.post<void>(
      `${this.base}/${reservationId}/cancel`, null,
      { headers: this.adminHeaders }
    );
  }

  buyerCancel(reservationId: number, buyerEmail: string, reservationCode?: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/${reservationId}/buyer-cancel`,
      { buyerEmail, reservationCode }
    );
  }
}
