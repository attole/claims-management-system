import { Pipe, PipeTransform } from '@angular/core';

const SPECIAL_LABELS: Readonly<Record<string, string>> = { Alae: 'ALAE' };

export function enumLabel(value: string): string {
  return SPECIAL_LABELS[value] ?? value.replace(/([a-z])([A-Z])/g, '$1 $2');
}

@Pipe({ name: 'enumLabel' })
export class EnumLabelPipe implements PipeTransform {
  transform(value: string): string {
    return enumLabel(value);
  }
}
