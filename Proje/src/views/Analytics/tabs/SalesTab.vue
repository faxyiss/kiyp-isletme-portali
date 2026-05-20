<template>
  <div class="space-y-6">
    <!-- KPI Kartları -->
    <div v-if="loading" class="grid grid-cols-4 gap-4">
      <div v-for="i in 4" :key="i" class="bg-white rounded-2xl border border-slate-200 p-5 animate-pulse">
        <div class="h-3 bg-slate-100 rounded w-24 mb-3"></div>
        <div class="h-7 bg-slate-100 rounded w-32"></div>
      </div>
    </div>
    <div v-else class="grid grid-cols-4 gap-4">
      <div v-for="kpi in kpiCards" :key="kpi.label"
        class="bg-white rounded-2xl border border-slate-200 p-5 hover:shadow-md transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <span class="text-xs font-semibold text-slate-400 uppercase tracking-wide">{{ kpi.label }}</span>
          <div class="w-9 h-9 rounded-xl flex items-center justify-center" :class="kpi.iconBg">
            <component :is="kpi.icon" :size="18" :class="kpi.iconColor" />
          </div>
        </div>
        <p class="text-2xl font-bold text-slate-800">{{ kpi.formatted }}</p>
      </div>
    </div>

    <!-- Grafikler: Günlük Trend + Ödeme Tipi -->
    <div class="grid grid-cols-3 gap-4">
      <!-- Günlük Ciro & Kâr Trendi -->
      <div class="col-span-2 bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Günlük Ciro & Kâr Trendi</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.dailyRevenue.length" class="h-64 flex items-center justify-center text-slate-400 text-sm">
          Bu dönemde satış verisi yok
        </div>
        <apexchart v-else type="area" height="260" :options="trendChartOptions" :series="trendSeries" />
      </div>

      <!-- Ödeme Tipi Dağılımı -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Ödeme Tipi Dağılımı</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!hasPaymentData" class="h-64 flex items-center justify-center text-slate-400 text-sm">
          Veri yok
        </div>
        <apexchart v-else type="donut" height="260" :options="paymentChartOptions" :series="paymentSeries" />
      </div>
    </div>

    <!-- Kategori Performansı -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <h3 class="text-sm font-bold text-slate-700 mb-4">Kategori Bazlı Satış Performansı</h3>
      <div v-if="loading" class="h-48 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!data?.categorySales.length" class="h-48 flex items-center justify-center text-slate-400 text-sm">
        Kategori verisi yok
      </div>
      <apexchart v-else type="bar" height="220" :options="categoryChartOptions" :series="categorySeries" />
    </div>

    <!-- En Çok Satan Ürünler Tablosu -->
    <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100">
        <h3 class="text-sm font-bold text-slate-700">En Çok Satan 10 Ürün</h3>
      </div>
      <div v-if="loading" class="p-6 space-y-3">
        <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="!data?.topProducts.length" class="p-8 text-center text-slate-400 text-sm">
        Bu dönemde satış verisi yok
      </div>
      <table v-else class="w-full text-sm">
        <thead>
          <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
            <th class="px-6 py-3 text-left">#</th>
            <th class="px-6 py-3 text-left">Ürün Adı</th>
            <th class="px-6 py-3 text-right">Satılan Adet</th>
            <th class="px-6 py-3 text-right">Ciro</th>
            <th class="px-6 py-3 text-right">Kâr</th>
            <th class="px-6 py-3 text-right">Kâr Marjı</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(p, i) in data.topProducts" :key="i"
            class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
            <td class="px-6 py-4 text-slate-400 font-medium">{{ i + 1 }}</td>
            <td class="px-6 py-4 font-semibold text-slate-800">{{ p.name }}</td>
            <td class="px-6 py-4 text-right text-slate-600">{{ formatNum(p.quantity) }}</td>
            <td class="px-6 py-4 text-right font-medium text-slate-800">{{ formatCurrency(p.revenue) }}</td>
            <td class="px-6 py-4 text-right font-medium text-emerald-600">{{ formatCurrency(p.profit) }}</td>
            <td class="px-6 py-4 text-right">
              <span class="px-2.5 py-1 rounded-lg text-xs font-semibold"
                :class="marginColor(p.revenue > 0 ? (p.profit / p.revenue) * 100 : 0)">
                %{{ (p.revenue > 0 ? (p.profit / p.revenue) * 100 : 0).toFixed(1) }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Ürün Portföy Matrisi (BCG) -->
    <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Ürün Portföy Matrisi</h3>
          <p class="text-xs text-slate-400 mt-0.5">Büyüme hızı ve pazar payına göre ürün sınıflandırması</p>
        </div>
        <div v-if="!portfolioLoading && portfolio" class="flex gap-2">
          <span class="px-2.5 py-1 bg-emerald-50 text-emerald-700 rounded-lg text-xs font-semibold">
            ★ {{ portfolioCount('Yıldız') }} Yıldız
          </span>
          <span class="px-2.5 py-1 bg-blue-50 text-blue-700 rounded-lg text-xs font-semibold">
            ◆ {{ portfolioCount('Nakit Kaynağı') }} Nakit
          </span>
          <span class="px-2.5 py-1 bg-orange-50 text-orange-700 rounded-lg text-xs font-semibold">
            ? {{ portfolioCount('Soru İşareti') }} Soru
          </span>
          <span class="px-2.5 py-1 bg-red-50 text-red-700 rounded-lg text-xs font-semibold">
            ✕ {{ portfolioCount('Köpek') }} Köpek
          </span>
        </div>
      </div>

      <div v-if="portfolioLoading" class="p-6 space-y-3">
        <div v-for="i in 6" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="!portfolio?.products.length" class="p-8 text-center text-slate-400 text-sm">
        Bu dönemde yeterli satış verisi yok
      </div>
      <div v-else>
        <!-- Kuadrant açıklaması -->
        <div class="grid grid-cols-4 gap-0 border-b border-slate-100">
          <div class="p-4 bg-emerald-50/40 border-r border-slate-100">
            <p class="text-xs font-bold text-emerald-700">Yıldız</p>
            <p class="text-xs text-slate-500 mt-0.5">Yüksek büyüme + yüksek pay</p>
          </div>
          <div class="p-4 bg-blue-50/40 border-r border-slate-100">
            <p class="text-xs font-bold text-blue-700">Nakit Kaynağı</p>
            <p class="text-xs text-slate-500 mt-0.5">Düşük büyüme + yüksek pay</p>
          </div>
          <div class="p-4 bg-orange-50/40 border-r border-slate-100">
            <p class="text-xs font-bold text-orange-700">Soru İşareti</p>
            <p class="text-xs text-slate-500 mt-0.5">Yüksek büyüme + düşük pay</p>
          </div>
          <div class="p-4 bg-red-50/40">
            <p class="text-xs font-bold text-red-700">Köpek</p>
            <p class="text-xs text-slate-500 mt-0.5">Düşük büyüme + düşük pay</p>
          </div>
        </div>
        <table class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-6 py-3 text-left">Ürün</th>
              <th class="px-6 py-3 text-left">Kategori</th>
              <th class="px-6 py-3 text-right">Ciro</th>
              <th class="px-6 py-3 text-right">Pazar Payı</th>
              <th class="px-6 py-3 text-right">Büyüme</th>
              <th class="px-6 py-3 text-center">Sınıf</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in portfolio.products" :key="p.productName"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-6 py-3 font-semibold text-slate-800">{{ p.productName }}</td>
              <td class="px-6 py-3 text-slate-500">{{ p.categoryName }}</td>
              <td class="px-6 py-3 text-right text-slate-700">{{ formatCurrency(p.revenue) }}</td>
              <td class="px-6 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <div class="w-16 h-1.5 bg-slate-100 rounded-full overflow-hidden">
                    <div class="h-full bg-blue-400 rounded-full" :style="{ width: Math.min(p.revenueShare, 100) + '%' }"></div>
                  </div>
                  <span class="text-xs text-slate-600 font-medium">%{{ p.revenueShare }}</span>
                </div>
              </td>
              <td class="px-6 py-3 text-right">
                <span class="text-xs font-semibold" :class="p.growthRate >= 0 ? 'text-emerald-600' : 'text-red-600'">
                  {{ p.growthRate >= 0 ? '▲' : '▼' }} {{ Math.abs(p.growthRate) }}%
                </span>
              </td>
              <td class="px-6 py-3 text-center">
                <span class="px-2.5 py-1 rounded-lg text-xs font-semibold"
                  :class="quadrantBadge(p.quadrantColor)">
                  {{ p.quadrant }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Satış verileri yüklenirken hata oluştu.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { TrendingUpIcon, DollarSignIcon, ShoppingBagIcon, ReceiptIcon } from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface SalesData {
  totalRevenue: number
  totalProfit: number
  totalOrders: number
  avgOrderValue: number
  dailyRevenue: { date: string; revenue: number; profit: number }[]
  paymentBreakdown: { label: string; value: number }[]
  categorySales: { category: string; revenue: number; profit: number }[]
  topProducts: { name: string; quantity: number; revenue: number; profit: number }[]
}

interface PortfolioProduct {
  productName: string; categoryName: string
  revenue: number; revenueShare: number; growthRate: number
  quadrant: string; quadrantColor: string
}
interface PortfolioData {
  totalRevenue: number; avgGrowthRate: number; avgRevenueShare: number
  products: PortfolioProduct[]
}

const data = ref<SalesData | null>(null)
const portfolio = ref<PortfolioData | null>(null)
const loading = ref(true)
const portfolioLoading = ref(true)
const error = ref(false)

const fetchData = async () => {
  loading.value = true
  error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/sales?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (!res.ok) throw new Error()
    data.value = await res.json()
  } catch {
    error.value = true
  } finally {
    loading.value = false
  }
}

const fetchPortfolio = async () => {
  portfolioLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/product-portfolio?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) portfolio.value = await res.json()
  } catch { /* sessiz */ }
  finally { portfolioLoading.value = false }
}

