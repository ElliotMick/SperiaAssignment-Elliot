import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { App } from './app';
import { API_BASE_URL } from './core/api/api-base-url';
import {
  ApiExpectations,
  TEST_API_BASE_URL,
  cachedSearch,
  cachedSearchesResponse,
  gifs,
  gifsResponse,
} from './testing/api-fakes';

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let element: HTMLElement;
  let http: HttpTestingController;
  let api: ApiExpectations;

  beforeEach(async () => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: TEST_API_BASE_URL },
      ],
    });
    http = TestBed.inject(HttpTestingController);
    api = new ApiExpectations(http);

    fixture = TestBed.createComponent(App);
    element = fixture.nativeElement as HTMLElement;
    api.trending().flush(gifsResponse(gifs('trending', 3), true));
    api
      .cachedSearches()
      .flush(cachedSearchesResponse([cachedSearch('cats'), cachedSearch('dogs')]));
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  it('shows the trending GIFs and the recent searches', () => {
    expect(element.querySelector('h1')?.textContent).toContain('Trending today');
    expect(element.querySelectorAll('app-gif-grid img')).toHaveLength(3);
    expect(element.querySelector('.badge')?.textContent).toContain('From the server cache');

    const chips = Array.from(element.querySelectorAll('.chip'), (chip) => chip.textContent?.trim());
    expect(chips).toEqual(['Trending', 'cats', 'dogs']);
    expect(element.querySelector('.hint')?.textContent).toContain('2 / 10');
  });

  it('opens a recent search from the browser cache, then refreshes it from the API', async () => {
    const catsChip = Array.from(element.querySelectorAll<HTMLButtonElement>('.chip')).find(
      (chip) => chip.textContent?.trim() === 'cats',
    );
    catsChip?.click();
    await fixture.whenStable();

    expect(element.querySelector('h1')?.textContent).toContain('Results for “cats”');
    expect(element.querySelector('.badge')?.textContent).toContain('From your browser cache');
    expect(element.querySelector<HTMLInputElement>('#search-input')?.value).toBe('cats');

    api.search('cats').flush(gifsResponse(gifs('cats', 4), true, 'cats'));
    api
      .cachedSearches()
      .flush(cachedSearchesResponse([cachedSearch('cats'), cachedSearch('dogs')]));
    await fixture.whenStable();

    expect(element.querySelectorAll('app-gif-grid img')).toHaveLength(4);
    expect(element.querySelector('.badge')?.textContent).toContain('From the server cache');
  });

  it('searches the typed term', async () => {
    const input = element.querySelector<HTMLInputElement>('#search-input')!;
    input.value = 'Otters';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();

    api.search('otters').flush(gifsResponse([], false, 'otters'));
    api.cachedSearches().flush(cachedSearchesResponse([cachedSearch('otters')]));
    await fixture.whenStable();

    expect(element.querySelector('.empty')?.textContent).toContain('No GIFs found for “otters”');
  });
});
