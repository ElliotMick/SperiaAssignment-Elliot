import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  linkedSignal,
  output,
} from '@angular/core';
import { SEARCH_TERM_MAX_LENGTH, normalizeSearchTerm } from '../../core/search/search-term';

@Component({
  selector: 'app-search-bar',
  templateUrl: './search-bar.html',
  styleUrl: './search-bar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchBar {
  /** The term currently displayed, mirrored in the input (e.g. when a recent search is picked). */
  readonly term = input('');
  readonly searched = output<string>();

  protected readonly value = linkedSignal(() => this.term());
  protected readonly isValid = computed(() => normalizeSearchTerm(this.value()) !== null);
  protected readonly maxLength = SEARCH_TERM_MAX_LENGTH;

  protected submit(event: Event): void {
    event.preventDefault();
    if (this.isValid()) {
      this.searched.emit(this.value());
    }
  }
}
