// Contracts of the Giphy Integration API.

export interface Gif {
  readonly id: string;
  readonly title: string;
  /** The original GIF. */
  readonly url: string;
  /** A lighter rendition, suited for grids. */
  readonly previewUrl: string;
}

export interface GifsResponse {
  /** The normalized search term, or null for trending GIFs. */
  readonly query: string | null;
  /** Whether the API served the GIFs without calling Giphy. */
  readonly fromCache: boolean;
  readonly count: number;
  readonly gifs: readonly Gif[];
}

export interface CachedSearch {
  readonly term: string;
  readonly lastSearchedAtUtc: string;
  readonly gifs: readonly Gif[];
}

export interface CachedSearchesResponse {
  /** How many searches the API keeps per user. */
  readonly maxEntries: number;
  /** Most recent first. */
  readonly searches: readonly CachedSearch[];
}
