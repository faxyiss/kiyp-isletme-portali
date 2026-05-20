<template>
  <div class="space-y-6">
    <!-- KPI Kartları (5 adet) -->
    <div v-if="loading" class="grid grid-cols-5 gap-4">
      <div v-for="i in 5" :key="i" class="bg-white rounded-2xl border border-slate-200 p-5 animate-pulse">
        <div class="h-3 bg-slate-100 rounded w-24 mb-3"></div>
        <div class="h-7 bg-slate-100 rounded w-28"></div>
      </div>
    </div>
    <div v-else class="grid grid-cols-5 gap-4">
      <div v-for="kpi in kpiCards" :key="kpi.label"
        class="bg-white rounded-2xl border border-slate-200 p-5 hover:shadow-md transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <span class="text-xs font-semibold text-slate-400 uppercase tracking-wide">{{ kpi.label }}</span>
          <div class="w-9 h-9 rounded-xl flex items-center justify-center" :class="kpi.iconBg">
            <component :is="kpi.icon" :size="18" :class="kpi.iconColor" />
          </div>
        </div>
        <p class="text-2xl font-bold text-slate-800">{{ kpi.formatted }}</p>
        <p v-if="kpi.subtitle" class="text-xs text-slate-400 mt-1">{{ kpi.subtitle }}</p>
      </div>
    </div>

    <!-- Günlük trend + Donut -->
    <div class="grid grid-cols-3 gap-4">
      <!-- Günlük üretim trendi -->
      <div class="col-span-2 bg-white rounded-2xl border border-slate-200 p-6">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-sm font-bold text-slate-700">Günlük Üretim Trendi</h3>
          <div class="flex gap-1 text-xs">
            <button @click="dailyView = 'qty'"
              class="px-2.5 py-1 rounded-lg font-medium transition-colors"
              :class="dailyView === 'qty' ? 'bg-teal-50 text-teal-700' : 'text-slate-400 hover:text-slate-600'">
              Miktar
            </button>
            <button @click="dailyView = 'cost'"
              class="px-2.5 py-1 rounded-lg font-medium transition-colors"
              :class="dailyView === 'cost' ? 'bg-violet-50 text-violet-700' : 'text-slate-400 hover:text-slate-600'">
              Maliyet
            </button>
          </div>
        </div>
        <div v-if="loading" class="h-56 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.dailyProduction.length"
          class="h-56 flex items-center justify-center text-slate-400 text-sm">
          Bu dönemde üretim yok
        </div>
        <apexchart v-else type="area" height="230" :options="dailyChartOptions" :series="dailySeries" />
      </div>

      <!-- Maliyet dağılımı donut -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Ürün Maliyet Dağılımı</h3>
        <div v-if="loading" class="h-56 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.productSummary.length"
          class="h-56 flex items-center justify-center text-slate-400 text-sm">Veri yok</div>
        <apexchart v-else type="donut" height="230" :options="costDonutOptions" :series="costDonutSeries" />
      </div>
    </div>

    <!-- Aylık trend çift Y ekseni -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <h3 class="text-sm font-bold text-slate-700 mb-4">Son 12 Ay Üretim Trendi
        <span class="text-slate-400 font-normal text-xs ml-2">Miktar (sol) · Maliyet (sağ)</span>
      </h3>
      <div v-if="loading" class="h-52 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!hasMonthlyData"
        class="h-52 flex items-center justify-center text-slate-400 text-sm">Veri yok</div>
      <apexchart v-else type="line" height="220" :options="monthlyChartOptions" :series="monthlySeries" />
    </div>

    <!-- Hammadde tüketimi + Ürün tablosu -->
    <div class="grid grid-cols-5 gap-4">
      <!-- Yatay bar: top hammadde tüketimi -->
      <div class="col-span-2 bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">En Çok Tüketilen Hammaddeler</h3>
        <div v-if="loading" class="h-56 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.rawMaterialConsumption.length"
          class="h-56 flex items-center justify-center text-slate-400 text-sm">
          Reçete verisi bulunamadı
        </div>
        <apexchart v-else type="bar" height="230" :options="rawMaterialChartOptions" :series="rawMaterialSeries" />
      </div>

      <!-- Ürün bazlı özet tablo -->
      <div class="col-span-3 bg-white rounded-2xl border border-slate-200 overflow-hidden">
        <div class="px-6 py-4 border-b border-slate-100">
          <h3 class="text-sm font-bold text-slate-700">Ürün Bazlı Üretim Özeti</h3>
        </div>
        <div v-if="loading" class="p-6 space-y-3">
          <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
        </div>
        <div v-else-if="!data?.productSummary.length" class="p-8 text-center text-slate-400 text-sm">
          Bu dönemde üretim kaydı yok
        </div>
        <table v-else class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-4 py-3 text-left">Ürün</th>
              <th class="px-4 py-3 text-right">Miktar</th>
              <th class="px-4 py-3 text-right">Top. Maliyet</th>
              <th class="px-4 py-3 text-right">Ort. Birim</th>
              <th class="px-4 py-3 text-right">Seans</th>
              <th class="px-4 py-3 text-right">Son Üretim</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(p, i) in data.productSummary" :key="i"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-4 py-3 font-semibold text-slate-800">{{ p.productName }}</td>
              <td class="px-4 py-3 text-right text-slate-700 font-medium">{{ formatNum(p.totalQuantity) }}</td>
              <td class="px-4 py-3 text-right font-semibold text-slate-800">{{ formatCurrency(p.totalCost) }}</td>
              <td class="px-4 py-3 text-right text-slate-600">{{ formatCurrency(p.avgUnitCost) }}</td>
              <td class="px-4 py-3 text-right">
                <span class="px-2 py-0.5 rounded bg-teal-50 text-teal-700 text-xs font-semibold">
                  {{ p.batchCount }}
                </span>
              </td>
              <td class="px-4 py-3 text-right text-slate-400 text-xs">{{ formatDate(p.lastProducedAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Hammadde tüketim tam tablo -->
    <div v-if="data?.rawMaterialConsumption.length" class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
        <h3 class="text-sm font-bold text-slate-700">Hammadde Tüketim Detayı</h3>
        <span class="text-xs text-slate-400">Reçeteden hesaplanmış tahmini tüketim</span>
      </div>
      <div class="grid grid-cols-4 divide-x divide-slate-100">
        <div v-for="(m, i) in data.rawMaterialConsumption" :key="i"
          class="px-5 py-3 flex items-center justify-between hover:bg-slate-50 transition-colors">
          <div>
            <p class="text-sm font-semibold text-slate-700 truncate">{{ m.materialName }}</p>
          </div>
          <span class="text-sm font-bold text-teal-600 ml-3 shrink-0">{{ formatNum(m.consumedQty) }}</span>
        </div>
      </div>
    </div>

    <!-- Üretim Karlılık Marjı -->
    <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Ürün Bazlı Karlılık Marjı</h3>
          <p class="text-xs text-slate-400 mt-0.5">Üretim maliyeti vs satış geliri karşılaştırması</p>
        </div>
        <div v-if="!profLoading && profitability" class="flex gap-3">
          <div class="text-right">
            <p class="text-xs text-slate-400">Genel Marj</p>
            <p class="text-lg font-bold"
              :class="profitability.overallMargin >= 20 ? 'text-emerald-600' : profitability.overallMargin >= 10 ? 'text-blue-600' : 'text-orange-600'">
              %{{ profitability.overallMargin }}
            </p>
          </div>
        </div>
      </div>

      <div v-if="profLoading" class="p-6 space-y-3">
        <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="!profitability?.products.length"
        class="p-8 text-center text-slate-400 text-sm">
        Bu dönemde üretim veya satış verisi yok
      </div>
      <div v-else>
        <!-- Özet bar chart -->
        <div class="p-6 border-b border-slate-100">
          <apexchart type="bar" height="200" :options="profChartOptions" :series="profSeries" />
        </div>
        <!-- Tablo -->
        <table class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-6 py-3 text-left">Ürün</th>
              <th class="px-6 py-3 text-right">Ürt. Maliyet</th>
              <th class="px-6 py-3 text-right">Satış Geliri</th>
              <th class="px-6 py-3 text-right">Brüt Kâr</th>
              <th class="px-6 py-3 text-right">Marj</th>
              <th class="px-6 py-3 text-right">Ürt. Qty</th>
              <th class="px-6 py-3 text-right">Sat. Qty</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in profitability.products" :key="p.productName"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-6 py-3 font-semibold text-slate-800">{{ p.productName }}</td>
              <td class="px-6 py-3 text-right text-violet-600">{{ formatCurrency(p.productionCost) }}</td>
              <td class="px-6 py-3 text-right text-blue-600">{{ formatCurrency(p.salesRevenue) }}</td>
              <td class="px-6 py-3 text-right font-semibold"
                :class="p.salesProfit >= 0 ? 'text-emerald-600' : 'text-red-600'">
                {{ formatCurrency(p.salesProfit) }}
              </td>
              <td class="px-6 py-3 text-right">
                <span class="px-2 py-0.5 rounded text-xs font-semibold"
                  :class="p.grossMarginPercent >= 20 ? 'bg-emerald-50 text-emerald-700' : p.grossMarginPercent >= 10 ? 'bg-blue-50 text-blue-700' : p.grossMarginPercent >= 0 ? 'bg-orange-50 text-orange-700' : 'bg-red-50 text-red-700'">
                  %{{ p.grossMarginPercent }}
                </span>
              </td>
              <td class="px-6 py-3 text-right text-slate-500">{{ formatNum(p.productionQty) }}</td>
              <td class="px-6 py-3 text-right text-slate-500">{{ formatNum(p.soldQty) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Hammadde Fiyat Trendi -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <div class="mb-4">
        <h3 class="text-sm font-bold text-slate-700">Hammadde Fiyat Trendi — Son 6 Ay</h3>
        <p class="text-xs text-slate-400 mt-0.5">Baz aya göre yüzdelik fiyat değişimi</p>
      </div>

      <div v-if="priceLoading" class="h-56 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!priceTrend?.series.length"
        class="h-56 flex items-center justify-center text-slate-400 text-sm">
        Son 6 ayda stok girişi yok
      </div>
      <apexchart v-else type="line" height="240" :options="priceChartOptions" :series="priceSeries" />
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Üretim verileri yüklenirken hata oluştu.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { LayersIcon, DollarSignIcon, PackageIcon, RepeatIcon, ActivityIcon } from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface ProductSummary {
  productName: string
  totalQuantity: number
  totalCost: number
  avgUnitCost: number
  batchCount: number
  lastProducedAt: string
}

interface ProductionData {
  totalQuantity: number
  totalCost: number
  avgUnitCost: number
  uniqueProductCount: number
  totalBatches: number
  productSummary: ProductSummary[]
  monthlyTrend: { label: string; quantity: number; cost: number }[]
  dailyProduction: { date: string; quantity: number; cost: number; batchCount: number }[]
  rawMaterialConsumption: { materialName: string; consumedQty: number }[]
}

interface ProductProfitability {
  productName: string; categoryName: string
  productionCost: number; salesRevenue: number; salesProfit: number
  grossMarginPercent: number; productionQty: number; soldQty: number
}
interface ProfitabilityData {
  totalProductionCost: number; totalSalesRevenue: number
  totalProfit: number; overallMargin: number
  products: ProductProfitability[]
}

interface MaterialPricePoint {
  label: string; avgPrice: number; minPrice: number; maxPrice: number; inflowCount: number
}
interface MaterialPriceSeries { materialName: string; points: MaterialPricePoint[] }
interface PriceTrendData { labels: string[]; series: MaterialPriceSeries[] }

const data = ref<ProductionData | null>(null)
const profitability = ref<ProfitabilityData | null>(null)
const priceTrend = ref<PriceTrendData | null>(null)
const loading = ref(true)
const profLoading = ref(true)
const priceLoading = ref(true)
const error = ref(false)
const dailyView = ref<'qty' | 'cost'>('qty')

const fetchData = async () => {
  loading.value = true
  error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/production?range=${props.range}`, {
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

const fetchProfitability = async () => {
  profLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/production-profitability?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) profitability.value = await res.json()
  } catch { /* sessiz */ }
  finally { profLoading.value = false }
}

const fetchPriceTrend = async () => {
  priceLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/material-price-trend`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) priceTrend.value = await res.json()
  } catch { /* sessiz */ }
  finally { priceLoading.value = false }
}

onMounted(() => { fetchData(); fetchProfitability(); fetchPriceTrend() })
watch(() => props.range, () => { fetchData(); fetchProfitability() })

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 2 }).format(v ?? 0)
const formatNum = (v: number) =>
  new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(v ?? 0)
const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

const kpiCards = computed(() => [
  {
    label: 'Toplam Üretim',
    formatted: formatNum(data.value?.totalQuantity ?? 0),
    subtitle: 'Üretilen adet',
    icon: LayersIcon,
    iconBg: 'bg-teal-50',
    iconColor: 'text-teal-500',
  },
  {
    label: 'Toplam Maliyet',
    formatted: formatCurrency(data.value?.totalCost ?? 0),
    subtitle: 'Hammadde maliyeti',
    icon: DollarSignIcon,
    iconBg: 'bg-violet-50',
    iconColor: 'text-violet-500',
  },
  {
    label: 'Ort. Birim Maliyet',
    formatted: formatCurrency(data.value?.avgUnitCost ?? 0),
    subtitle: 'Maliyet / adet',
    icon: ActivityIcon,
    iconBg: 'bg-blue-50',
    iconColor: 'text-blue-500',
  },
  {
    label: 'Ürün Çeşidi',
    formatted: String(data.value?.uniqueProductCount ?? 0),
    subtitle: 'Farklı ürün',
    icon: PackageIcon,
    iconBg: 'bg-emerald-50',
    iconColor: 'text-emerald-500',
  },
  {
    label: 'Üretim Seansı',
    formatted: String(data.value?.totalBatches ?? 0),
    subtitle: 'Üretim kaydı',
    icon: RepeatIcon,
    iconBg: 'bg-orange-50',
    iconColor: 'text-orange-500',
  },
])

const hasMonthlyData = computed(() =>
  data.value?.monthlyTrend.some(m => m.quantity > 0 || m.cost > 0)
)

// Günlük trend chart (toggle: miktar veya maliyet)
const dailyChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  stroke: { curve: 'smooth', width: 2 },
  fill: { type: 'gradient', gradient: { opacityFrom: 0.3, opacityTo: 0.05 } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: data.value?.dailyProduction.map(d => d.date) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '11px' }, rotate: -30 },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: dailyView.value === 'cost'
        ? (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v)
        : (v: number) => formatNum(v),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: dailyView.value === 'qty' ? ['#14b8a6'] : ['#8b5cf6'],
  tooltip: {
    y: {
      formatter: dailyView.value === 'cost'
        ? (v: number) => formatCurrency(v)
        : (v: number) => `${formatNum(v)} adet`,
    },
  },
}))

