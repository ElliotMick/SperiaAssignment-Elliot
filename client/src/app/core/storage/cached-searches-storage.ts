import { Injectable } from '@angular/core';
import { CachedSearchesResponse } from '../models/gif.models';

const STORAGE_KEY = 'giphy-client.cached-searches.v1';

/**
 * Keeps the user's cached searches for the lifetime of the browser tab, so they show instantly on reload
 * while the API is asked for the authoritative list.
 */
@Injectable({ providedIn: 'root' })
export class CachedSearchesStorage {
  load(): CachedSearchesResponse | null {
    try {
      const stored = sessionStorage.getItem(STORAGE_KEY);
      return stored ? (JSON.parse(stored) as CachedSearchesResponse) : null;
    } catch {
      // Storage unavailable (privacy settings) or corrupted: start empty, the API sync fills it.
      return null;
    }
  }

  save(value: CachedSearchesResponse): void {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(value));
    } catch {
      // Best effort only: the API stays the source of truth.
    }
  }
}
