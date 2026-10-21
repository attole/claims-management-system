import { environment } from '../../../environments/environment';
import { apiUrl, isApiUrl } from './api-url';

describe('apiUrl (A:UI-General)', () => {
  it('joins the base URL and the path with one slash', () => {
    const url = apiUrl('/claims');

    expect(url).toBe(`${environment.apiBaseUrl}/claims`);
  });

  it('isApiUrl is true only for URLs under the base', () => {
    const inside = isApiUrl(apiUrl('claims'));
    const outside = isApiUrl('https://example.com/claims');

    expect(inside).toBe(true);
    expect(outside).toBe(false);
  });
});
