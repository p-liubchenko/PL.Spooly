import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { PrintersService } from '../../services/printers.service';
import { MaterialsService } from '../../services/materials.service';
import { TransactionsService } from '../../services/transactions.service';
import { SettingsService } from '../../services/settings.service';
import {
  Printer, FilamentMaterial, AppSettings,
  StockTransaction, PrintTransaction, PrintTransactionStatus,
  FILAMENT_TYPE_LABELS,
} from '../../models';

const CHART_COLORS = ['#58a6ff', '#3fb950', '#d29922', '#f78166', '#bc8cff', '#79c0ff', '#8b949e'];
const DAYS = 30;

// SVG layout constants (shared by both charts)
const L  = 66;    // left (y-axis labels)
const T  = 12;    // top
const R  = 20;    // right
const B  = 35;    // bottom (x-axis labels)
const VW = 900;
const VH = 300;
const CW = VW - L - R;
const CH = VH - T - B;
const ZY = T + CH / 2;   // zero-line Y for candlestick chart

// ── Candlestick types ─────────────────────────────────────────────────────

interface GridLine { y: number; label: string; isZero: boolean; }

export interface CandleBar {
  wickX: number; capX1: number; capX2: number;
  topWickY: number; bottomWickY: number;
  bodyLeft: number; bodyWidth: number;
  bodyTopY: number; bodyBotY: number; bodyH: number;
  isGreen: boolean; hasData: boolean;
}

export interface CandleSeries {
  type: string; color: string; candles: CandleBar[];
}

export interface ChartData {
  series: CandleSeries[];
  zeroY: number;
  axisLabels: { label: string; x: number; show: boolean }[];
  yGridLines: GridLine[];
  left: number; right: number; viewBoxW: number; viewBoxH: number;
}

// ── Stock-level line chart types ──────────────────────────────────────────

export interface StockLineSeries {
  type: string;
  color: string;
  polylinePoints: string;   // SVG polyline points attribute
  areaPoints: string;       // SVG polygon points (closed area to bottom)
}

export interface StockChartData {
  series: StockLineSeries[];
  axisLabels: { label: string; x: number; show: boolean }[];
  yGridLines: { y: number; label: string; isZero: boolean }[];
  left: number; right: number; viewBoxW: number; viewBoxH: number;
}

