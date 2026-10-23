import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Gif } from '../../core/models/gif.models';

const PLACEHOLDER_COUNT = 12;

/** Responsive grid of GIF previews, each opening the original GIF. */
@Component({
  selector: 'app-gif-grid',
  templateUrl: './gif-grid.html',
  styleUrl: './gif-grid.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GifGrid {
  readonly gifs = input.required<readonly Gif[]>();
  readonly loading = input(false);

  protected readonly placeholders = Array.from({ length: PLACEHOLDER_COUNT }, (_, index) => index);
}
