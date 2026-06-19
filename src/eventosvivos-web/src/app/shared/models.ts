export interface Venue {
  id: number;
  name: string;
  capacity: number;
  city: string;
}

export interface EventModel {
  id: number;
  title: string;
  description: string;
  venueId: number;
  venueName?: string;
  venueCity?: string;
  maxCapacity: number;
  startsAt: string;
  endsAt: string;
  ticketPrice: number;
  type: string;
  effectiveState: string;
}

export interface Reservation {
  id: number;
  eventId: number;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
  state: string;
  reservationCode?: string;
  createdAt: string;
  confirmedAt?: string;
  canceledAt?: string;
  lostCapacity: boolean;
}

export interface OccupancyReport {
  eventId: number;
  eventTitle: string;
  eventState: string;
  confirmedTicketsSold: number;
  pendingTicketsHeld: number;
  lostTickets: number;
  remainingTickets: number;
  occupancyPercentage: number;
  totalIncome: number;
}

export interface CreateEventRequest {
  title: string;
  description: string;
  venueId: number;
  maxCapacity: number;
  startsAt: string;
  endsAt: string;
  ticketPrice: number;
  type: string;
}

export interface CreateReservationRequest {
  eventId: number;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
}
