import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

/** Allows any component to open the global quick-record modal. */
@Injectable({ providedIn: 'root' })
export class QuickRecordService {
  private readonly _open = new Subject<void>();
  readonly open$ = this._open.asObservable();

  open(): void { this._open.next(); }
}
