import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { QuickRecordService } from './services/quick-record.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css',
})
export class App {
  constructor(
    public auth: AuthService,
    private router: Router,
    private quickRecord: QuickRecordService,
  ) {}

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  openQuickRecord(): void {
    this.quickRecord.open();
  }

  onPrintSaved(): void {
    // Reload the current page data if the user is on a page that shows transactions/materials
    const url = this.router.url;
    if (url.startsWith('/transactions') || url.startsWith('/materials') || url === '/') {
      this.router.navigateByUrl(url, { skipLocationChange: true })
        .then(() => this.router.navigate([url]));
    }
  }
}
