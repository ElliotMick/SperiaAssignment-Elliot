import { SEARCH_TERM_MAX_LENGTH, normalizeSearchTerm } from './search-term';

describe('normalizeSearchTerm', () => {
  it('normalizes like the API', () => {
    expect(normalizeSearchTerm('cats')).toBe('cats');
    expect(normalizeSearchTerm('  Funny   CATS ')).toBe('funny cats');
    expect(normalizeSearchTerm('dogs\tand\ncats')).toBe('dogs and cats');
  });

  it('rejects blank terms', () => {
    expect(normalizeSearchTerm('')).toBeNull();
    expect(normalizeSearchTerm('   ')).toBeNull();
  });

  it('accepts terms up to the maximum length', () => {
    expect(normalizeSearchTerm('a'.repeat(SEARCH_TERM_MAX_LENGTH))).not.toBeNull();
    expect(normalizeSearchTerm('a'.repeat(SEARCH_TERM_MAX_LENGTH + 1))).toBeNull();
  });
});