onMounted(() => { fetchData(); fetchPortfolio() })
watch(() => props.range, () => { fetchData(); fetchPortfolio() })

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(v ?? 0)
const formatNum = (v: number) =>
  new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(v ?? 0)

const marginColor = (pct: number) =>
  pct >= 30 ? 'bg-emerald-50 text-emerald-700' : pct >= 15 ? 'bg-blue-50 text-blue-700' : 'bg-orange-50 text-orange-700'

const kpiCards = computed(() => [
  { label: 'Toplam Ciro',    formatted: formatCurrency(data.value?.totalRevenue ?? 0),    icon: TrendingUpIcon,  iconBg: 'bg-blue-50',    iconColor: 'text-blue-500' },
  { label: 'Toplam Kâr',    formatted: formatCurrency(data.value?.totalProfit ?? 0),     icon: DollarSignIcon,  iconBg: 'bg-emerald-50', iconColor: 'text-emerald-500' },
  { label: 'Satış Adedi',   formatted: formatNum(data.value?.totalOrders ?? 0),          icon: ShoppingBagIcon, iconBg: 'bg-violet-50',  iconColor: 'text-violet-500' },
  { label: 'Ort. Sepet',    formatted: formatCurrency(data.value?.avgOrderValue ?? 0),   icon: ReceiptIcon,     iconBg: 'bg-orange-50',  iconColor: 'text-orange-500' },
])

