<template>
  <div class="space-y-6">
    <!-- KPI Kartları -->
    <div v-if="loading" class="grid grid-cols-3 gap-4">
      <div v-for="i in 6" :key="i" class="bg-white rounded-2xl border border-slate-200 p-5 animate-pulse">
        <div class="h-3 bg-slate-100 rounded w-24 mb-3"></div>
        <div class="h-7 bg-slate-100 rounded w-32"></div>
      </div>
    </div>

    <div v-else class="grid grid-cols-3 gap-4">
      <div
        v-for="kpi in kpiCards"
        :key="kpi.label"
        class="bg-white rounded-2xl border border-slate-200 p-5 hover:shadow-md transition-shadow"
      >
        <div class="flex items-center justify-between mb-3">
          <span class="text-xs font-semibold text-slate-400 uppercase tracking-wide">{{ kpi.label }}</span>
          <div class="w-9 h-9 rounded-xl flex items-center justify-center" :class="kpi.iconBg">
            <component :is="kpi.icon" :size="18" :class="kpi.iconColor" />
          </div>
        </div>
        <p class="text-2xl font-bold" :class="kpi.valueColor ?? 'text-slate-800'">
          {{ formatCurrency(kpi.value) }}
        </p>
        <div class="flex items-center justify-between mt-1">
          <p class="text-xs text-slate-400">{{ kpi.subtitle }}</p>
          <!-- Dönemsel karşılaştırma badge -->
          <span v-if="kpi.change !== undefined && !compLoading"
            class="text-xs font-semibold px-1.5 py-0.5 rounded-md"
            :class="changeBadgeClass(kpi.change, kpi.inverseChange)">
            {{ kpi.change >= 0 ? '▲' : '▼' }} {{ Math.abs(kpi.change).toFixed(1) }}%
          </span>
        </div>
      </div>
    </div>

    <!-- Başabaş Analizi -->
    <div v-if="!bLoading && breakeven" class="bg-white rounded-2xl border border-slate-200 p-6">
      <div class="flex items-center justify-between mb-4">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Başabaş Noktası Analizi</h3>
          <p class="text-xs text-slate-400 mt-0.5">Son 30 gün satış verisi · aylık sabit maliyetler</p>
        </div>
        <span class="px-3 py-1.5 rounded-xl text-xs font-bold"
          :class="breakeven.isAboveBreakeven ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-700'">
          {{ breakeven.isAboveBreakeven ? 'Kâr Bölgesi' : 'Zarar Bölgesi' }}
        </span>
      </div>

      <div class="grid grid-cols-4 gap-4 mb-5">
        <div class="bg-slate-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Aylık Sabit Maliyet</p>
          <p class="text-lg font-bold text-slate-800">{{ formatCurrency(breakeven.monthlyFixedCosts) }}</p>
          <p class="text-xs text-slate-400 mt-0.5">Bordro + sabit giderler</p>
        </div>
        <div class="bg-slate-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Değişken Maliyet Oranı</p>
          <p class="text-lg font-bold text-slate-800">%{{ breakeven.variableCostRatio }}</p>
          <p class="text-xs text-slate-400 mt-0.5">Satış maliyeti / ciro</p>
        </div>
        <div class="bg-slate-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Başabaş Cirosu</p>
          <p class="text-lg font-bold text-orange-600">{{ formatCurrency(breakeven.breakevenRevenue) }}</p>
          <p class="text-xs text-slate-400 mt-0.5">Bu ciroya ulaşmak şart</p>
        </div>
        <div class="bg-slate-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Güvenli Bölge</p>
          <p class="text-lg font-bold" :class="breakeven.isAboveBreakeven ? 'text-emerald-600' : 'text-red-600'">
            {{ formatCurrency(breakeven.safetyMargin) }}
          </p>
          <p class="text-xs text-slate-400 mt-0.5">Gerçek ciro − başabaş</p>
        </div>
      </div>

      <!-- Progress bar -->
      <div>
        <div class="flex justify-between text-xs text-slate-400 mb-1">
          <span>Başabaş: {{ formatCurrency(breakeven.breakevenRevenue) }}</span>
          <span>Mevcut: {{ formatCurrency(breakeven.currentMonthlyRevenue) }}</span>
        </div>
        <div class="h-3 bg-slate-100 rounded-full overflow-hidden">
          <div class="h-full rounded-full transition-all duration-500"
            :class="breakeven.isAboveBreakeven ? 'bg-emerald-500' : 'bg-red-400'"
            :style="{ width: progressWidth + '%' }">
          </div>
        </div>
        <p class="text-xs text-slate-400 mt-1 text-right">
          Başabaş noktasının
          <span class="font-semibold" :class="breakeven.isAboveBreakeven ? 'text-emerald-600' : 'text-red-600'">
            %{{ Math.abs(breakeven.safetyMarginPercent).toFixed(1) }}
            {{ breakeven.isAboveBreakeven ? 'üzerinde' : 'altında' }}
          </span>
        </p>
      </div>
    </div>

    <!-- Gelir vs Gider Grafiği -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <h3 class="text-sm font-bold text-slate-700 mb-4">Son 12 Ay — Gelir & Gider Karşılaştırması</h3>
      <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
      <apexchart
        v-else
        type="bar"
        height="280"
        :options="chartOptions"
        :series="chartSeries"
      />
    </div>

    <!-- Nakit Akış Projeksiyonu -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <div class="flex items-center justify-between mb-5">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Nakit Akış Projeksiyonu</h3>
          <p class="text-xs text-slate-400 mt-0.5">Son 6 ay gerçek + sonraki 3 ay tahmini (son 3 aylık ortalama)</p>
        </div>
        <span class="px-2.5 py-1 bg-slate-100 text-slate-500 rounded-lg text-xs font-medium">* Tahmini ay</span>
      </div>

      <!-- KPI özeti -->
      <div v-if="!cfLoading && cashflow" class="grid grid-cols-4 gap-4 mb-5">
        <div class="bg-blue-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Ort. Aylık Gelir</p>
          <p class="text-lg font-bold text-blue-700">{{ formatCurrency(cashflow.avgMonthlyRevenue) }}</p>
        </div>
        <div class="bg-orange-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Ort. Aylık Gider</p>
          <p class="text-lg font-bold text-orange-700">{{ formatCurrency(cashflow.avgMonthlyExpenses) }}</p>
        </div>
        <div class="bg-slate-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Ort. Net Nakit</p>
          <p class="text-lg font-bold" :class="cashflow.avgNetCashFlow >= 0 ? 'text-emerald-600' : 'text-red-600'">
            {{ formatCurrency(cashflow.avgNetCashFlow) }}
          </p>
        </div>
        <div class="bg-violet-50 rounded-xl p-4">
          <p class="text-xs text-slate-400 mb-1">Yıllık Ciro Tahmini</p>
          <p class="text-lg font-bold text-violet-700">{{ formatCurrency(cashflow.projectedAnnualRevenue) }}</p>
        </div>
      </div>

      <div v-if="cfLoading" class="h-56 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!cashflow" class="h-56 flex items-center justify-center text-slate-400 text-sm">
        Veri yok
      </div>
      <apexchart v-else type="bar" height="240" :options="cfChartOptions" :series="cfSeries" />
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Veriler yüklenirken hata oluştu. Lütfen tekrar deneyin.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import {
  TrendingUpIcon, DollarSignIcon, ArrowDownIcon,
  BarChart2Icon, PackageIcon, CreditCardIcon,
} from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface OverviewData {
  totalRevenue: number; totalProfit: number; totalExpenses: number
  netProfit: number; stockValue: number; totalReceivables: number
  revenueVsExpense: { label: string; revenue: number; expense: number }[]
}
interface ComparisonData {
  revenue: { changePercent: number }; profit: { changePercent: number }
  expenses: { changePercent: number }; netProfit: { changePercent: number }
  orderCount: { changePercent: number }
}
interface BreakevenData {
  monthlyFixedCosts: number; variableCostRatio: number; breakevenRevenue: number
  currentMonthlyRevenue: number; safetyMargin: number; safetyMarginPercent: number
  isAboveBreakeven: boolean
}

