import { Pipe, PipeTransform } from '@angular/core';

const UNITS = ['B', 'KB', 'MB', 'GB'] as const;
const STEP = 1024;

export function formatFileSize(bytes: number): string {
  const exponent = bytes > 0 ? Math.min(Math.floor(Math.log(bytes) / Math.log(STEP)), UNITS.length - 1) : 0;
  const value = bytes / STEP ** exponent;
  const digits = exponent === 0 ? 0 : 1;

  return `${value.toFixed(digits)} ${UNITS[exponent]}`;
}

@Pipe({ name: 'fileSize' })
export class FileSizePipe implements PipeTransform {
  transform(bytes: number): string {
    return formatFileSize(bytes);
  }
}
