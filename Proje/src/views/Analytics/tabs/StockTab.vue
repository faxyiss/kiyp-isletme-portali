<template>
  <div class="space-y-6">
    <!-- KPI Kartları -->
    <div v-if="loading" class="grid grid-cols-3 gap-4">
      <div v-for="i in 3" :key="i" class="bg-white rounded-2xl border border-slate-200 p-5 animate-pulse">
        <div class="h-3 bg-slate-100 rounded w-24 mb-3"></div>
        <div class="h-7 bg-slate-100 rounded w-32"></div>
      </div>
    </div>
    <div v-else class="grid grid-cols-3 gap-4">
      <div v-for="kpi in kpiCards" :key="kpi.label"
        class="bg-white rounded-2xl border border-slate-200 p-5 hover:shadow-md transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <span class="text-xs font-semibold text-slate-400 uppercase tracking-wide">{{ kpi.label }}</span>
          <div class="w-9 h-9 rounded-xl flex items-center justify-center" :class="kpi.iconBg">
            <component :is="kpi.icon" :size="18" :class="kpi.iconColor" />
          </div>
        </div>
        <p class="text-2xl font-bold" :class="kpi.valueColor ?? 'text-slate-800'">{{ kpi.formatted }}</p>
        <p class="text-xs text-slate-400 mt-1">{{ kpi.subtitle }}</p>
      </div>
    </div>

    <!-- Kategori Değeri + Hızlı/Yavaş Eriyen -->
    <div class="grid grid-cols-3 gap-4">
      <!-- Kategori bazlı stok değeri -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <h3 class="text-sm font-bold text-slate-700 mb-4">Kategori Bazlı Stok Değeri</h3>
        <div v-if="loading" class="h-56 bg-slate-50 rounded-xl animate-pulse"></div>
        <div v-else-if="!data?.valueByCategory.length" class="h-56 flex items-center justify-center text-slate-400 text-sm">Veri yok</div>
        <apexchart v-else type="donut" height="240" :options="categoryDonutOptions" :series="categorySeries" />
      </div>

      <!-- Hızlı eriyen ürünler -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <div class="flex items-center gap-2 mb-4">
          <div class="w-2 h-2 rounded-full bg-emerald-500"></div>
          <h3 class="text-sm font-bold text-slate-700">En Hızlı Eriyen</h3>
        </div>
        <div v-if="loading" class="space-y-3">
          <div v-for="i in 5" :key="i" class="h-8 bg-slate-50 rounded-lg animate-pulse"></div>
        </div>
        <div v-else-if="!data?.fastMoving.length" class="h-40 flex items-center justify-center text-slate-400 text-sm">Bu dönemde satış yok</div>
        <div v-else class="space-y-2">
          <div v-for="(item, i) in data.fastMoving" :key="i"
            class="flex items-center justify-between py-2 border-b border-slate-50 last:border-0">
            <div class="flex items-center gap-2 min-w-0">
              <span class="text-xs font-bold text-slate-300 w-4">{{ i + 1 }}</span>
              <span class="text-sm text-slate-700 font-medium truncate">{{ item.productName }}</span>
            </div>
            <span class="text-sm font-semibold text-emerald-600 ml-2 shrink-0">{{ formatNum(item.soldQty) }} adet</span>
          </div>
        </div>
      </div>

      <!-- Yavaş eriyen ürünler -->
      <div class="bg-white rounded-2xl border border-slate-200 p-6">
        <div class="flex items-center gap-2 mb-4">
          <div class="w-2 h-2 rounded-full bg-orange-400"></div>
          <h3 class="text-sm font-bold text-slate-700">En Yavaş Eriyen</h3>
        </div>
        <div v-if="loading" class="space-y-3">
          <div v-for="i in 5" :key="i" class="h-8 bg-slate-50 rounded-lg animate-pulse"></div>
        </div>
        <div v-else-if="!data?.slowMoving.length" class="h-40 flex items-center justify-center text-slate-400 text-sm">Veri yok</div>
        <div v-else class="space-y-2">
          <div v-for="(item, i) in data.slowMoving" :key="i"
            class="flex items-center justify-between py-2 border-b border-slate-50 last:border-0">
            <div class="flex items-center gap-2 min-w-0">
              <span class="text-xs font-bold text-slate-300 w-4">{{ i + 1 }}</span>
              <span class="text-sm text-slate-700 font-medium truncate">{{ item.productName }}</span>
            </div>
            <span class="text-sm font-semibold text-orange-500 ml-2 shrink-0">{{ formatNum(item.soldQty) }} adet</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Ürün bazlı stok tablosu -->
    <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
        <h3 class="text-sm font-bold text-slate-700">Ürün Bazlı Stok Durumu</h3>
        <div v-if="!loading && data" class="flex items-center gap-2 text-xs text-slate-400">
          <span class="w-2 h-2 rounded-full bg-red-400 inline-block"></span> Kritik stok
        </div>
      </div>
      <div v-if="loading" class="p-6 space-y-3">
        <div v-for="i in 6" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="!data?.stockLevels.length" class="p-8 text-center text-slate-400 text-sm">
        Stokta ürün bulunamadı
      </div>
      <table v-else class="w-full text-sm">
        <thead>
          <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
            <th class="px-6 py-3 text-left">Ürün Adı</th>
            <th class="px-6 py-3 text-left">Kategori</th>
            <th class="px-6 py-3 text-right">Kalan Miktar</th>
            <th class="px-6 py-3 text-right">Birim Maliyet</th>
            <th class="px-6 py-3 text-right">Toplam Değer</th>
            <th class="px-6 py-3 text-center">Durum</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(s, i) in pagedStockLevels" :key="i"
            class="border-t border-slate-100 hover:bg-slate-50 transition-colors"
            :class="s.isCritical ? 'bg-red-50/30' : ''">
            <td class="px-6 py-4 font-semibold text-slate-800">{{ s.productName }}</td>
            <td class="px-6 py-4 text-slate-500">{{ s.categoryName }}</td>
            <td class="px-6 py-4 text-right font-medium" :class="s.isCritical ? 'text-red-600' : 'text-slate-700'">
              {{ formatNum(s.remainingQty) }}
            </td>
            <td class="px-6 py-4 text-right text-slate-600">{{ formatCurrency(s.unitCost) }}</td>
            <td class="px-6 py-4 text-right font-semibold text-slate-800">{{ formatCurrency(s.totalValue) }}</td>
            <td class="px-6 py-4 text-center">
              <span v-if="s.isCritical" class="px-2.5 py-1 rounded-lg text-xs font-semibold bg-red-50 text-red-600">
                Kritik
              </span>
              <span v-else class="px-2.5 py-1 rounded-lg text-xs font-semibold bg-emerald-50 text-emerald-600">
                Normal
              </span>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="stockTotalPages > 1" class="flex items-center justify-between px-6 py-3 border-t border-slate-100 bg-slate-50/50">
        <span class="text-xs text-slate-400">{{ sortedStockLevels.length }} ürün · Sayfa {{ stockPage }}/{{ stockTotalPages }}</span>
        <div class="flex gap-2">
          <button @click="stockPage--" :disabled="stockPage === 1"
            class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed">
            Önceki
          </button>
          <button @click="stockPage++" :disabled="stockPage === stockTotalPages"
            class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed">
            Sonraki
          </button>
        </div>
      </div>
    </div>

    <!-- Stok Devir Hızı Analizi -->
    <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden">
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
        <div>
          <h3 class="text-sm font-bold text-slate-700">Stok Devir Hızı Analizi</h3>
          <p class="text-xs text-slate-400 mt-0.5">Son 30 günlük satışa göre — düşük devir = bağlı sermaye</p>
        </div>
        <div v-if="turnover" class="flex gap-3 text-xs">
          <span class="px-2.5 py-1 rounded-lg bg-slate-100 text-slate-600 font-medium">
            Ort. devir: {{ turnover.avgTurnoverRate }}x
          </span>
          <span v-if="turnover.deadStockCount > 0" class="px-2.5 py-1 rounded-lg bg-red-50 text-red-600 font-semibold">
            {{ turnover.deadStockCount }} ölü stok · {{ formatCurrency(turnover.deadStockValue) }}
          </span>
        </div>
      </div>

      <!-- KPI satırı -->
      <div v-if="turnover" class="grid grid-cols-4 divide-x divide-slate-100 border-b border-slate-100">
        <div class="px-6 py-4">
          <p class="text-xs text-slate-400 mb-1">Toplam Bağlı Sermaye</p>
          <p class="text-lg font-bold text-violet-700">{{ formatCurrency(turnover.totalBoundCapital) }}</p>
        </div>
        <div class="px-6 py-4">
          <p class="text-xs text-slate-400 mb-1">Ort. Devir Hızı</p>
          <p class="text-lg font-bold text-slate-800">{{ turnover.avgTurnoverRate }}x <span class="text-xs text-slate-400 font-normal">/ay</span></p>
        </div>
        <div class="px-6 py-4">
          <p class="text-xs text-slate-400 mb-1">Ölü Stok Adedi</p>
          <p class="text-lg font-bold" :class="turnover.deadStockCount > 0 ? 'text-red-600' : 'text-emerald-600'">
            {{ turnover.deadStockCount }} ürün
          </p>
        </div>
        <div class="px-6 py-4">
          <p class="text-xs text-slate-400 mb-1">Ölü Stok Değeri</p>
          <p class="text-lg font-bold text-red-600">{{ formatCurrency(turnover.deadStockValue) }}</p>
        </div>
      </div>

      <div v-if="turnoverLoading" class="p-6 space-y-3">
        <div v-for="i in 5" :key="i" class="h-10 bg-slate-50 rounded-xl animate-pulse"></div>
      </div>
      <div v-else-if="!turnover?.items.length" class="p-8 text-center text-slate-400 text-sm">
        Stok verisi bulunamadı
      </div>
      <table v-else class="w-full text-sm">
        <thead>
          <tr class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wide">
            <th class="px-6 py-3 text-left">Ürün</th>
            <th class="px-6 py-3 text-left">Kategori</th>
            <th class="px-6 py-3 text-right">Kalan</th>
            <th class="px-6 py-3 text-right">30g Satış</th>
            <th class="px-6 py-3 text-right cursor-pointer select-none hover:text-slate-700"
                @click="turnoverSortDesc = !turnoverSortDesc; turnoverPage = 1">
              Devir Hızı {{ turnoverSortDesc ? '↓' : '↑' }}
            </th>
            <th class="px-6 py-3 text-right">Satış Süresi</th>
            <th class="px-6 py-3 text-right">Bağlı Sermaye</th>
            <th class="px-6 py-3 text-center">Durum</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(t, i) in pagedTurnoverItems" :key="i"
            class="border-t border-slate-100 hover:bg-slate-50 transition-colors"
            :class="t.isDeadStock ? 'bg-red-50/20' : ''">
            <td class="px-6 py-3 font-semibold text-slate-800">{{ t.productName }}</td>
            <td class="px-6 py-3 text-slate-500">{{ t.categoryName }}</td>
            <td class="px-6 py-3 text-right text-slate-700">{{ formatNum(t.remainingQty) }}</td>
            <td class="px-6 py-3 text-right" :class="t.soldQty30d > 0 ? 'text-emerald-700 font-medium' : 'text-slate-300'">
              {{ t.soldQty30d > 0 ? formatNum(t.soldQty30d) : '—' }}
            </td>
            <td class="px-6 py-3 text-right">
              <span class="font-semibold" :class="turnoverRateColor(t.turnoverRate, t.isDeadStock)">
                {{ t.isDeadStock ? '0.00' : t.turnoverRate.toFixed(2) }}x
              </span>
            </td>
            <td class="px-6 py-3 text-right text-slate-600">
              {{ t.isDeadStock ? '∞' : t.daysToSell + ' gün' }}
            </td>
            <td class="px-6 py-3 text-right font-semibold text-violet-700">{{ formatCurrency(t.boundCapital) }}</td>
            <td class="px-6 py-3 text-center">
              <span v-if="t.isDeadStock" class="px-2 py-0.5 rounded-lg text-xs font-bold bg-red-50 text-red-600">Ölü Stok</span>
              <span v-else-if="t.turnoverRate < 0.3" class="px-2 py-0.5 rounded-lg text-xs font-bold bg-orange-50 text-orange-600">Yavaş</span>
              <span v-else-if="t.turnoverRate >= 1" class="px-2 py-0.5 rounded-lg text-xs font-bold bg-emerald-50 text-emerald-600">Hızlı</span>
              <span v-else class="px-2 py-0.5 rounded-lg text-xs font-bold bg-blue-50 text-blue-600">Normal</span>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="turnoverTotalPages > 1" class="flex items-center justify-between px-6 py-3 border-t border-slate-100 bg-slate-50/50">
        <span class="text-xs text-slate-400">{{ sortedTurnoverItems.length }} ürün · Sayfa {{ turnoverPage }}/{{ turnoverTotalPages }}</span>
        <div class="flex gap-2">
          <button @click="turnoverPage--" :disabled="turnoverPage === 1"
            class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed">
            Önceki
          </button>
          <button @click="turnoverPage++" :disabled="turnoverPage === turnoverTotalPages"
            class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed">
            Sonraki
          </button>
        </div>
      </div>
    </div>

    <div v-if="error" class="bg-red-50 border border-red-200 rounded-2xl p-4 text-sm text-red-600">
      Stok verileri yüklenirken hata oluştu.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { PackageIcon, DollarSignIcon, AlertTriangleIcon } from 'lucide-vue-next'

