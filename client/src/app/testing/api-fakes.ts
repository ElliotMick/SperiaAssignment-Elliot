import { HttpTestingController } from '@angular/common/http/testing';
import { CachedSearch, CachedSearchesResponse, Gif, GifsResponse } from '../core/models/gif.models';

export const TEST_API_BASE_URL = 'http://api.test';

export function gifs(prefix: string, count = 2): Gif[] {
  return Array.from({ length: count }, (_, index) => ({
    id: `${prefix}-${index}`,
    title: `${prefix} ${index}`,
    url: `https://media.giphy.com/${prefix}-${index}.gif`,
    previewUrl: `https://media.giphy.com/${prefix}-${index}-preview.gif`,
  }));
}

export function gifsResponse(
  items: Gif[],
  fromCache: boolean,
  query: string | null = null,
): GifsResponse {
  return { query, fromCache, count: items.length, gifs: items };
}

export function cachedSearch(term: string): CachedSearch {
  return { term, lastSearchedAtUtc: '2026-10-23T12:00:00Z', gifs: gifs(term) };
}

export function cachedSearchesResponse(
  searches: CachedSearch[],
  maxEntries = 10,
): CachedSearchesResponse {
  return { maxEntries, searches };
}

/** Expectations on the API endpoints, against Angular's HTTP testing backend. */
export class ApiExpectations {
  constructor(private readonly http: HttpTestingController) {}

  trending() {
    return this.http.expectOne(`${TEST_API_BASE_URL}/api/gifs/trending`);
  }

  search(term: string) {
    return this.http.expectOne(
      (request) =>
        request.url === `${TEST_API_BASE_URL}/api/gifs/search` && request.params.get('q') === term,
    );
  }

  cachedSearches() {
    return this.http.expectOne(`${TEST_API_BASE_URL}/api/gifs/searches`);
  }
}