const dailySeries = computed(() => [
  dailyView.value === 'qty'
    ? { name: 'Üretim Miktarı', data: data.value?.dailyProduction.map(d => d.quantity) ?? [] }
    : { name: 'Üretim Maliyeti', data: data.value?.dailyProduction.map(d => d.cost) ?? [] },
])

// Aylık trend — çift Y ekseni
const monthlyChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  stroke: { width: [0, 3], curve: 'smooth' },
  dataLabels: { enabled: false },
  xaxis: {
    categories: data.value?.monthlyTrend.map(m => m.label) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: [
    {
      seriesName: 'Miktar',
      labels: {
        style: { colors: '#14b8a6', fontSize: '11px' },
        formatter: (v: number) => formatNum(v),
      },
    },
    {
      seriesName: 'Maliyet',
      opposite: true,
      labels: {
        style: { colors: '#8b5cf6', fontSize: '11px' },
        formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
      },
    },
  ],
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#14b8a6', '#8b5cf6'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  plotOptions: { bar: { columnWidth: '50%', borderRadius: 4 } },
  tooltip: {
    y: [
      { formatter: (v: number) => `${formatNum(v)} adet` },
      { formatter: (v: number) => formatCurrency(v) },
    ],
  },
}))

const monthlySeries = computed(() => [
  { name: 'Miktar', type: 'bar', data: data.value?.monthlyTrend.map(m => m.quantity) ?? [] },
  { name: 'Maliyet', type: 'line', data: data.value?.monthlyTrend.map(m => m.cost) ?? [] },
])

