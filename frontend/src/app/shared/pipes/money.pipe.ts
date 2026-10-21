import { Pipe, PipeTransform } from '@angular/core';

const USD = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 });
const HALF_CENT = 0.005;

export function formatMoney(value: number): string {
  const amount = Math.abs(value) < HALF_CENT ? 0 : value;

  return USD.format(amount);
}

@Pipe({ name: 'money' })
export class MoneyPipe implements PipeTransform {
  transform(value: number): string {
    return formatMoney(value);
  }
}
