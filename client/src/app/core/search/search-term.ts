/** Maximum length accepted by the API (Giphy's limit). */
export const SEARCH_TERM_MAX_LENGTH = 50;

/**
 * Normalizes a search term the way the API does (trimmed, single spaces, lower case), so the client finds
 * its cached searches under the same key as the server.
 *
 * @returns The normalized term, or null when it is blank or too long.
 */
export function normalizeSearchTerm(input: string): string | null {
  const normalized = input.trim().split(/\s+/).join(' ').toLowerCase();
  return normalized.length > 0 && normalized.length <= SEARCH_TERM_MAX_LENGTH ? normalized : null;
}
