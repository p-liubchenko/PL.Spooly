import { Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-duration-input',
  standalone: false,
  templateUrl: './duration-input.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DurationInputComponent),
      multi: true,
    },
  ],
})
export class DurationInputComponent implements ControlValueAccessor {
  hours = 0;
  minutes = 0;
  isDisabled = false;

  private onChange: (v: number) => void = () => {};
  private onTouched: () => void = () => {};

  get decimalHours(): number {
    return this.hours + this.minutes / 60;
  }

  // ControlValueAccessor — called by Angular when the bound model changes
  writeValue(val: number): void {
    if (val == null || isNaN(val) || val < 0) {
      this.hours = 0;
      this.minutes = 0;
      return;
    }
    this.hours = Math.floor(val);
    this.minutes = Math.round((val - this.hours) * 60);
    // Fix floating-point edge cases (e.g. 59.9999 → 60)
    if (this.minutes >= 60) {
      this.hours += Math.floor(this.minutes / 60);
      this.minutes = this.minutes % 60;
    }
  }

  registerOnChange(fn: (v: number) => void): void { this.onChange = fn; }
  registerOnTouched(fn: () => void): void { this.onTouched = fn; }
  setDisabledState(disabled: boolean): void { this.isDisabled = disabled; }

  onHoursInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value;
    this.hours = Math.max(0, Math.floor(parseFloat(raw) || 0));
    this.emit();
  }

  onMinutesInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value;
    let m = Math.round(parseFloat(raw) || 0);
    if (m < 0) m = 0;
    // Allow overflow — typing "90" becomes 1h 30m
    if (m >= 60) {
      this.hours += Math.floor(m / 60);
      m = m % 60;
    }
    this.minutes = m;
    this.emit();
  }

  private emit(): void {
    this.onChange(this.decimalHours);
    this.onTouched();
  }
}