// ─────────────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  printers:  Printer[] = [];
  materials: FilamentMaterial[] = [];
  prints:    PrintTransaction[] = [];
  stock:     StockTransaction[] = [];
  settings:  AppSettings | null = null;
  loading = true;

  chartData:      ChartData      | null = null;
  stockChartData: StockChartData | null = null;

  constructor(
    private printersService:     PrintersService,
    private materialsService:    MaterialsService,
    private transactionsService: TransactionsService,
    private settingsService:     SettingsService,
  ) {}

  ngOnInit(): void {
    forkJoin({
      printers:  this.printersService.getAll(),
      materials: this.materialsService.getAll(),
      prints:    this.transactionsService.getPrints(),
      stock:     this.transactionsService.getStock(),
      settings:  this.settingsService.get(),
    }).subscribe({
      next: res => {
        this.printers  = res.printers;
        this.materials = res.materials;
        this.prints    = res.prints;
        this.stock     = res.stock;
        this.settings  = res.settings;
        this.chartData      = this.buildChart(res.stock, res.prints);
        this.stockChartData = this.buildStockChart(res.materials, res.stock, res.prints);
        this.loading = false;
      },
      error: () => { this.loading = false; },
    });
  }

  get selectedPrinter(): Printer | undefined {
    return this.printers.find(p => p.id === this.settings?.selectedPrinterId);
  }

  get totalStockKg(): number {
    return this.materials.reduce((s, m) => s + m.amountKg, 0);
  }

  get printCount(): number {
    return this.prints.filter(p => p.status === PrintTransactionStatus.Completed).length;
  }

  // ── Filament flow (candlestick) ───────────────────────────────────────

  private buildChart(stockTxs: StockTransaction[], prints: PrintTransaction[]): ChartData | null {
    const months = this.getLastNDays(DAYS);

    const typeMap = new Map<string, Map<string, { positive: number; negative: number }>>();
    const ensure = (typeLabel: string, key: string) => {
      if (!typeMap.has(typeLabel)) typeMap.set(typeLabel, new Map());
      const mm = typeMap.get(typeLabel)!;
      if (!mm.has(key)) mm.set(key, { positive: 0, negative: 0 });
      return mm.get(key)!;
    };

    for (const s of stockTxs) {
      const mat = this.materials.find(m => m.id === s.materialId);
      const label = mat != null ? FILAMENT_TYPE_LABELS[mat.type] : 'Other';
      const entry = ensure(label, s.createdAt.substring(0, 10));
      if (s.kgDelta > 0) entry.positive += s.kgDelta;
      else entry.negative += s.kgDelta;
    }

    for (const p of prints) {
      if (p.status !== PrintTransactionStatus.Completed) continue;
      const mat = this.materials.find(m => m.id === p.materialId);
      const label = mat != null ? FILAMENT_TYPE_LABELS[mat.type] : 'Other';
      const entry = ensure(label, p.createdAt.substring(0, 10));
      entry.negative -= p.filamentKg;
    }

    if (typeMap.size === 0) return null;

    const typeCount = typeMap.size;
    const slotW     = CW / months.length;
    const candleGap = Math.max(1, Math.floor(slotW * 0.08));
    const candleW   = Math.max(2, Math.floor(slotW * 0.62 / typeCount - candleGap));
    const groupW    = typeCount * candleW + (typeCount - 1) * candleGap;
    const centerXs  = months.map((_, i) => L + (i + 0.5) * slotW);

    let maxPos = 0.001, maxNeg = 0.001;
    for (const mm of typeMap.values())
      for (const { positive, negative } of mm.values()) {
        if (positive > maxPos)          maxPos = positive;
        if (Math.abs(negative) > maxNeg) maxNeg = Math.abs(negative);
      }

    const halfH  = CH / 2;
    const pyPos  = (kg: number) => ZY - (kg / maxPos) * halfH;
    const pyNeg  = (kg: number) => ZY + (Math.abs(kg) / maxNeg) * halfH;
    const pyNet  = (kg: number) => kg >= 0 ? pyPos(kg) : pyNeg(kg);

    const series: CandleSeries[] = Array.from(typeMap.entries()).map(([type, mm], idx) => ({
      type,
      color: CHART_COLORS[idx % CHART_COLORS.length],
      candles: months.map((m, mi) => {
        const { positive, negative } = mm.get(m.key) ?? { positive: 0, negative: 0 };
        const net     = positive + negative;
        const hasData = positive !== 0 || negative !== 0;

        const groupLeft = centerXs[mi] - groupW / 2;
        const slotLeft  = groupLeft + idx * (candleW + candleGap);
        const wickX     = slotLeft + candleW / 2;
        const capHalf   = Math.max(4, candleW * 0.35);

        const topWickY    = positive > 0 ? pyPos(positive) : ZY;
        const bottomWickY = negative < 0 ? pyNeg(negative) : ZY;
        const bodyOpenY   = ZY;
        const bodyCloseY  = hasData ? pyNet(net) : ZY;
        const bodyTopY    = Math.min(bodyOpenY, bodyCloseY);
        const bodyBotY    = Math.max(bodyOpenY, bodyCloseY);
        const bodyH       = Math.max(bodyBotY - bodyTopY, 1.5);
        const bodyWidth   = Math.max(4, Math.round(candleW * 0.55));
        const bodyLeft    = wickX - bodyWidth / 2;

        return {
          wickX, capX1: wickX - capHalf, capX2: wickX + capHalf,
          topWickY, bottomWickY,
          bodyLeft, bodyWidth, bodyTopY, bodyBotY, bodyH,
          isGreen: net >= 0, hasData,
        };
      }),
    }));

    const yGridLines: GridLine[] = [
      { y: T,              label: `+${maxPos.toFixed(3)} kg`, isZero: false },
      { y: ZY - halfH / 2, label: `+${(maxPos / 2).toFixed(3)}`,  isZero: false },
      { y: ZY,             label: '0',                             isZero: true  },
      { y: ZY + halfH / 2, label: `−${(maxNeg / 2).toFixed(3)}`,  isZero: false },
      { y: VH - B,         label: `−${maxNeg.toFixed(3)} kg`,      isZero: false },
    ];

    const axisLabels = months.map((m, i) => ({
      label: m.label,
      x: centerXs[i],
      show: m.isMonthStart || i % 5 === 0 || i === months.length - 1,
    }));

    return { series, zeroY: ZY, axisLabels, yGridLines, left: L, right: VW - R, viewBoxW: VW, viewBoxH: VH };
  }

  // ── Stock level (line chart) ──────────────────────────────────────────

  private buildStockChart(
    materials: FilamentMaterial[],
    stockTxs: StockTransaction[],
    prints: PrintTransaction[],
  ): StockChartData | null {
    const days = this.getLastNDays(DAYS);

    // Current stock grouped by type label
    const currentByType = new Map<string, number>();
    for (const mat of materials) {
      const label = FILAMENT_TYPE_LABELS[mat.type] ?? 'Other';
      currentByType.set(label, (currentByType.get(label) ?? 0) + mat.amountKg);
    }
    if (currentByType.size === 0) return null;

    const types = Array.from(currentByType.keys());

    // stockByDay[dayIndex] → typeLabel → kg at END of that day
    const stockByDay: Map<string, number>[] = Array.from({ length: DAYS }, () => new Map());

    // Today is the last slot
    for (const [type, kg] of currentByType)
      stockByDay[DAYS - 1].set(type, kg);

    // Walk backwards, undoing each day's transactions
    for (let i = DAYS - 2; i >= 0; i--) {
      const nextKey = days[i + 1].key;  // the day whose transactions we're reversing

      // Inherit from the day ahead
      for (const type of types)
        stockByDay[i].set(type, stockByDay[i + 1].get(type) ?? 0);

      // Undo stock transactions that happened on nextKey
      for (const s of stockTxs) {
        if (s.createdAt.substring(0, 10) !== nextKey) continue;
        const mat   = materials.find(m => m.id === s.materialId);
        const label = mat ? (FILAMENT_TYPE_LABELS[mat.type] ?? 'Other') : null;
        if (!label || !types.includes(label)) continue;
        stockByDay[i].set(label, Math.max(0, (stockByDay[i].get(label) ?? 0) - s.kgDelta));
      }

      // Undo print transactions that happened on nextKey
      for (const p of prints) {
        if (p.status !== PrintTransactionStatus.Completed) continue;
        if (p.createdAt.substring(0, 10) !== nextKey) continue;
        const mat   = materials.find(m => m.id === p.materialId);
        const label = mat ? (FILAMENT_TYPE_LABELS[mat.type] ?? 'Other') : null;
        if (!label || !types.includes(label)) continue;
        stockByDay[i].set(label, (stockByDay[i].get(label) ?? 0) + p.filamentKg);
      }
    }

    // Max kg for y-axis scale
    let maxKg = 0.001;
    for (const day of stockByDay)
      for (const kg of day.values())
        if (kg > maxKg) maxKg = kg;

    const slotW  = CW / DAYS;
    const dayXs  = days.map((_, i) => L + (i + 0.5) * slotW);
    const bottom = T + CH;
    const toY    = (kg: number) => T + CH * (1 - kg / maxKg);

    const series: StockLineSeries[] = types.map((type, idx) => {
      const pts = days.map((_, i) => {
        const kg = stockByDay[i].get(type) ?? 0;
        return `${dayXs[i].toFixed(1)},${toY(kg).toFixed(1)}`;
      });

      const polylinePoints = pts.join(' ');
      const areaPoints = [
        `${dayXs[0].toFixed(1)},${bottom}`,
        ...pts,
        `${dayXs[DAYS - 1].toFixed(1)},${bottom}`,
      ].join(' ');

      return { type, color: CHART_COLORS[idx % CHART_COLORS.length], polylinePoints, areaPoints };
    });

    const step = maxKg / 4;
    const yGridLines = [0, 1, 2, 3, 4].map(n => ({
      y: toY(step * n),
      label: n === 0 ? '0' : `${(step * n).toFixed(2)} kg`,
      isZero: n === 0,
    }));

    const axisLabels = days.map((d, i) => ({
      label: d.label,
      x: dayXs[i],
      show: d.isMonthStart || i % 5 === 0 || i === days.length - 1,
    }));

    return { series, axisLabels, yGridLines, left: L, right: VW - R, viewBoxW: VW, viewBoxH: VH };
  }

  // ── Shared helpers ────────────────────────────────────────────────────

  private getLastNDays(n: number): { key: string; label: string; isMonthStart: boolean }[] {
    const result: { key: string; label: string; isMonthStart: boolean }[] = [];
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    for (let i = n - 1; i >= 0; i--) {
      const d = new Date(now.getTime() - i * 86_400_000);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
      const isMonthStart = d.getDate() === 1;
      const label = isMonthStart
        ? d.toLocaleString('default', { month: 'short', day: 'numeric' })
        : String(d.getDate());
      result.push({ key, label, isMonthStart });
    }
    return result;
  }
}
