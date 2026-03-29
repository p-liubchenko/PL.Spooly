import { Pipe, PipeTransform } from '@angular/core';

/** Converts a decimal-hours number to a human-readable duration string.
 *  Examples:  5.1667 → "5h 10m"   0.75 → "45m"   2.0 → "2h"   0 → "0m" */
@Pipe({ name: 'duration', standalone: false })
export class DurationPipe implements PipeTransform {
  transform(decimalHours: number | null | undefined): string {
    if (decimalHours == null || isNaN(decimalHours)) return '—';
    const totalMinutes = Math.round(decimalHours * 60);
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;
    if (h === 0) return `${m}m`;
    if (m === 0) return `${h}h`;
    return `${h}h ${m}m`;
  }
}