// Maliyet donut
const costDonutOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  labels: data.value?.productSummary.map(p => p.productName) ?? [],
  colors: ['#14b8a6', '#3b82f6', '#8b5cf6', '#f97316', '#f43f5e', '#10b981', '#eab308'],
  legend: { position: 'bottom', labels: { colors: '#64748b' }, fontSize: '11px' },
  dataLabels: { enabled: false },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
  plotOptions: { pie: { donut: { size: '60%' } } },
}))

const costDonutSeries = computed(() =>
  data.value?.productSummary.map(p => p.totalCost) ?? []
)

// Yatay bar: top 8 hammadde
const rawMaterialChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: {
    bar: {
      horizontal: true,
      barHeight: '60%',
      borderRadius: 4,
      borderRadiusApplication: 'end',
    },
  },
  dataLabels: { enabled: false },
  xaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => formatNum(v),
    },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: {
    labels: { style: { colors: '#64748b', fontSize: '11px' } },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4, xaxis: { lines: { show: true } }, yaxis: { lines: { show: false } } },
  colors: ['#f97316'],
  tooltip: { x: { show: false }, y: { formatter: (v: number) => `${formatNum(v)} birim` } },
}))

const rawMaterialSeries = computed(() => [{
  name: 'Tüketim',
  data: (data.value?.rawMaterialConsumption ?? [])
    .slice(0, 8)
    .map(m => ({ x: m.materialName, y: m.consumedQty })),
}])

