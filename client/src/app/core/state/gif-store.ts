import { Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, Subject, catchError, switchMap, tap } from 'rxjs';
import { GifApi } from '../api/gif-api';
import { toErrorMessage } from '../api/api-error';
import { CachedSearch, Gif } from '../models/gif.models';
import { normalizeSearchTerm } from '../search/search-term';
import { CachedSearchesStorage } from '../storage/cached-searches-storage';

export type View =
  { readonly kind: 'trending' } | { readonly kind: 'search'; readonly term: string };

/** Where the GIFs on screen come from, from cheapest to most expensive. */
export type GifSource = 'browser' | 'server' | 'giphy';

interface ViewState {
  readonly gifs: readonly Gif[];
  readonly source: GifSource | null;
  readonly loading: boolean;
  readonly error: string | null;
}

const DEFAULT_MAX_ENTRIES = 10;

/**
 * Application state. GIFs are shown instantly from the browser cache when available, then refreshed from the
 * API, which stays the source of truth: the user's cached searches are re-fetched after every search.
 */
@Injectable({ providedIn: 'root' })
export class GifStore {
  private readonly api = inject(GifApi);
  private readonly storage = inject(CachedSearchesStorage);
  private readonly viewRequests = new Subject<View>();

  private readonly stored = this.storage.load();
  private readonly viewSignal = signal<View>({ kind: 'trending' });
  private readonly state = signal<ViewState>({
    gifs: [],
    source: null,
    loading: false,
    error: null,
  });
  private readonly cachedSearchesSignal = signal<readonly CachedSearch[]>(
    this.stored?.searches ?? [],
  );
  private readonly maxEntriesSignal = signal(this.stored?.maxEntries ?? DEFAULT_MAX_ENTRIES);
  private trending: readonly Gif[] | null = null;

  readonly view = this.viewSignal.asReadonly();
  readonly gifs = computed(() => this.state().gifs);
  readonly source = computed(() => this.state().source);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);
  readonly cachedSearches = this.cachedSearchesSignal.asReadonly();
  readonly maxEntries = this.maxEntriesSignal.asReadonly();

  constructor() {
    // switchMap drops the response of a view the user already left (and cancels its request).
    this.viewRequests
      .pipe(
        switchMap((view) => this.load(view)),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  init(): void {
    this.showTrending();
    this.syncCachedSearches().subscribe();
  }

  showTrending(): void {
    this.select({ kind: 'trending' });
  }

  /** @returns false when the term is invalid, in which case nothing is searched. */
  search(input: string): boolean {
    const term = normalizeSearchTerm(input);
    if (term === null) {
      return false;
    }

    this.select({ kind: 'search', term });
    return true;
  }

  retry(): void {
    this.select(this.viewSignal());
  }

  private select(view: View): void {
    this.viewSignal.set(view);
    this.viewRequests.next(view);
  }

  private load(view: View): Observable<unknown> {
    const local = this.findInBrowserCache(view);
    this.state.set({
      gifs: local ?? [],
      source: local ? 'browser' : null,
      loading: true,
      error: null,
    });

    const request = view.kind === 'trending' ? this.api.getTrending() : this.api.search(view.term);
    return request.pipe(
      tap((response) => {
        this.rememberInBrowserCache(view, response.gifs);
        this.state.set({
          gifs: response.gifs,
          source: response.fromCache ? 'server' : 'giphy',
          loading: false,
          error: null,
        });
      }),
      // Keep showing what we have (possibly the browser cache) next to the error.
      catchError((error: unknown) => {
        this.state.update((state) => ({ ...state, loading: false, error: toErrorMessage(error) }));
        return EMPTY;
      }),
      // A search changes the user's cached searches on the server: fetch the authoritative list.
      switchMap(() => (view.kind === 'search' ? this.syncCachedSearches() : EMPTY)),
    );
  }

  private syncCachedSearches(): Observable<unknown> {
    return this.api.getCachedSearches().pipe(
      tap((response) => {
        this.cachedSearchesSignal.set(response.searches);
        this.maxEntriesSignal.set(response.maxEntries);
        this.persist();
      }),
      // Keep the local copy; the next sync catches up.
      catchError(() => EMPTY),
    );
  }

  private findInBrowserCache(view: View): readonly Gif[] | null {
    return view.kind === 'trending'
      ? this.trending
      : (this.cachedSearchesSignal().find((search) => search.term === view.term)?.gifs ?? null);
  }

  /** Mirrors the server's eviction (most recent first, capped) until the next sync confirms it. */
  private rememberInBrowserCache(view: View, gifs: readonly Gif[]): void {
    if (view.kind === 'trending') {
      this.trending = gifs;
      return;
    }

    const search: CachedSearch = {
      term: view.term,
      lastSearchedAtUtc: new Date().toISOString(),
      gifs,
    };
    this.cachedSearchesSignal.update((searches) =>
      [search, ...searches.filter((s) => s.term !== view.term)].slice(0, this.maxEntriesSignal()),
    );
    this.persist();
  }

  private persist(): void {
    this.storage.save({
      maxEntries: this.maxEntriesSignal(),
      searches: this.cachedSearchesSignal(),
    });
  }
}
