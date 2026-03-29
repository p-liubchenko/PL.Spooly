import { Component, EventEmitter, HostListener, OnDestroy, OnInit, Output } from '@angular/core';
import { forkJoin, Subscription } from 'rxjs';
import { FilamentMaterial, FilamentType, FILAMENT_TYPE_LABELS, Printer, PrintCostResult, AppSettings } from '../models';
import { MaterialsService } from '../services/materials.service';
import { PrintersService } from '../services/printers.service';
import { SettingsService } from '../services/settings.service';
import { TransactionsService, RecordPrintRequest } from '../services/transactions.service';
import { QuickRecordService } from '../services/quick-record.service';

@Component({
  selector: 'app-quick-record',
  standalone: false,
  templateUrl: './quick-record.component.html',
})
export class QuickRecordComponent implements OnInit, OnDestroy {
  @Output() saved = new EventEmitter<void>();

  visible = false;

  // Data
  materials: FilamentMaterial[] = [];
  printers: Printer[] = [];
  settings: AppSettings | null = null;
  loadError = '';

  // Cascading type → material selection
  selectedType: FilamentType | null = null;

  // Form
  form: RecordPrintRequest = { materialId: '', printerId: '', filamentGrams: 0, printHours: 0, extraFixedCost: 0 };

  // Calculation state
  calcResult: PrintCostResult | null = null;
  calcError = '';
  calculating = false;
  saving = false;
  saveError = '';

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;
  private openSub?: Subscription;

  constructor(
    private materialsService: MaterialsService,
    private printersService: PrintersService,
    private settingsService: SettingsService,
    private transactionsService: TransactionsService,
    private quickRecordService: QuickRecordService,
  ) {}

  ngOnInit(): void {
    this.openSub = this.quickRecordService.open$.subscribe(() => this.open());
  }

  ngOnDestroy(): void {
    this.openSub?.unsubscribe();
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
  }

  /** Types present in the current materials list, in enum order. */
  get availableTypes(): { type: FilamentType; label: string }[] {
    const seen = new Set<FilamentType>();
    for (const m of this.materials) seen.add(m.type);
    return Array.from(seen)
      .sort((a, b) => a - b)
      .map(type => ({ type, label: FILAMENT_TYPE_LABELS[type] ?? String(type) }));
  }

  /** Materials filtered to the selected type. */
  get filteredMaterials(): FilamentMaterial[] {
    if (this.selectedType === null) return this.materials;
    return this.materials.filter(m => m.type === this.selectedType);
  }

  open(): void {
    this.reset();
    this.visible = true;
    forkJoin({
      materials: this.materialsService.getAll(),
      printers:  this.printersService.getAll(),
      settings:  this.settingsService.get(),
    }).subscribe({
      next: res => {
        this.materials = res.materials;
        this.printers  = res.printers;
        this.settings  = res.settings;
        this.form.printerId = res.settings?.selectedPrinterId ?? res.printers[0]?.id ?? '';
        this.pickDefaultType();
      },
      error: () => { this.loadError = 'Failed to load data.'; },
    });
  }

  close(): void { this.visible = false; }

  reset(): void {
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    this.selectedType = null;
    this.form = {
      materialId: '',
      printerId: this.settings?.selectedPrinterId ?? this.printers[0]?.id ?? '',
      filamentGrams: 0,
      printHours: 0,
      extraFixedCost: 0,
    };
    this.calcResult  = null;
    this.calcError   = '';
    this.saveError   = '';
    this.calculating = false;
    this.saving      = false;
    this.loadError   = '';
  }

  onTypeChange(): void {
    // Auto-select the first material of the new type
    this.form.materialId = this.filteredMaterials[0]?.id ?? '';
    this.onFormChange();
  }

  onFormChange(): void {
    this.calcResult = null;
    this.calcError  = '';
    this.saveError  = '';
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    if (!this.canCalculate()) return;
    this.debounceTimer = setTimeout(() => this.calculate(), 450);
  }

  canCalculate(): boolean {
    return !!(this.form.materialId && this.form.printerId && this.form.filamentGrams > 0 && this.form.printHours > 0);
  }

  calculate(): void {
    if (!this.canCalculate()) return;
    this.calculating = true;
    this.calcError   = '';
    this.transactionsService.calculate(this.form).subscribe({
      next: res  => { this.calcResult = res; this.calculating = false; },
      error: err => { this.calcError = err?.error ?? 'Calculation failed.'; this.calculating = false; },
    });
  }

  save(): void {
    if (!this.calcResult) return;
    this.saving    = true;
    this.saveError = '';
    this.transactionsService.recordPrint(this.form).subscribe({
      next: () => { this.saving = false; this.close(); this.saved.emit(); },
      error: err => { this.saveError = err?.error ?? 'Failed to save.'; this.saving = false; },
    });
  }

  @HostListener('document:keydown.escape')
  onEscape(): void { if (this.visible) this.close(); }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private pickDefaultType(): void {
    const types = this.availableTypes;
    if (!types.length) return;
    this.selectedType = types[0].type;
    this.form.materialId = this.filteredMaterials[0]?.id ?? '';
  }
}
