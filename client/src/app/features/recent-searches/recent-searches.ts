import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CachedSearch } from '../../core/models/gif.models';
import { View } from '../../core/state/gif-store';

/** Trending plus the user's cached searches, as quick filters. */
@Component({
  selector: 'app-recent-searches',
  imports: [DatePipe],
  templateUrl: './recent-searches.html',
  styleUrl: './recent-searches.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecentSearches {
  readonly searches = input.required<readonly CachedSearch[]>();
  readonly maxEntries = input.required<number>();
  readonly activeView = input.required<View>();

  readonly trendingSelected = output<void>();
  readonly searchSelected = output<string>();

  protected isActive(term: string): boolean {
    const view = this.activeView();
    return view.kind === 'search' && view.term === term;
  }
}
