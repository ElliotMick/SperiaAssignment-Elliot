import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ApiExpectations,
  TEST_API_BASE_URL,
  cachedSearch,
  cachedSearchesResponse,
  gifs,
  gifsResponse,
} from '../../testing/api-fakes';
import { API_BASE_URL } from '../api/api-base-url';
import { CachedSearchesStorage } from '../storage/cached-searches-storage';
import { GifStore } from './gif-store';

describe('GifStore', () => {
  let store: GifStore;
  let http: HttpTestingController;
  let api: ApiExpectations;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: TEST_API_BASE_URL },
      ],
    });
    store = TestBed.inject(GifStore);
    http = TestBed.inject(HttpTestingController);
    api = new ApiExpectations(http);
  });

  afterEach(() => http.verify());

  it('shows trending GIFs and loads the cached searches on init', () => {
    store.init();
    expect(store.loading()).toBe(true);

    api.trending().flush(gifsResponse(gifs('trending'), true));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('cats')], 5));

    expect(store.view()).toEqual({ kind: 'trending' });
    expect(store.gifs()).toEqual(gifs('trending'));
    expect(store.source()).toBe('server');
    expect(store.loading()).toBe(false);
    expect(store.cachedSearches().map((s) => s.term)).toEqual(['cats']);
    expect(store.maxEntries()).toBe(5);
  });

  it('searches with the normalized term, then resyncs the cached searches from the API', () => {
    expect(store.search('  Funny CATS ')).toBe(true);

    api.search('funny cats').flush(gifsResponse(gifs('cats'), false, 'funny cats'));
    expect(store.view()).toEqual({ kind: 'search', term: 'funny cats' });
    expect(store.gifs()).toEqual(gifs('cats'));
    expect(store.source()).toBe('giphy');
    // Remembered locally right away...
    expect(store.cachedSearches().map((s) => s.term)).toEqual(['funny cats']);

    // ...then replaced by the server's authoritative list.
    api
      .cachedSearches()
      .flush(cachedSearchesResponse([cachedSearch('funny cats'), cachedSearch('dogs')]));
    expect(store.cachedSearches().map((s) => s.term)).toEqual(['funny cats', 'dogs']);
  });

  it('shows a cached search instantly from the browser cache while refreshing it', () => {
    store.search('cats');
    api.search('cats').flush(gifsResponse(gifs('cats'), false));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('cats')]));

    store.search('cats');

    expect(store.gifs()).toEqual(cachedSearch('cats').gifs);
    expect(store.source()).toBe('browser');
    expect(store.loading()).toBe(true);

    api.search('cats').flush(gifsResponse(gifs('cats', 3), true));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('cats')]));
    expect(store.gifs()).toEqual(gifs('cats', 3));
    expect(store.source()).toBe('server');
  });

  it('keeps at most maxEntries searches in the browser cache, most recent first', () => {
    store.init();
    api.trending().flush(gifsResponse(gifs('trending'), true));
    api
      .cachedSearches()
      .flush(cachedSearchesResponse([cachedSearch('cats'), cachedSearch('dogs')], 2));

    store.search('birds');
    api.search('birds').flush(gifsResponse(gifs('birds'), false));

    expect(store.cachedSearches().map((s) => s.term)).toEqual(['birds', 'cats']);
    api
      .cachedSearches()
      .flush(cachedSearchesResponse([cachedSearch('birds'), cachedSearch('cats')], 2));
  });

  it('persists the cached searches for the browser tab', () => {
    store.init();
    api.trending().flush(gifsResponse(gifs('trending'), true));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('cats')], 7));

    expect(TestBed.inject(CachedSearchesStorage).load()).toEqual(
      cachedSearchesResponse([cachedSearch('cats')], 7),
    );
  });

  it('drops the response of a view the user already left', () => {
    store.search('cats');
    store.search('dogs');

    const staleRequest = api.search('cats');
    expect(staleRequest.cancelled).toBe(true);

    api.search('dogs').flush(gifsResponse(gifs('dogs'), false));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('dogs')]));
    expect(store.gifs()).toEqual(gifs('dogs'));
  });

  it('exposes API errors and retries on demand', () => {
    store.search('cats');
    api
      .search('cats')
      .flush(
        { title: 'The GIF provider is currently unavailable.' },
        { status: 502, statusText: 'Bad Gateway' },
      );

    expect(store.loading()).toBe(false);
    expect(store.error()).toBe('The GIF provider is currently unavailable.');

    store.retry();
    expect(store.error()).toBeNull();
    api.search('cats').flush(gifsResponse(gifs('cats'), false));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('cats')]));
    expect(store.gifs()).toEqual(gifs('cats'));
  });

  it('ignores invalid terms', () => {
    expect(store.search('   ')).toBe(false);
    expect(store.view()).toEqual({ kind: 'trending' });
  });
});
