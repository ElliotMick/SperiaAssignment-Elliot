import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CachedSearchesResponse, GifsResponse } from '../models/gif.models';
import { API_BASE_URL } from './api-base-url';

/** Thin HTTP client of the Giphy Integration API. */
@Injectable({ providedIn: 'root' })
export class GifApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(API_BASE_URL)}/api/gifs`;

  getTrending(): Observable<GifsResponse> {
    return this.http.get<GifsResponse>(`${this.baseUrl}/trending`);
  }

  search(term: string): Observable<GifsResponse> {
    return this.http.get<GifsResponse>(`${this.baseUrl}/search`, { params: { q: term } });
  }

  getCachedSearches(): Observable<CachedSearchesResponse> {
    return this.http.get<CachedSearchesResponse>(`${this.baseUrl}/searches`);
  }
}