interface CashFlowPoint {
  label: string; revenue: number; expenses: number; netCashFlow: number; isProjection: boolean
}
interface CashFlowData {
  avgMonthlyRevenue: number; avgMonthlyExpenses: number
  avgNetCashFlow: number; projectedAnnualRevenue: number
  points: CashFlowPoint[]
}

const data = ref<OverviewData | null>(null)
const comparison = ref<ComparisonData | null>(null)
const breakeven = ref<BreakevenData | null>(null)
const cashflow = ref<CashFlowData | null>(null)
const loading = ref(true)
const compLoading = ref(true)
const bLoading = ref(true)
const cfLoading = ref(true)
const error = ref(false)

const fetchData = async () => {
  loading.value = true; error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/overview?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (!res.ok) throw new Error()
    data.value = await res.json()
  } catch { error.value = true }
  finally { loading.value = false }
}

const fetchComparison = async () => {
  compLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/period-comparison?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) comparison.value = await res.json()
  } catch { /* sessiz hata */ }
  finally { compLoading.value = false }
}

const fetchBreakeven = async () => {
  bLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/breakeven`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) breakeven.value = await res.json()
  } catch { /* sessiz hata */ }
  finally { bLoading.value = false }
}

const fetchCashflow = async () => {
  cfLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/cashflow-projection`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) cashflow.value = await res.json()
  } catch { /* sessiz */ }
  finally { cfLoading.value = false }
}

