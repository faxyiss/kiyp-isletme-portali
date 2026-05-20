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
        <p v-if="kpi.subtitle" class="text-xs text-slate-400 mt-1">{{ kpi.subtitle }}</p>
      </div>
    </div>

    <!-- Grafik + Donut yan yana -->
    <div class="grid grid-cols-3 gap-4">
      <!-- Aylık gider trendi -->
      <div class="col-span-2 bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Son 12 Ay Gider Trendi</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!hasTrendData" class="h-64 flex items-center justify-center text-slate-400 text-sm">
          Bu dönemde gider verisi yok
        </div>
        <apexchart v-else type="bar" height="260" :options="trendChartOptions" :series="trendSeries" />
      </div>

      <!-- Kategori dağılımı -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Kategori Dağılımı</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.categoryBreakdown.length" class="h-64 flex items-center justify-center text-slate-400 text-sm">
          Veri yok
        </div>
        <apexchart v-else type="donut" height="260" :options="donutChartOptions" :series="donutSeries" />
      </div>
    </div>

    <!-- En yüksek 10 gider tablosu -->
    <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100">
        <h3 class="text-sm font-bold text-slate-700">En Yüksek 10 Gider</h3>
        <p class="text-xs text-slate-400 mt-0.5">Seçili dönem içindeki giderler</p>
      </div>
      <div v-if="loading" class="p-6 space-y-3">
        <div v-for="i in 6" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="!data?.topExpenses.length" class="p-8 text-center text-slate-400 text-sm">
        Bu dönemde gider kaydı yok
      </div>
      <table v-else class="w-full text-sm">
        <thead>
          <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
            <th class="px-6 py-3 text-left">#</th>
            <th class="px-6 py-3 text-left">Gider Başlığı</th>
            <th class="px-6 py-3 text-left">Kategori</th>
            <th class="px-6 py-3 text-left">Tür</th>
            <th class="px-6 py-3 text-right">Tutar</th>
            <th class="px-6 py-3 text-right">Tarih</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(e, i) in data.topExpenses" :key="i"
            class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
            <td class="px-6 py-4 text-slate-400 font-medium">{{ i + 1 }}</td>
            <td class="px-6 py-4 font-semibold text-slate-800">{{ e.title }}</td>
            <td class="px-6 py-4 text-slate-500">{{ e.category }}</td>
            <td class="px-6 py-4">
              <span class="px-2 py-0.5 rounded text-xs font-medium" :class="typeColor(e.type)">{{ e.type }}</span>
            </td>
            <td class="px-6 py-4 text-right font-semibold text-red-600">{{ formatCurrency(e.amount) }}</td>
            <td class="px-6 py-4 text-right text-slate-400 text-xs">{{ formatDate(e.date) }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Gider Oranı Analizi -->
    <div class="bg-white rounded-2xl border border-slate-200 p-6">
      <div class="flex items-center justify-between mb-5">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Gider Oranı Analizi — Son 12 Ay</h3>
          <p class="text-xs text-slate-400 mt-0.5">Toplam çıkışın ciro içindeki ağırlığı</p>
        </div>
        <div v-if="!ratioLoading && ratio" class="flex gap-3">
          <div class="text-right">
            <p class="text-xs text-slate-400">Ort. Gider Oranı</p>
            <p class="text-lg font-bold" :class="ratio.avgExpenseRatio > 80 ? 'text-red-600' : ratio.avgExpenseRatio > 60 ? 'text-orange-600' : 'text-emerald-600'">
              %{{ ratio.avgExpenseRatio }}
            </p>
          </div>
          <div class="text-right">
            <p class="text-xs text-slate-400">Faaliyet Marjı</p>
            <p class="text-lg font-bold" :class="ratio.avgOperatingMargin >= 0 ? 'text-emerald-600' : 'text-red-600'">
              %{{ ratio.avgOperatingMargin }}
            </p>
          </div>
        </div>
      </div>

      <div v-if="ratioLoading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
      <div v-else-if="!ratio?.monthly.some(m => m.revenue > 0)"
        class="h-64 flex items-center justify-center text-slate-400 text-sm">
        Henüz veri yok
      </div>
      <apexchart v-else type="bar" height="260" :options="ratioChartOptions" :series="ratioSeries" />

      <!-- Aylık tablo -->
      <div v-if="!ratioLoading && ratio" class="mt-5 overflow-x-auto">
        <table class="w-full text-xs">
          <thead>
            <tr class="text-slate-400 font-semibold uppercase tracking-wide border-b border-slate-100">
              <th class="pb-2 text-left">Ay</th>
              <th class="pb-2 text-right">Ciro</th>
              <th class="pb-2 text-right">Gider</th>
              <th class="pb-2 text-right">Personel</th>
              <th class="pb-2 text-right">Toplam Çıkış</th>
              <th class="pb-2 text-right">Gider Oranı</th>
              <th class="pb-2 text-right">Faaliyet Kârı</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in ratio.monthly" :key="m.label"
              class="border-b border-slate-50 hover:bg-slate-50 transition-colors">
              <td class="py-2 font-medium text-slate-700">{{ m.label }}</td>
              <td class="py-2 text-right text-slate-600">{{ formatCurrency(m.revenue) }}</td>
              <td class="py-2 text-right text-orange-600">{{ formatCurrency(m.expenses) }}</td>
              <td class="py-2 text-right text-violet-600">{{ formatCurrency(m.payroll) }}</td>
              <td class="py-2 text-right font-semibold text-slate-700">{{ formatCurrency(m.totalCost) }}</td>
              <td class="py-2 text-right">
                <span v-if="m.revenue > 0" class="font-semibold"
                  :class="m.ratio > 80 ? 'text-red-600' : m.ratio > 60 ? 'text-orange-600' : 'text-emerald-600'">
                  %{{ m.ratio }}
                </span>
                <span v-else class="text-slate-300">—</span>
              </td>
              <td class="py-2 text-right font-semibold"
                :class="m.operatingProfit >= 0 ? 'text-emerald-600' : 'text-red-600'">
                {{ formatCurrency(m.operatingProfit) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Gider verileri yüklenirken hata oluştu.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { WalletIcon, UsersIcon, ReceiptIcon, ListIcon } from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface ExpenseData {
  totalExpenses: number
  totalPayroll: number
  totalCombined: number
  expenseCount: number
  categoryBreakdown: { category: string; amount: number }[]
  monthlyTrend: { label: string; expenses: number; payroll: number }[]
  topExpenses: { title: string; category: string; type: string; amount: number; date: string }[]
}

interface ExpenseRatioMonth {
  label: string; revenue: number; expenses: number; payroll: number
  totalCost: number; ratio: number; operatingProfit: number
}
interface ExpenseRatioData {
  avgExpenseRatio: number; avgOperatingMargin: number
  totalRevenue: number; totalExpenses: number
  monthly: ExpenseRatioMonth[]
}

const data = ref<ExpenseData | null>(null)
const ratio = ref<ExpenseRatioData | null>(null)
const loading = ref(true)
const ratioLoading = ref(true)
const error = ref(false)

const fetchData = async () => {
  loading.value = true
  error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/expenses?range=${props.range}`, {
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

const fetchRatio = async () => {
  ratioLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/expense-ratio`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) ratio.value = await res.json()
  } catch { /* sessiz */ }
  finally { ratioLoading.value = false }
}

onMounted(() => { fetchData(); fetchRatio() })
watch(() => props.range, fetchData)

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(v ?? 0)
const formatNum = (v: number) =>
  new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(v ?? 0)
const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

const typeColor = (type: string) => {
  if (type === 'Aylık Sabit') return 'bg-blue-50 text-blue-700'
  if (type === 'Dönemsel') return 'bg-violet-50 text-violet-700'
  return 'bg-slate-100 text-slate-600'
}

const kpiCards = computed(() => [
  {
    label: 'Toplam Gider',
    formatted: formatCurrency(data.value?.totalExpenses ?? 0),
    subtitle: 'Kayıtlı gider kalemleri',
    icon: WalletIcon,
    iconBg: 'bg-orange-50',
    iconColor: 'text-orange-500',
  },
  {
    label: 'Personel Maliyeti',
    formatted: formatCurrency(data.value?.totalPayroll ?? 0),
    subtitle: 'İşveren toplam yükü',
    icon: UsersIcon,
    iconBg: 'bg-violet-50',
    iconColor: 'text-violet-500',
  },
  {
    label: 'Toplam Çıkış',
    formatted: formatCurrency(data.value?.totalCombined ?? 0),
    subtitle: 'Gider + personel',
    icon: ReceiptIcon,
    iconBg: 'bg-red-50',
    iconColor: 'text-red-500',
  },
  {
    label: 'Gider Adedi',
    formatted: formatNum(data.value?.expenseCount ?? 0),
    subtitle: 'Kayıt sayısı',
    icon: ListIcon,
    iconBg: 'bg-slate-100',
    iconColor: 'text-slate-500',
  },
])

const hasTrendData = computed(() =>
  data.value?.monthlyTrend.some(m => m.expenses > 0 || m.payroll > 0)
)

const trendChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit', stacked: false },
  plotOptions: { bar: { columnWidth: '55%', borderRadius: 4, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: data.value?.monthlyTrend.map(m => m.label) ?? [],
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
  colors: ['#f97316', '#8b5cf6'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
}))

const trendSeries = computed(() => [
  { name: 'Giderler', data: data.value?.monthlyTrend.map(m => m.expenses) ?? [] },
  { name: 'Personel', data: data.value?.monthlyTrend.map(m => m.payroll) ?? [] },
])

const donutChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  labels: data.value?.categoryBreakdown.map(c => c.category) ?? [],
  colors: ['#f97316', '#3b82f6', '#10b981', '#8b5cf6', '#f43f5e', '#14b8a6', '#eab308'],
  legend: { position: 'bottom', labels: { colors: '#64748b' }, fontSize: '12px' },
  dataLabels: { enabled: false },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
  plotOptions: { pie: { donut: { size: '60%' } } },
}))