const props = defineProps<{ range: string }>()
const BASE_URL = import.meta.env.VITE_API_BASE_URL

interface StockLevelItem {
  productName: string
  categoryName: string
  remainingQty: number
  unitCost: number
  totalValue: number
  isCritical: boolean
}

interface StockData {
  totalStockValue: number
  totalProducts: number
  criticalStockCount: number
  stockLevels: StockLevelItem[]
  valueByCategory: { category: string; totalValue: number }[]
  fastMoving: { productName: string; soldQty: number; remainingQty: number }[]
  slowMoving: { productName: string; soldQty: number; remainingQty: number }[]
}

interface TurnoverItem {
  productName: string; categoryName: string; turnoverRate: number; daysToSell: number
  soldQty30d: number; remainingQty: number; boundCapital: number; isDeadStock: boolean
}
interface TurnoverData {
  avgTurnoverRate: number; deadStockCount: number; deadStockValue: number
  totalBoundCapital: number; items: TurnoverItem[]
}

const data = ref<StockData | null>(null)
const turnover = ref<TurnoverData | null>(null)
const loading = ref(true)
const turnoverLoading = ref(true)
const error = ref(false)

const PAGE_SIZE = 10
const stockPage = ref(1)
const turnoverPage = ref(1)
const turnoverSortDesc = ref(true)

