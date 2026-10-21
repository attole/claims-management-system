import { environment } from '../../../environments/environment';

export function apiUrl(path: string): string {
  return `${environment.apiBaseUrl}/${path.replace(/^\/+/, '')}`;
}

export function isApiUrl(url: string): boolean {
  return url.startsWith(environment.apiBaseUrl);
}
