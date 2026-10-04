import { HttpErrorResponse } from '@angular/common/http';

export interface ApiErrorMessage {
  message: string;
  validationMessages: string[];
}

interface ProblemDetailsPayload {
  errors?: Record<string, unknown>;
}

export function toApiErrorMessage(error: unknown): ApiErrorMessage {
  if (!(error instanceof HttpErrorResponse)) {
    return { message: 'Something went wrong. Please try again.', validationMessages: [] };
  }

  const validationMessages = error.status === 400 ? readValidationMessages(error.error) : [];
  const messages: Record<number, string> = {
    400: 'Some property details are invalid. Review the form and try again.',
    401: 'You are not authorized to view this information.',
    403: 'You do not have permission to manage these properties.',
    404: 'This property could not be found. It may have been removed.',
    409: 'This request conflicts with the current property data. Refresh and try again.',
    429: 'Too many requests. Wait a moment, then try again.',
    500: 'The service is temporarily unavailable. Try again later.',
  };

  const message = error.status === 0
    ? 'The property service could not be reached. Check your connection and try again.'
    : messages[error.status] ?? 'The request could not be completed. Please try again.';

  return { message, validationMessages };
}

function readValidationMessages(value: unknown): string[] {
  if (!isRecord(value) || !isRecord(value['errors'])) return [];
  const details = value as ProblemDetailsPayload;
  return Object.values(details.errors ?? {})
    .flatMap(messages => Array.isArray(messages) ? messages : [])
    .filter((message): message is string => typeof message === 'string')
    .slice(0, 5);
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}