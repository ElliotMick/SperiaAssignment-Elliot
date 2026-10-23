import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { GifSource, GifStore } from './core/state/gif-store';
import { GifGrid } from './features/gif-grid/gif-grid';
import { RecentSearches } from './features/recent-searches/recent-searches';
import { SearchBar } from './features/search-bar/search-bar';

const SOURCE_LABELS: Readonly<Record<GifSource, string>> = {
  browser: 'From your browser cache',
  server: 'From the server cache',
  giphy: 'Fresh from Giphy',
};

@Component({
  selector: 'app-root',
  imports: [SearchBar, RecentSearches, GifGrid],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly store = inject(GifStore);
  protected readonly sourceLabels = SOURCE_LABELS;

  protected readonly activeTerm = computed(() => {
    const view = this.store.view();
    return view.kind === 'search' ? view.term : '';
  });

  protected readonly heading = computed(() =>
    this.activeTerm() ? `Results for “${this.activeTerm()}”` : 'Trending today',
  );

  protected readonly isEmpty = computed(
    () => !this.store.loading() && !this.store.error() && this.store.gifs().length === 0,
  );

  constructor() {
    this.store.init();
  }
}
