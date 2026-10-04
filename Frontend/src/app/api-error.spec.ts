import { HttpErrorResponse } from '@angular/common/http';
import { toApiErrorMessage } from './api-error';

describe('toApiErrorMessage', () => {
  it('uses safe validation messages from a ProblemDetails response', () => {
    const result = toApiErrorMessage(new HttpErrorResponse({
      status: 400,
      error: { title: 'Validation failed', errors: { Title: ['The title field is required.'] } },
    }));

    expect(result.message).toContain('invalid');
    expect(result.validationMessages).toEqual(['The title field is required.']);
  });

  it('does not expose server exception details for a 500 response', () => {
    const result = toApiErrorMessage(new HttpErrorResponse({
      status: 500,
      error: { detail: 'System.InvalidOperationException: secret stack trace' },
    }));

    expect(result.message).toContain('temporarily unavailable');
    expect(result.message).not.toContain('secret stack trace');
  });

  it.each([
    [401, 'authorized'],
    [403, 'permission'],
    [404, 'found'],
    [409, 'conflicts'],
    [429, 'Too many requests'],
  ])('maps HTTP %i to a human-readable message', (status, expected) => {
    const result = toApiErrorMessage(new HttpErrorResponse({ status }));
    expect(result.message).toContain(expected);
  });
});