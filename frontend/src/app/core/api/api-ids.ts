export const EMPTY_ID = '00000000-0000-0000-0000-000000000000';

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function isGuid(value: string): boolean {
  return GUID.test(value);
}

export function guidOrEmpty(value: string): string {
  return isGuid(value) ? value : '';
}