const hasPaymentData = computed(() => data.value?.paymentBreakdown.some(p => p.value > 0))

// Günlük trend chart
const trendChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  stroke: { curve: 'smooth', width: 2 },
  fill: { type: 'gradient', gradient: { opacityFrom: 0.3, opacityTo: 0.05 } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: data.value?.dailyRevenue.map(d => d.date) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '11px' }, rotate: -30 },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#3b82f6', '#10b981'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: {
    y: { formatter: (v: number) => formatCurrency(v) },
  },
}))

const trendSeries = computed(() => [
  { name: 'Ciro',  data: data.value?.dailyRevenue.map(d => d.revenue) ?? [] },
  { name: 'Kâr',   data: data.value?.dailyRevenue.map(d => d.profit) ?? [] },
])

// Ödeme tipi donut
const paymentChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  labels: data.value?.paymentBreakdown.map(p => p.label) ?? [],
  colors: ['#10b981', '#3b82f6', '#f97316'],
  legend: { position: 'bottom', labels: { colors: '#64748b' } },
  dataLabels: { style: { fontSize: '12px' } },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
  plotOptions: { pie: { donut: { size: '65%' } } },
}))

const paymentSeries = computed(() => data.value?.paymentBreakdown.map(p => p.value) ?? [])

// Kategori bar chart
const categoryChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: { bar: { columnWidth: '50%', borderRadius: 5, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: data.value?.categorySales.map(c => c.category) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#3b82f6', '#10b981'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
}))

const categorySeries = computed(() => [
  { name: 'Ciro', data: data.value?.categorySales.map(c => c.revenue) ?? [] },
  { name: 'Kâr',  data: data.value?.categorySales.map(c => c.profit) ?? [] },
])

const portfolioCount = (q: string) => portfolio.value?.products.filter(p => p.quadrant === q).length ?? 0
const quadrantBadge = (color: string) => ({
  emerald: 'bg-emerald-50 text-emerald-700',
  blue:    'bg-blue-50 text-blue-700',
  orange:  'bg-orange-50 text-orange-700',
  red:     'bg-red-50 text-red-700',
}[color] ?? 'bg-slate-100 text-slate-600')
</script>