// Karlılık bar chart
const profChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: { bar: { columnWidth: '55%', borderRadius: 4, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: profitability.value?.products.map(p => p.productName) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '11px' }, rotate: -20 },
    axisBorder: { show: false }, axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#8b5cf6', '#3b82f6', '#10b981'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
}))

const profSeries = computed(() => [
  { name: 'Üretim Maliyeti', data: profitability.value?.products.map(p => p.productionCost) ?? [] },
  { name: 'Satış Geliri',   data: profitability.value?.products.map(p => p.salesRevenue) ?? [] },
  { name: 'Brüt Kâr',      data: profitability.value?.products.map(p => p.salesProfit) ?? [] },
])

// Hammadde fiyat trendi
const priceColors = ['#3b82f6', '#10b981', '#f97316', '#8b5cf6', '#f43f5e', '#14b8a6']

const priceChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  stroke: { curve: 'smooth', width: 2.5 },
  dataLabels: { enabled: false },
  markers: { size: 5 },
  xaxis: {
    categories: priceTrend.value?.labels ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false }, axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => (v >= 0 ? '+' : '') + v.toFixed(1) + '%',
    },
  },
  annotations: {
    yaxis: [{
      y: 0,
      borderColor: '#cbd5e1',
      borderWidth: 1.5,
      strokeDashArray: 5,
      label: {
        text: 'baz',
        position: 'left',
        offsetX: 8,
        style: { color: '#94a3b8', fontSize: '10px', background: 'transparent', cssClass: 'apexcharts-yaxis-annotation-label' },
      },
    }],
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: priceColors,
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: {
    y: {
      formatter: (v: number | null) =>
        v != null ? (v >= 0 ? '+' : '') + v.toFixed(2) + '%' : '—',
    },
  },
}))

const priceSeries = computed(() =>
  (priceTrend.value?.series ?? []).map(s => {
    const prices = s.points.map(p => p.avgPrice)
    const base = prices.find(p => p > 0) ?? 1
    return {
      name: s.materialName,
      data: prices.map(p => p > 0 ? parseFloat(((p - base) / base * 100).toFixed(2)) : null),
    }
  })
)
</script>