const donutSeries = computed(() => data.value?.categoryBreakdown.map(c => c.amount) ?? [])

// Gider Oranı — kombine bar + line chart
const ratioChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  stroke: { width: [0, 3], curve: 'smooth' },
  plotOptions: { bar: { columnWidth: '50%', borderRadius: 4, borderRadiusApplication: 'end' } },
  dataLabels: { enabled: false },
  xaxis: {
    categories: ratio.value?.monthly.map(m => m.label) ?? [],
    labels: { style: { colors: '#94a3b8', fontSize: '12px' } },
    axisBorder: { show: false }, axisTicks: { show: false },
  },
  yaxis: [
    {
      seriesName: 'Toplam Çıkış',
      labels: {
        style: { colors: '#94a3b8', fontSize: '11px' },
        formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
      },
    },
    {
      seriesName: 'Gider Oranı',
      opposite: true,
      min: 0, max: 120,
      labels: {
        style: { colors: '#94a3b8', fontSize: '11px' },
        formatter: (v: number) => `%${v.toFixed(0)}`,
      },
    },
  ],
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  colors: ['#f97316', '#ef4444'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  tooltip: {
    shared: true, intersect: false,
    y: [
      { formatter: (v: number) => formatCurrency(v) },
      { formatter: (v: number) => `%${v?.toFixed(1) ?? 0}` },
    ],
  },
}))

const ratioSeries = computed(() => [
  { name: 'Toplam Çıkış', type: 'bar',  data: ratio.value?.monthly.map(m => m.totalCost) ?? [] },
  { name: 'Gider Oranı',  type: 'line', data: ratio.value?.monthly.map(m => m.revenue > 0 ? m.ratio : null) ?? [] },
])
</script>