const loadAll = () => { fetchData(); fetchComparison(); fetchBreakeven(); fetchCashflow() }
onMounted(loadAll)
watch(() => props.range, () => { fetchData(); fetchComparison() })

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(val ?? 0)

const changeBadgeClass = (pct: number, inverse = false) => {
  const positive = inverse ? pct < 0 : pct >= 0
  return positive ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-600'
}

const kpiCards = computed(() => [
  {
    label: 'Toplam Ciro', value: data.value?.totalRevenue ?? 0,
    subtitle: 'Seçili dönemdeki toplam satış',
    icon: TrendingUpIcon, iconBg: 'bg-blue-50', iconColor: 'text-blue-500',
    change: comparison.value?.revenue.changePercent,
  },
  {
    label: 'Brüt Kâr', value: data.value?.totalProfit ?? 0,
    subtitle: 'Satış geliri - maliyet',
    icon: DollarSignIcon, iconBg: 'bg-emerald-50', iconColor: 'text-emerald-500',
    change: comparison.value?.profit.changePercent,
  },
  {
    label: 'Toplam Gider', value: data.value?.totalExpenses ?? 0,
    subtitle: 'Gider + bordro maliyeti',
    icon: ArrowDownIcon, iconBg: 'bg-orange-50', iconColor: 'text-orange-500',
    change: comparison.value?.expenses.changePercent,
    inverseChange: true,
  },
  {
    label: 'Net Kâr', value: data.value?.netProfit ?? 0,
    subtitle: 'Brüt kâr - toplam gider',
    icon: BarChart2Icon,
    iconBg: (data.value?.netProfit ?? 0) >= 0 ? 'bg-emerald-50' : 'bg-red-50',
    iconColor: (data.value?.netProfit ?? 0) >= 0 ? 'text-emerald-500' : 'text-red-500',
    valueColor: (data.value?.netProfit ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-600',
    change: comparison.value?.netProfit.changePercent,
  },
  {
    label: 'Stok Değeri', value: data.value?.stockValue ?? 0,
    subtitle: 'Mevcut stok × birim maliyet',
    icon: PackageIcon, iconBg: 'bg-violet-50', iconColor: 'text-violet-500',
  },
  {
    label: 'Açık Veresiye', value: data.value?.totalReceivables ?? 0,
    subtitle: 'Müşterilerden alacak toplamı',
    icon: CreditCardIcon, iconBg: 'bg-rose-50', iconColor: 'text-rose-500',
    valueColor: 'text-rose-600', inverseChange: true,
  },
])

const progressWidth = computed(() => {
  if (!breakeven.value) return 0
  const { breakevenRevenue, currentMonthlyRevenue } = breakeven.value
  if (breakevenRevenue <= 0) return 100
  return Math.min(Math.round((currentMonthlyRevenue / breakevenRevenue) * 100), 100)
})

const chartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: { bar: { columnWidth: '55%', borderRadius: 6, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  stroke: { show: false },
  xaxis: {
    categories: data.value?.revenueVsExpense.map(r => r.label) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false }, axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#3b82f6', '#f97316'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
}))

const chartSeries = computed(() => [
  { name: 'Gelir', data: data.value?.revenueVsExpense.map(r => r.revenue) ?? [] },
  { name: 'Gider', data: data.value?.revenueVsExpense.map(r => r.expense) ?? [] },
])

const cfChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: { bar: { columnWidth: '50%', borderRadius: 5, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: cashflow.value?.points.map(p => p.label) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false }, axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#3b82f6', '#f97316', '#10b981'],
  fill: {
    opacity: cashflow.value?.points.map(p => p.isProjection ? 0.4 : 1) ?? [],
  },
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
}))

const cfSeries = computed(() => [
  { name: 'Gelir',      data: cashflow.value?.points.map(p => p.revenue) ?? [] },
  { name: 'Gider',      data: cashflow.value?.points.map(p => p.expenses) ?? [] },
  { name: 'Net Nakit',  data: cashflow.value?.points.map(p => p.netCashFlow) ?? [] },
])
</script>
