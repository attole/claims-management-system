import { errorMessage } from './error-message.pipe';

describe('errorMessage (S:UI-Intake-Steps, S:API-ErrorShape)', () => {
  it('turns each known error into one readable message', () => {
    const results = [
      errorMessage({ required: true }),
      errorMessage({ maxlength: { requiredLength: 50, actualLength: 60 } }),
      errorMessage({ minTrimmedLength: { requiredLength: 20, actualLength: 3 } }),
      errorMessage({ notInFuture: true }),
    ];

    expect(results).toEqual([
      'This field is required.',
      'Use at most 50 characters.',
      'Enter at least 20 characters.',
      'The loss date cannot be in the future.',
    ]);
  });

  it('shows a server message as the API sent it', () => {
    const result = errorMessage({ server: 'Loss description is required.' });

    expect(result).toBe('Loss description is required.');
  });

  it('gives nothing without errors and a general message for an unknown error', () => {
    const results = [errorMessage(null), errorMessage({ odd: true })];

    expect(results).toEqual(['', 'This value is not valid.']);
  });
});
