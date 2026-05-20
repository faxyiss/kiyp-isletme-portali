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

    <!-- Aylık bordro trendi + Maaş bileşen donut -->
    <div class="grid grid-cols-3 gap-4">
      <!-- 12 aylık bordro trendi -->
      <div class="col-span-2 bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Son 12 Ay Bordro Trendi</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!hasPayrollData"
          class="h-64 flex items-center justify-center text-slate-400 text-sm">
          Bu dönemde bordro kaydı yok
        </div>
        <apexchart v-else type="line" height="260" :options="trendChartOptions" :series="trendSeries" />
      </div>

      <!-- Maaş bileşeni donut -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Maaş Bileşen Dağılımı</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!hasComponentData"
          class="h-64 flex items-center justify-center text-slate-400 text-sm">Veri yok</div>
        <apexchart v-else type="donut" height="240" :options="componentDonutOptions" :series="componentDonutSeries" />
      </div>
    </div>

    <!-- Personel bazlı maliyet bar + Personel tablosu -->
    <div class="grid grid-cols-5 gap-4">
      <!-- Yatay bar: personel maliyet sıralaması -->
      <div class="col-span-2 bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Personel Bazlı İşveren Maliyeti</h3>
        <div v-if="loading" class="h-64 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.employeeSummaries.length"
          class="h-64 flex items-center justify-center text-slate-400 text-sm">Veri yok</div>
        <apexchart v-else type="bar" height="260" :options="empBarOptions" :series="empBarSeries" />
      </div>

      <!-- Personel detay tablosu -->
      <div class="col-span-3 bg-white rounded-2xl border border-slate-200 overflow-hidden">
        <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <h3 class="text-sm font-bold text-slate-700">Personel Bordro Özeti</h3>
          <div class="flex gap-2 text-xs">
            <span class="px-2 py-0.5 rounded bg-emerald-50 text-emerald-700 font-medium">Aktif {{ data?.activeEmployeeCount ?? 0 }}</span>
            <span v-if="(data?.totalEmployeeCount ?? 0) > (data?.activeEmployeeCount ?? 0)"
              class="px-2 py-0.5 rounded bg-slate-100 text-slate-500 font-medium">
              Pasif {{ (data?.totalEmployeeCount ?? 0) - (data?.activeEmployeeCount ?? 0) }}
            </span>
          </div>
        </div>
        <div v-if="loading" class="p-6 space-y-3">
          <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
        </div>
        <div v-else-if="!data?.employeeSummaries.length" class="p-8 text-center text-slate-400 text-sm">
          Personel kaydı yok
        </div>
        <table v-else class="w-full text-sm">
          <thead>
            <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
              <th class="px-4 py-3 text-left">Personel</th>
              <th class="px-4 py-3 text-right">Brüt</th>
              <th class="px-4 py-3 text-right">Ort. Net</th>
              <th class="px-4 py-3 text-right">Ort. İşv. Yükü</th>
              <th class="px-4 py-3 text-right">Ödeme</th>
              <th class="px-4 py-3 text-right">İzin</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(e, i) in data.employeeSummaries" :key="i"
              class="border-t border-slate-100 hover:bg-slate-50 transition-colors"
              :class="!e.isActive ? 'opacity-50' : ''">
              <td class="px-4 py-3">
                <p class="font-semibold text-slate-800">{{ e.fullName }}</p>
                <p class="text-xs text-slate-400">{{ e.position }}</p>
              </td>
              <td class="px-4 py-3 text-right font-medium text-slate-700">{{ formatCurrency(e.grossSalary) }}</td>
              <td class="px-4 py-3 text-right text-emerald-700 font-semibold">{{ formatCurrency(e.avgNetSalary) }}</td>
              <td class="px-4 py-3 text-right text-violet-700 font-semibold">{{ formatCurrency(e.avgEmployerCost) }}</td>
              <td class="px-4 py-3 text-right">
                <span class="px-2 py-0.5 rounded bg-slate-100 text-slate-600 text-xs font-medium">
                  {{ e.paymentCount }}
                </span>
              </td>
              <td class="px-4 py-3 text-right text-slate-500">
                <span v-if="e.totalLeaveDays > 0" class="text-orange-600 font-medium">{{ e.totalLeaveDays }} gün</span>
                <span v-else class="text-slate-300">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- İzin kayıtları tablosu -->
    <div v-if="data?.leaveRecords.length" class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
        <h3 class="text-sm font-bold text-slate-700">İzin Kayıtları</h3>
        <span class="text-xs text-slate-400">Son 20 kayıt · seçili dönem</span>
      </div>
      <table class="w-full text-sm">
        <thead>
          <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
            <th class="px-6 py-3 text-left">Personel</th>
            <th class="px-6 py-3 text-left">Pozisyon</th>
            <th class="px-6 py-3 text-right">Başlangıç</th>
            <th class="px-6 py-3 text-right">Bitiş</th>
            <th class="px-6 py-3 text-right">Gün</th>
            <th class="px-6 py-3 text-left">Açıklama</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(l, i) in data.leaveRecords" :key="i"
            class="border-t border-slate-100 hover:bg-slate-50 transition-colors">
            <td class="px-6 py-3 font-semibold text-slate-800">{{ l.fullName }}</td>
            <td class="px-6 py-3 text-slate-500">{{ l.position }}</td>
            <td class="px-6 py-3 text-right text-slate-600">{{ formatDate(l.startDate) }}</td>
            <td class="px-6 py-3 text-right text-slate-600">{{ formatDate(l.endDate) }}</td>
            <td class="px-6 py-3 text-right">
              <span class="px-2 py-0.5 rounded bg-orange-50 text-orange-600 text-xs font-semibold">
                {{ l.dayCount }} gün
              </span>
            </td>
            <td class="px-6 py-3 text-slate-400 text-xs">{{ l.reason ?? '—' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Personel verileri yüklenirken hata oluştu.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { BriefcaseIcon, DollarSignIcon, UsersIcon, CalendarIcon, TrendingUpIcon } from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface EmployeeSummary {
  fullName: string
  position: string
  isActive: boolean
  grossSalary: number
  avgNetSalary: number
  avgEmployerCost: number
  paymentCount: number
  totalLeaveDays: number
}

interface LeaveRecord {
  fullName: string
  position: string
  startDate: string
  endDate: string
  dayCount: number
  reason: string | null
}

interface HRData {
  activeEmployeeCount: number
  totalEmployeeCount: number
  totalGrossSalary: number
  totalEmployerCost: number
  avgNetSalary: number
  totalLeaveDays: number
  salaryComponents: { net: number; employeeTax: number; employerExtra: number }
  monthlyTrend: { label: string; gross: number; net: number; employerCost: number }[]
  employeeSummaries: EmployeeSummary[]
  leaveRecords: LeaveRecord[]
}

const data = ref<HRData | null>(null)
const loading = ref(true)
const error = ref(false)

const fetchData = async () => {
  loading.value = true
  error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/hr?range=${props.range}`, {
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

onMounted(fetchData)
watch(() => props.range, fetchData)

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(v ?? 0)
const formatNum = (v: number) =>
  new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(v ?? 0)
const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

const kpiCards = computed(() => [
  {
    label: 'Aktif Personel',
    formatted: formatNum(data.value?.activeEmployeeCount ?? 0),
    subtitle: `Toplam ${data.value?.totalEmployeeCount ?? 0} kayıt`,
    icon: UsersIcon,
    iconBg: 'bg-blue-50',
    iconColor: 'text-blue-500',
  },
  {
    label: 'Toplam Brüt',
    formatted: formatCurrency(data.value?.totalGrossSalary ?? 0),
    subtitle: 'Dönem brüt maaş',
    icon: BriefcaseIcon,
    iconBg: 'bg-slate-100',
    iconColor: 'text-slate-500',
  },
  {
    label: 'İşveren Yükü',
    formatted: formatCurrency(data.value?.totalEmployerCost ?? 0),
    subtitle: 'Brüt + SGK + işsizlik',
    icon: TrendingUpIcon,
    iconBg: 'bg-violet-50',
    iconColor: 'text-violet-500',
  },
  {
    label: 'Ort. Net Maaş',
    formatted: formatCurrency(data.value?.avgNetSalary ?? 0),
    subtitle: 'Çalışan eline geçen',
    icon: DollarSignIcon,
    iconBg: 'bg-emerald-50',
    iconColor: 'text-emerald-500',
  },
  {
    label: 'İzin Günü',
    formatted: formatNum(data.value?.totalLeaveDays ?? 0),
    subtitle: 'Dönem toplam izin',
    icon: CalendarIcon,
    iconBg: (data.value?.totalLeaveDays ?? 0) > 0 ? 'bg-orange-50' : 'bg-slate-100',
    iconColor: (data.value?.totalLeaveDays ?? 0) > 0 ? 'text-orange-500' : 'text-slate-400',
  },
])

const hasPayrollData = computed(() =>
  data.value?.monthlyTrend.some(m => m.gross > 0)
)

const hasComponentData = computed(() => {
  const c = data.value?.salaryComponents
  return c && (c.net + c.employeeTax + c.employerExtra) > 0
})

// Aylık bordro trendi — brüt / net / işveren yükü
const trendChartOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  stroke: { width: [2, 2, 2], curve: 'smooth', dashArray: [0, 0, 4] },
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
  colors: ['#64748b', '#10b981', '#8b5cf6'],
  legend: { position: 'top', horizontalAlign: 'right', labels: { colors: '#64748b' } },
  markers: { size: 3 },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
}))

const trendSeries = computed(() => [
  { name: 'Brüt', data: data.value?.monthlyTrend.map(m => m.gross) ?? [] },
  { name: 'Net', data: data.value?.monthlyTrend.map(m => m.net) ?? [] },
  { name: 'İşveren Yükü', data: data.value?.monthlyTrend.map(m => m.employerCost) ?? [] },
])

// Maaş bileşen donut
const componentDonutOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  labels: ['Net Maaş', 'Çalışan Kesintileri', 'İşveren Ek Yükü'],
  colors: ['#10b981', '#f97316', '#8b5cf6'],
  legend: { position: 'bottom', labels: { colors: '#64748b' }, fontSize: '11px' },
  dataLabels: { enabled: false },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
  plotOptions: { pie: { donut: { size: '60%' } } },
}))

const componentDonutSeries = computed(() => {
  const c = data.value?.salaryComponents
  return c ? [c.net, c.employeeTax, c.employerExtra] : []
})

// Personel yatay bar
const empBarOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  plotOptions: {
    bar: { horizontal: true, barHeight: '60%', borderRadius: 4, borderRadiusApplication: 'end' },
  },
  dataLabels: { enabled: false },
  xaxis: {
    labels: {
      style: { colors: '#94a3b8', fontSize: '11px' },
      formatter: (v: number) => new Intl.NumberFormat('tr-TR', { notation: 'compact', maximumFractionDigits: 1 }).format(v),
    },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: { labels: { style: { colors: '#64748b', fontSize: '11px' } } },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4, xaxis: { lines: { show: true } }, yaxis: { lines: { show: false } } },
  colors: ['#8b5cf6'],
  tooltip: { x: { show: false }, y: { formatter: (v: number) => formatCurrency(v) } },
}))

const empBarSeries = computed(() => [{
  name: 'İşveren Maliyeti',
  data: (data.value?.employeeSummaries ?? [])
    .slice(0, 10)
    .map(e => ({ x: e.fullName.split(' ')[0], y: e.avgEmployerCost })),
}])
</script>
