import { Component, HostListener, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { TransactionsService } from '../../services/transactions.service';
import { MaterialsService } from '../../services/materials.service';
import { PrintersService } from '../../services/printers.service';
import { SettingsService } from '../../services/settings.service';
import { QuickRecordService } from '../../services/quick-record.service';
import {
  PrintTransaction, StockTransaction, FilamentMaterial, AppSettings, Printer,
  PrintTransactionStatus, STOCK_TRANSACTION_TYPE_LABELS, StockTransactionType,
} from '../../models';
import { AuthService } from '../../services/auth.service';

export interface CombinedEntry {
  date: string;
  kind: 'print' | 'stock';
  print?: PrintTransaction;
  stock?: StockTransaction;
}

@Component({
  selector: 'app-transactions',
  standalone: false,
  templateUrl: './transactions.component.html',
})
export class TransactionsComponent implements OnInit {
  prints: PrintTransaction[] = [];
  stock: StockTransaction[] = [];
  materials: FilamentMaterial[] = [];
  printers: Printer[] = [];
  settings: AppSettings | null = null;
  tab: 'all' | 'prints' | 'stock' = 'all';
  loading = true;
  error = '';

  detailEntry: CombinedEntry | null = null;

  PrintTransactionStatus = PrintTransactionStatus;
  stockTypeLabel = (t: StockTransactionType) => STOCK_TRANSACTION_TYPE_LABELS[t] ?? String(t);

  constructor(
    private transactionsService: TransactionsService,
    private materialsService: MaterialsService,
    private printersService: PrintersService,
    private settingsService: SettingsService,
    private quickRecord: QuickRecordService,
    public auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    forkJoin({
      prints: this.transactionsService.getPrints(),
      stock: this.transactionsService.getStock(),
      materials: this.materialsService.getAll(),
      printers: this.printersService.getAll(),
      settings: this.settingsService.get(),
    }).subscribe({
      next: res => {
        this.prints = res.prints.sort((a, b) => b.createdAt.localeCompare(a.createdAt));
        this.stock = res.stock.sort((a, b) => b.createdAt.localeCompare(a.createdAt));
        this.materials = res.materials;
        this.printers = res.printers;
        this.settings = res.settings;
        this.loading = false;
      },
      error: () => { this.loading = false; },
    });
  }

  get combinedEntries(): CombinedEntry[] {
    const entries: CombinedEntry[] = [
      ...this.prints.map(p => ({ date: p.createdAt, kind: 'print' as const, print: p })),
      ...this.stock.map(s => ({ date: s.createdAt, kind: 'stock' as const, stock: s })),
    ];
    return entries.sort((a, b) => b.date.localeCompare(a.date));
  }

  openRecord(): void { this.quickRecord.open(); }

  revert(id: string): void {
    if (!confirm('Revert this print? Stock will be restored.')) return;
    this.transactionsService.revertPrint(id).subscribe({
      next: () => this.load(),
      error: (err: any) => { this.error = err?.error ?? 'Revert failed.'; },
    });
  }

  delete(id: string): void {
    if (!confirm('Delete this print record? This cannot be undone.')) return;
    this.transactionsService.deletePrint(id).subscribe({
      next: () => this.load(),
      error: (err: any) => { this.error = err?.error ?? 'Delete failed.'; },
    });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString();
  }

  openDetail(entry: CombinedEntry): void { this.detailEntry = entry; }
  openPrintDetail(p: PrintTransaction): void { this.detailEntry = { date: p.createdAt, kind: 'print', print: p }; }
  openStockDetail(s: StockTransaction): void { this.detailEntry = { date: s.createdAt, kind: 'stock', stock: s }; }
  closeDetail(): void { this.detailEntry = null; }

  printTotal(p: PrintTransaction): number {
    return p.materialCost + p.electricityCost + p.printerWearCost + p.fixedCost + p.extraFixedCost;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void { this.closeDetail(); }
}
