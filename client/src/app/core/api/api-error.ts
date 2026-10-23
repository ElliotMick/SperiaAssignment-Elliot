import { HttpErrorResponse } from '@angular/common/http';

/** RFC 9457 problem details, as returned by the API on errors. */
interface ProblemDetails {
  readonly title?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}

/** Turns a failed API call into a message fit for the user. */
export function toErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong.';
  }

  if (error.status === 0) {
    return 'The API cannot be reached. Is it running?';
  }

  const problem = error.error as ProblemDetails | null;
  const validationError = problem?.errors ? Object.values(problem.errors).flat()[0] : undefined;
  return validationError ?? problem?.title ?? `The request failed (${error.status}).`;
}
