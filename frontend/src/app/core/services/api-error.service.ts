import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ApiErrorService {
  getMessage(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    if (error.status === 0) {
      return 'The API is unavailable. Check your connection and try again.';
    }

    const body = error.error;
    if (!isRecord(body)) {
      return fallback;
    }

    const validationMessages = readValidationMessages(body['errors']);
    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }

    const message = body['message'];
    if (typeof message === 'string' && message.trim()) {
      return message;
    }

    const detail = body['detail'];
    if (typeof detail === 'string' && detail.trim()) {
      return detail;
    }

    const title = body['title'];
    return typeof title === 'string' && title.trim() ? title : fallback;
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function readValidationMessages(value: unknown): string[] {
  if (!isRecord(value)) {
    return [];
  }

  return Object.values(value).flatMap((messages) =>
    Array.isArray(messages)
      ? messages.filter((message): message is string => typeof message === 'string')
      : [],
  );
}
