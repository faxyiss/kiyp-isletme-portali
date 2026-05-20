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
        <p class="text-2xl font-bold" :class="kpi.valueColor ?? 'text-slate-800'">{{ kpi.formatted }}</p>
        <p v-if="kpi.subtitle" class="text-xs text-slate-400 mt-1">{{ kpi.subtitle }}</p>
      </div>
    </div>

    <!-- Yeni Müşteri Trendi -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <h3 class="text-sm font-bold text-slate-700 mb-4">Son 12 Ay Yeni Müşteri Trendi</h3>
      <div v-if="loading" class="h-52 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!hasTrendData" class="h-52 flex items-center justify-center text-slate-400 text-sm">
        Henüz müşteri kaydı yok
      </div>
      <apexchart v-else type="bar" height="220" :options="trendChartOptions" :series="trendSeries" />
    </div>

    <!-- Alt iki tablo -->
    <div class="grid grid-cols-2 gap-4">
      <!-- En çok alışveriş yapan müşteriler -->
      <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
        <div class="px-6 py-4 border-b border-slate-100">
          <h3 class="text-sm font-bold text-slate-700">En Çok Alışveriş Yapan 10 Müşteri</h3>
          <p class="text-xs text-slate-400 mt-0.5">Seçili dönem içindeki satışlar</p>
        </div>
        <div v-if="loading" class="p-6 space-y-3">
          <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
        </div>
        <div v-else-if="!data?.topCustomers.length" class="p-8 text-center text-slate-400 text-sm">
          Bu dönemde müşterili satış yok
        </div>
        <table v-else class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-4 py-3 text-left">#</th>
              <th class="px-4 py-3 text-left">Müşteri</th>
              <th class="px-4 py-3 text-right">Sipariş</th>
              <th class="px-4 py-3 text-right">Toplam</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(c, i) in data.topCustomers" :key="i"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-4 py-3 text-slate-400 font-medium">{{ i + 1 }}</td>
              <td class="px-4 py-3">
                <p class="font-semibold text-slate-800 truncate max-w-32.5">{{ c.fullName }}</p>
                <p v-if="c.phone" class="text-xs text-slate-400">{{ c.phone }}</p>
              </td>
              <td class="px-4 py-3 text-right text-slate-600">{{ c.orderCount }}</td>
              <td class="px-4 py-3 text-right font-semibold text-slate-800">{{ formatCurrency(c.totalRevenue) }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Veresiye borçluları -->
      <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
        <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h3 class="text-sm font-bold text-slate-700">Veresiye Borçluları</h3>
            <p class="text-xs text-slate-400 mt-0.5">Bakiyeye göre sıralı</p>
          </div>
          <span v-if="data && data.debtCustomerCount > 0"
            class="px-2.5 py-1 rounded-lg text-xs font-semibold bg-orange-50 text-orange-600">
            {{ data.debtCustomerCount }} müşteri
          </span>
        </div>
        <div v-if="loading" class="p-6 space-y-3">
          <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
        </div>
        <div v-else-if="!data?.debtCustomers.length" class="p-8 text-center text-slate-400 text-sm">
          Veresiye borçlu müşteri yok
        </div>
        <table v-else class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-4 py-3 text-left">Müşteri</th>
              <th class="px-4 py-3 text-right">Bakiye</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(c, i) in data.debtCustomers" :key="i"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-4 py-3">
                <p class="font-semibold text-slate-800 truncate max-w-40">{{ c.fullName }}</p>
                <p v-if="c.phone" class="text-xs text-slate-400">{{ c.phone }}</p>
              </td>
              <td class="px-4 py-3 text-right">
                <span class="font-semibold text-orange-600">{{ formatCurrency(c.balance) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Veresiye Yaşlandırma Analizi -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <div class="flex items-center justify-between mb-4">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Veresiye Yaşlandırma Analizi</h3>
          <p class="text-xs text-slate-400 mt-0.5">En eski veresiye satış tarihine göre</p>
        </div>
        <span v-if="aging && aging.totalReceivables > 0"
          class="text-sm font-bold text-orange-600">
          {{ formatCurrency(aging.totalReceivables) }} toplam alacak
        </span>
      </div>
      <div v-if="agingLoading" class="h-20 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!aging || aging.totalReceivables === 0"
        class="h-20 flex items-center justify-center text-slate-400 text-sm">
        Açık veresiye alacak yok
      </div>
      <template v-else>
        <!-- Bucket kartları -->
        <div class="grid grid-cols-4 gap-3 mb-5">
          <div v-for="b in aging.buckets" :key="b.label"
            class="rounded-xl p-4 border"
            :class="agingBucketStyle(b.label)">
            <p class="text-xs font-semibold mb-1" :class="agingLabelColor(b.label)">{{ b.label }}</p>
            <p class="text-xl font-bold text-slate-800">{{ formatCurrency(b.amount) }}</p>
            <p class="text-xs text-slate-400 mt-0.5">{{ b.customerCount }} müşteri</p>
          </div>
        </div>
        <!-- Müşteri detay tablosu -->
        <table class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-4 py-2 text-left">Müşteri</th>
              <th class="px-4 py-2 text-right">Bakiye</th>
              <th class="px-4 py-2 text-right">Yaş (Gün)</th>
              <th class="px-4 py-2 text-left">Risk</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(c, i) in aging.customerDetails" :key="i"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-4 py-3">
                <p class="font-semibold text-slate-800">{{ c.fullName }}</p>
                <p v-if="c.phone" class="text-xs text-slate-400">{{ c.phone }}</p>
              </td>
              <td class="px-4 py-3 text-right font-bold text-orange-600">{{ formatCurrency(c.balance) }}</td>
              <td class="px-4 py-3 text-right text-slate-600">{{ c.daysOld }} gün</td>
              <td class="px-4 py-3">
                <span class="px-2 py-0.5 rounded text-xs font-semibold"
                  :class="agingRiskBadge(c.bucket)">{{ c.bucket }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </template>
    </div>

    <!-- RFM Müşteri Segmentasyonu -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <div class="flex items-center justify-between mb-4">
        <div>
          <h3 class="text-sm font-bold text-slate-700">RFM Müşteri Segmentasyonu</h3>
          <p class="text-xs text-slate-400 mt-0.5">Recency · Frequency · Monetary — tüm zamanlar</p>
        </div>
      </div>
      <div v-if="rfmLoading" class="h-20 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!rfm || rfm.totalCustomers === 0"
        class="h-20 flex items-center justify-center text-slate-400 text-sm">
        Müşterili satış kaydı yok
      </div>
      <template v-else>
        <!-- Segment sayaçları -->
        <div class="grid grid-cols-4 gap-3 mb-5">
          <div class="bg-emerald-50 rounded-xl p-4 text-center">
            <p class="text-2xl font-bold text-emerald-700">{{ rfm.champions }}</p>
            <p class="text-xs font-semibold text-emerald-600 mt-1">Şampiyon</p>
            <p class="text-xs text-slate-400">Yüksek R·F·M</p>
          </div>
          <div class="bg-blue-50 rounded-xl p-4 text-center">
            <p class="text-2xl font-bold text-blue-700">{{ rfm.loyal }}</p>
            <p class="text-xs font-semibold text-blue-600 mt-1">Sadık</p>
            <p class="text-xs text-slate-400">Sık alışveriş</p>
          </div>
          <div class="bg-orange-50 rounded-xl p-4 text-center">
            <p class="text-2xl font-bold text-orange-700">{{ rfm.atRisk }}</p>
            <p class="text-xs font-semibold text-orange-600 mt-1">Risk Altında</p>
            <p class="text-xs text-slate-400">Uzun süredir yok</p>
          </div>
          <div class="bg-red-50 rounded-xl p-4 text-center">
            <p class="text-2xl font-bold text-red-700">{{ rfm.lost }}</p>
            <p class="text-xs font-semibold text-red-600 mt-1">Kayıp</p>
            <p class="text-xs text-slate-400">Düşük R·F·M</p>
          </div>
        </div>
        <!-- RFM müşteri tablosu -->
        <table class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-4 py-2 text-left">Müşteri</th>
              <th class="px-4 py-2 text-center">R</th>
              <th class="px-4 py-2 text-center">F</th>
              <th class="px-4 py-2 text-center">M</th>
              <th class="px-4 py-2 text-right">Toplam</th>
              <th class="px-4 py-2 text-right">Son Alış</th>
              <th class="px-4 py-2 text-left">Segment</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(c, i) in rfm.customers" :key="i"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
              <td class="px-4 py-3">
                <p class="font-semibold text-slate-800">{{ c.fullName }}</p>
                <p v-if="c.phone" class="text-xs text-slate-400">{{ c.phone }}</p>
              </td>
              <td class="px-4 py-3 text-center">
                <span class="font-bold" :class="scoreColor(c.rScore)">{{ c.rScore }}</span>
              </td>
              <td class="px-4 py-3 text-center">
                <span class="font-bold" :class="scoreColor(c.fScore)">{{ c.fScore }}</span>
              </td>
              <td class="px-4 py-3 text-center">
                <span class="font-bold" :class="scoreColor(c.mScore)">{{ c.mScore }}</span>
              </td>
              <td class="px-4 py-3 text-right font-semibold text-slate-800">{{ formatCurrency(c.totalRevenue) }}</td>
              <td class="px-4 py-3 text-right text-xs text-slate-400">{{ formatDate(c.lastOrderDate) }}</td>
              <td class="px-4 py-3">
                <span class="px-2 py-0.5 rounded-md text-xs font-bold" :class="rfmBadge(c.segmentColor)">
                  {{ c.segment }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </template>
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Müşteri verileri yüklenirken hata oluştu.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { UsersIcon, AlertCircleIcon, CreditCardIcon, ShoppingCartIcon } from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface TopCustomerItem {
  fullName: string
  phone: string | null
  totalRevenue: number
  orderCount: number
  avgOrder: number
}

interface DebtCustomerItem {
  fullName: string
  phone: string | null
  balance: number
  createdAt: string
}

interface CustomerData {
  totalCustomers: number
  debtCustomerCount: number
  totalReceivables: number
  avgOrderValue: number
  topCustomers: TopCustomerItem[]
  debtCustomers: DebtCustomerItem[]
  newCustomerTrend: { label: string; count: number }[]
}

interface AgingBucket { label: string; amount: number; customerCount: number }
interface AgingCustomerDetail { fullName: string; phone: string | null; balance: number; daysOld: number; bucket: string }
interface AgingData {
  totalReceivables: number
  buckets: AgingBucket[]
  customerDetails: AgingCustomerDetail[]
}

interface RFMCustomer {
  fullName: string; phone: string | null
  rScore: number; fScore: number; mScore: number; totalScore: number
  segment: string; segmentColor: string
  totalRevenue: number; orderCount: number; lastOrderDate: string
}
interface RFMData {
  totalCustomers: number; champions: number; loyal: number; atRisk: number; lost: number
  customers: RFMCustomer[]
}

const data = ref<CustomerData | null>(null)
const aging = ref<AgingData | null>(null)
const rfm = ref<RFMData | null>(null)
const loading = ref(true)
const agingLoading = ref(true)
const rfmLoading = ref(true)
const error = ref(false)

const fetchData = async () => {
  loading.value = true; error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/customers?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (!res.ok) throw new Error()
    data.value = await res.json()
  } catch { error.value = true }
  finally { loading.value = false }
}

const fetchAging = async () => {
  agingLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/receivables-aging`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) aging.value = await res.json()
  } catch { /* sessiz */ }
  finally { agingLoading.value = false }
}

const fetchRFM = async () => {
  rfmLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/rfm`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) rfm.value = await res.json()
  } catch { /* sessiz */ }
  finally { rfmLoading.value = false }
}

onMounted(() => { fetchData(); fetchAging(); fetchRFM() })
watch(() => props.range, fetchData)

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(v ?? 0)
const formatNum = (v: number) =>
  new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(v ?? 0)
const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

// Yaşlandırma yardımcı fonksiyonları
const agingBucketStyle = (label: string) => {
  if (label === '90+ Gün') return 'bg-red-50 border-red-200'
  if (label === '61–90 Gün') return 'bg-orange-50 border-orange-200'
  if (label === '31–60 Gün') return 'bg-yellow-50 border-yellow-200'
  return 'bg-emerald-50 border-emerald-200'
}
const agingLabelColor = (label: string) => {
  if (label === '90+ Gün') return 'text-red-600'
  if (label === '61–90 Gün') return 'text-orange-600'
  if (label === '31–60 Gün') return 'text-yellow-700'
  return 'text-emerald-700'
}
const agingRiskBadge = (bucket: string) => {
  if (bucket === '90+ Gün') return 'bg-red-100 text-red-700'
  if (bucket === '61–90 Gün') return 'bg-orange-100 text-orange-700'
  if (bucket === '31–60 Gün') return 'bg-yellow-100 text-yellow-700'
  return 'bg-emerald-100 text-emerald-700'
}

// RFM yardımcı fonksiyonları
const scoreColor = (s: number) =>
  s === 3 ? 'text-emerald-600' : s === 2 ? 'text-blue-500' : 'text-red-500'

const rfmBadge = (color: string): string => {
  const map: Record<string, string> = {
    emerald: 'bg-emerald-100 text-emerald-800',
    blue:    'bg-blue-100 text-blue-800',
    sky:     'bg-sky-100 text-sky-800',
    violet:  'bg-violet-100 text-violet-800',
    orange:  'bg-orange-100 text-orange-800',
    red:     'bg-red-100 text-red-800',
  }
  return map[color] ?? 'bg-slate-100 text-slate-700'
}

const kpiCards = computed(() => [
  {
    label: 'Toplam Müşteri',
    formatted: formatNum(data.value?.totalCustomers ?? 0),
    subtitle: 'Kayıtlı müşteri',
    icon: UsersIcon,
    iconBg: 'bg-blue-50',
    iconColor: 'text-blue-500',
  },
  {
    label: 'Veresiyeli',
    formatted: formatNum(data.value?.debtCustomerCount ?? 0),
    subtitle: 'Borçlu müşteri',
    icon: AlertCircleIcon,
    iconBg: (data.value?.debtCustomerCount ?? 0) > 0 ? 'bg-orange-50' : 'bg-emerald-50',
    iconColor: (data.value?.debtCustomerCount ?? 0) > 0 ? 'text-orange-500' : 'text-emerald-500',
    valueColor: (data.value?.debtCustomerCount ?? 0) > 0 ? 'text-orange-600' : 'text-emerald-600',
  },
  {
    label: 'Toplam Alacak',
    formatted: formatCurrency(data.value?.totalReceivables ?? 0),
    subtitle: 'Açık veresiye bakiyesi',
    icon: CreditCardIcon,
    iconBg: 'bg-red-50',
    iconColor: 'text-red-500',
    valueColor: (data.value?.totalReceivables ?? 0) > 0 ? 'text-red-600' : 'text-slate-800',
  },
  {
    label: 'Ort. Sepet',
    formatted: formatCurrency(data.value?.avgOrderValue ?? 0),
    subtitle: 'Dönem ortalaması',
    icon: ShoppingCartIcon,
    iconBg: 'bg-violet-50',
    iconColor: 'text-violet-500',
  },
])

const hasTrendData = computed(() => data.value?.newCustomerTrend.some(m => m.count > 0))

const trendChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: { bar: { columnWidth: '50%', borderRadius: 5, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: data.value?.newCustomerTrend.map(m => m.label) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => Math.round(v).toString(),
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#3b82f6'],
  tooltip: { y: { formatter: (v: number) => `${v} müşteri` } },
}))

const trendSeries = computed(() => [
  { name: 'Yeni Müşteri', data: data.value?.newCustomerTrend.map(m => m.count) ?? [] },
])
</script>