const sortedStockLevels = computed(() =>
  [...(data.value?.stockLevels ?? [])].sort((a, b) => b.remainingQty - a.remainingQty)
)
const pagedStockLevels = computed(() =>
  sortedStockLevels.value.slice((stockPage.value - 1) * PAGE_SIZE, stockPage.value * PAGE_SIZE)
)
const stockTotalPages = computed(() => Math.ceil(sortedStockLevels.value.length / PAGE_SIZE))

const sortedTurnoverItems = computed(() => {
  const items = [...(turnover.value?.items ?? [])]
  return items.sort((a, b) =>
    turnoverSortDesc.value ? b.turnoverRate - a.turnoverRate : a.turnoverRate - b.turnoverRate
  )
})
const pagedTurnoverItems = computed(() =>
  sortedTurnoverItems.value.slice((turnoverPage.value - 1) * PAGE_SIZE, turnoverPage.value * PAGE_SIZE)
)
const turnoverTotalPages = computed(() => Math.ceil(sortedTurnoverItems.value.length / PAGE_SIZE))

const fetchData = async () => {
  loading.value = true; error.value = false
  try {
    const res = await fetch(`${BASE_URL}/analytics/stocks?range=${props.range}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (!res.ok) throw new Error()
    data.value = await res.json()
  } catch { error.value = true }
  finally { loading.value = false }
}

const fetchTurnover = async () => {
  turnoverLoading.value = true
  try {
    const res = await fetch(`${BASE_URL}/analytics/stock-turnover`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    })
    if (res.ok) turnover.value = await res.json()
  } catch { /* sessiz */ }
  finally { turnoverLoading.value = false }
}

onMounted(() => { fetchData(); fetchTurnover() })
watch(() => props.range, fetchData)

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(v ?? 0)
const formatNum = (v: number) =>
  new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(v ?? 0)

const turnoverRateColor = (rate: number, dead: boolean) => {
  if (dead) return 'text-red-600'
  if (rate >= 1) return 'text-emerald-600'
  if (rate >= 0.3) return 'text-blue-600'
  return 'text-orange-500'
}

const kpiCards = computed(() => [
  {
    label: 'Toplam Stok Değeri',
    formatted: formatCurrency(data.value?.totalStockValue ?? 0),
    subtitle: 'Kalan miktar × birim maliyet',
    icon: DollarSignIcon,
    iconBg: 'bg-violet-50',
    iconColor: 'text-violet-500',
  },
  {
    label: 'Ürün Çeşidi',
    formatted: String(data.value?.totalProducts ?? 0),
    subtitle: 'Stokta kalan farklı ürün',
    icon: PackageIcon,
    iconBg: 'bg-blue-50',
    iconColor: 'text-blue-500',
  },
  {
    label: 'Kritik Stok',
    formatted: String(data.value?.criticalStockCount ?? 0),
    subtitle: 'Dikkat gerektiren ürün',
    icon: AlertTriangleIcon,
    iconBg: (data.value?.criticalStockCount ?? 0) > 0 ? 'bg-red-50' : 'bg-emerald-50',
    iconColor: (data.value?.criticalStockCount ?? 0) > 0 ? 'text-red-500' : 'text-emerald-500',
    valueColor: (data.value?.criticalStockCount ?? 0) > 0 ? 'text-red-600' : 'text-emerald-600',
  },
])

const categoryDonutOptions = computed(() => ({
  chart: { toolbar: { show: false }, background: 'transparent', fontFamily: 'inherit' },
  labels: data.value?.valueByCategory.map(c => c.category) ?? [],
  colors: ['#3b82f6', '#10b981', '#f97316', '#8b5cf6', '#f43f5e', '#14b8a6'],
  legend: { position: 'bottom', labels: { colors: '#64748b' }, fontSize: '12px' },
  dataLabels: { enabled: false },
  tooltip: { y: { formatter: (v: number) => formatCurrency(v) } },
  plotOptions: { pie: { donut: { size: '60%' } } },
}))

const categorySeries = computed(() => data.value?.valueByCategory.map(c => c.totalValue) ?? [])
</script>
