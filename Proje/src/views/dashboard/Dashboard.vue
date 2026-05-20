<template>
  <div class="space-y-6">
    <!-- Satış & Finans Kartları -->
    <div>
      <p class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-3">Satış & Finans</p>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Bu Ayki Ciro</p>
            <h3 class="text-2xl font-bold text-slate-800 mt-1">
              {{ isLoadingSales ? '...' : formatCurrency(monthlySales.totalRevenue) }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">{{ currentMonthLabel }}</p>
          </div>
          <div class="p-3 bg-emerald-50 rounded-xl shrink-0">
            <TrendingUpIcon :size="22" class="text-emerald-500" />
          </div>
        </div>

        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
              Bu Ayki Giderler
            </p>
            <h3 class="text-2xl font-bold text-slate-800 mt-1">
              {{ isLoadingExpenses ? '...' : formatCurrency(monthlyExpense) }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">Operasyonel giderler</p>
          </div>
          <div class="p-3 bg-red-50 rounded-xl shrink-0">
            <ReceiptIcon :size="22" class="text-red-500" />
          </div>
        </div>

        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
              Personel Maliyeti
            </p>
            <h3 class="text-2xl font-bold text-slate-800 mt-1">
              {{ isLoadingEmployees ? '...' : formatCurrency(employeeSummary.totalEmployerCost) }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">
              {{ employeeSummary.activeEmployees }} aktif personel
            </p>
          </div>
          <div class="p-3 bg-blue-50 rounded-xl shrink-0">
            <UsersIcon :size="22" class="text-blue-500" />
          </div>
        </div>

        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
              Bekleyen Veresiye
            </p>
            <h3 class="text-2xl font-bold text-amber-600 mt-1">
              {{ isLoadingCustomers ? '...' : formatCurrency(totalDebt) }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">{{ debtorCount }} borçlu müşteri</p>
          </div>
          <div class="p-3 bg-amber-50 rounded-xl shrink-0">
            <ClockIcon :size="22" class="text-amber-500" />
          </div>
        </div>
      </div>
    </div>

    <!-- Stok & Üretim Kartları -->
    <div>
      <p class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-3">Stok & Üretim</p>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
              Envanter Değeri
            </p>
            <h3 class="text-2xl font-bold text-slate-800 mt-1">
              {{ isLoadingProducts ? '...' : formatCurrency(totalInventoryValue) }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">{{ totalProductCount }} ürün çeşidi</p>
          </div>
          <div class="p-3 bg-indigo-50 rounded-xl shrink-0">
            <PackageIcon :size="22" class="text-indigo-500" />
          </div>
        </div>

        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
              Biten Ürünler
            </p>
            <h3
              class="text-2xl font-bold mt-1"
              :class="outOfStockCount > 0 ? 'text-red-600' : 'text-slate-800'"
            >
              {{ isLoadingProducts ? '...' : `${outOfStockCount} Ürün` }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">Stoku tükenen ürünler</p>
          </div>
          <div
            class="p-3 rounded-xl shrink-0"
            :class="outOfStockCount > 0 ? 'bg-red-50' : 'bg-slate-50'"
          >
            <AlertTriangleIcon
              :size="22"
              :class="outOfStockCount > 0 ? 'text-red-500' : 'text-slate-300'"
            />
          </div>
        </div>

        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">
              Hammadde Değeri
            </p>
            <h3 class="text-2xl font-bold text-slate-800 mt-1">
              {{ isLoadingRawMaterials ? '...' : formatCurrency(totalRawMaterialValue) }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">{{ totalRawMaterialCount }} hammadde çeşidi</p>
          </div>
          <div class="p-3 bg-orange-50 rounded-xl shrink-0">
            <LayersIcon :size="22" class="text-orange-500" />
          </div>
        </div>

        <div
          class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center justify-between"
        >
          <div>
            <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Bu Ay Üretim</p>
            <h3 class="text-2xl font-bold text-slate-800 mt-1">
              {{ isLoadingProduction ? '...' : `${productionSummary.totalQuantity} Adet` }}
            </h3>
            <p class="text-xs text-slate-400 mt-1">
              {{ formatCurrency(productionSummary.totalCost) }} maliyet
            </p>
          </div>
          <div class="p-3 bg-purple-50 rounded-xl shrink-0">
            <SettingsIcon :size="22" class="text-purple-500" />
          </div>
        </div>
      </div>
    </div>

    <!-- Grafik + Son Satışlar -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Gelir & Gider Grafiği -->
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 lg:col-span-2">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-4">
          <h3 class="font-bold text-slate-800 text-base">Aylık Gelir & Gider</h3>
          <span class="text-xs text-blue-600 bg-blue-50 px-2.5 py-1 rounded-lg font-semibold">Son 12 Ay</span>
        </div>
        <div
          v-if="isLoadingChart"
          class="h-64 flex items-center justify-center text-slate-400 text-sm"
        >
          Grafik yükleniyor...
        </div>
        <div v-else class="h-64">
          <apexchart type="bar" height="100%" :options="chartOptions" :series="chartSeries" />
        </div>
      </div>

      <!-- Son Satışlar -->
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-4">
          <h3 class="font-bold text-slate-800 text-base">Son Satışlar</h3>
          <router-link
            to="/sales"
            class="text-xs font-semibold text-blue-600 hover:text-blue-700 transition-colors"
          >
            Tümü →
          </router-link>
        </div>

        <div v-if="isLoadingSales" class="py-8 text-center text-slate-400 text-sm">
          Yükleniyor...
        </div>

        <div v-else-if="recentSales.length === 0" class="py-8 text-center">
          <ShoppingCartIcon :size="28" class="text-slate-200 mx-auto mb-2" />
          <p class="text-sm text-slate-400">Henüz satış yok</p>
        </div>

        <div v-else class="space-y-2.5">
          <div
            v-for="sale in recentSales"
            :key="sale.id"
            class="flex items-center justify-between gap-3 p-3 rounded-xl bg-slate-50 border border-slate-100 hover:border-slate-200 transition-colors"
          >
            <div class="flex items-center gap-2.5 min-w-0">
              <div
                class="w-8 h-8 rounded-lg bg-emerald-100 flex items-center justify-center shrink-0"
              >
                <ShoppingCartIcon :size="13" class="text-emerald-600" />
              </div>
              <div class="min-w-0">
                <p class="text-xs font-bold text-slate-800 truncate">{{ sale.productName }}</p>
                <p class="text-[11px] text-slate-400 mt-0.5">
                  {{ sale.customerName || 'Perakende' }} · {{ formatDate(sale.saleDate) }}
                </p>
              </div>
            </div>
            <div class="text-right shrink-0">
              <p class="text-xs font-bold text-emerald-600">
                {{ formatCurrency(sale.totalAmount) }}
              </p>
              <span
                class="text-[10px] font-semibold px-1.5 py-0.5 rounded-md"
                :class="paymentBadge(sale.paymentType)"
              >
                {{ sale.paymentType }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- En Çok Satan Ürünler + Ödeme Tipi Dağılımı -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 lg:col-span-2">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-4">
          <h3 class="font-bold text-slate-800 text-base">En Çok Satan Ürünler</h3>
          <span class="text-xs text-slate-400">Son 12 Ay</span>
        </div>
        <div
          v-if="isLoadingChart"
          class="h-64 flex items-center justify-center text-slate-400 text-sm"
        >
          Grafik yükleniyor...
        </div>
        <div
          v-else-if="topProducts.length === 0"
          class="h-64 flex items-center justify-center"
        >
          <p class="text-sm text-slate-400">Satış verisi yok</p>
        </div>
        <div v-else class="h-64">
          <apexchart
            type="bar"
            height="100%"
            :options="topProductsChartOptions"
            :series="topProductsSeries"
          />
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-4">
          <h3 class="font-bold text-slate-800 text-base">Kategori Dağılımı</h3>
          <span class="text-xs text-slate-400">{{ currentMonthLabel }}</span>
        </div>
        <div
          v-if="isLoadingCategory"
          class="h-64 flex items-center justify-center text-slate-400 text-sm"
        >
          Yükleniyor...
        </div>
        <div
          v-else-if="categorySeries.length === 0"
          class="h-64 flex items-center justify-center"
        >
          <p class="text-sm text-slate-400">Bu ay satış yok</p>
        </div>
        <div v-else class="h-64">
          <apexchart
            type="donut"
            height="100%"
            :options="categoryChartOptions"
            :series="categorySeries"
          />
        </div>
      </div>
    </div>

    <!-- En Borçlu Müşteriler + Kritik Stok -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- En Borçlu Müşteriler -->
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-4">
          <h3 class="font-bold text-slate-800 text-base">En Borçlu Müşteriler</h3>
          <router-link
            to="/customers"
            class="text-xs font-semibold text-blue-600 hover:text-blue-700 transition-colors"
          >
            Tümü →
          </router-link>
        </div>
        <div v-if="isLoadingCustomers" class="py-8 text-center text-slate-400 text-sm">
          Yükleniyor...
        </div>
        <div v-else-if="topDebtors.length === 0" class="py-8 text-center">
          <p class="text-sm text-slate-400">Borçlu müşteri yok</p>
        </div>
        <div v-else class="space-y-3.5">
          <div v-for="customer in topDebtors" :key="customer.id" class="space-y-1.5">
            <div class="flex items-center justify-between">
              <span class="text-sm font-semibold text-slate-700 truncate">
                {{ customer.firstName }} {{ customer.lastName }}
              </span>
              <span class="text-sm font-bold text-amber-600 shrink-0 ml-2">
                {{ formatCurrency(customer.currentBalance) }}
              </span>
            </div>
            <div class="w-full bg-slate-100 rounded-full h-1.5">
              <div
                class="bg-amber-400 h-1.5 rounded-full"
                :style="{
                  width: `${Math.min(100, (customer.currentBalance / maxDebt) * 100)}%`,
                }"
              />
            </div>
            <p class="text-[11px] text-slate-400">{{ customer.phoneNumber }}</p>
          </div>
        </div>
      </div>

      <!-- Kritik Stok Durumu -->
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-4">
          <h3 class="font-bold text-slate-800 text-base">Kritik Stok Durumu</h3>
          <router-link
            to="/stocks"
            class="text-xs font-semibold text-blue-600 hover:text-blue-700 transition-colors"
          >
            Tümü →
          </router-link>
        </div>
        <div v-if="isLoadingProducts" class="py-8 text-center text-slate-400 text-sm">
          Yükleniyor...
        </div>
        <div v-else-if="criticalStock.length === 0" class="py-8 text-center">
          <CheckCircleIcon :size="28" class="text-emerald-300 mx-auto mb-2" />
          <p class="text-sm text-slate-400">Kritik stok durumu yok</p>
        </div>
        <div v-else class="space-y-2">
          <div
            v-for="product in criticalStock"
            :key="product.id"
            class="flex items-center justify-between p-3 rounded-xl bg-slate-50 border border-slate-100"
          >
            <div class="min-w-0">
              <p class="text-sm font-semibold text-slate-700 truncate">{{ product.name }}</p>
              <p class="text-[11px] text-slate-400 mt-0.5">{{ product.categoryName || '—' }}</p>
            </div>
            <span
              class="shrink-0 ml-3 text-xs font-bold px-2.5 py-1 rounded-lg"
              :class="
                product.remainingQuantity === 0
                  ? 'bg-red-100 text-red-600'
                  : 'bg-amber-100 text-amber-600'
              "
            >
              {{ product.remainingQuantity === 0 ? 'Tükendi' : `${product.remainingQuantity} adet` }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Finansal Özet -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
      <div class="flex items-center justify-between border-b border-slate-100 pb-4 mb-5">
        <h3 class="font-bold text-slate-800 text-base">{{ currentMonthLabel }} — Finansal Özet</h3>
        <span class="text-xs text-blue-600 bg-blue-50 px-2.5 py-1 rounded-lg font-semibold"
          >Bu Ay</span
        >
      </div>
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div class="text-center p-3 rounded-xl bg-emerald-50 border border-emerald-100">
          <p class="text-xs text-emerald-600 font-semibold uppercase tracking-wide">Toplam Gelir</p>
          <p class="text-lg font-bold text-emerald-700 mt-1">
            {{ formatCurrency(monthlySales.totalRevenue) }}
          </p>
        </div>
        <div class="text-center p-3 rounded-xl bg-red-50 border border-red-100">
          <p class="text-xs text-red-500 font-semibold uppercase tracking-wide">Toplam Gider</p>
          <p class="text-lg font-bold text-red-600 mt-1">
            {{ formatCurrency(totalMonthlyExpenses) }}
          </p>
        </div>
        <div
          class="text-center p-3 rounded-xl border"
          :class="estimatedProfit >= 0 ? 'bg-blue-50 border-blue-100' : 'bg-red-50 border-red-100'"
        >
          <p
            class="text-xs font-semibold uppercase tracking-wide"
            :class="estimatedProfit >= 0 ? 'text-blue-600' : 'text-red-500'"
          >
            Tahmini Kâr
          </p>
          <p
            class="text-lg font-bold mt-1"
            :class="estimatedProfit >= 0 ? 'text-blue-700' : 'text-red-600'"
          >
            {{ formatCurrency(estimatedProfit) }}
          </p>
        </div>
        <div class="text-center p-3 rounded-xl bg-slate-50 border border-slate-100">
          <p class="text-xs text-slate-500 font-semibold uppercase tracking-wide">Personel Yükü</p>
          <p class="text-lg font-bold text-slate-700 mt-1">
            {{ formatCurrency(employeeSummary.totalEmployerCost) }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  TrendingUpIcon,
  ReceiptIcon,
  UsersIcon,
  ClockIcon,
  ShoppingCartIcon,
  PackageIcon,
  AlertTriangleIcon,
  LayersIcon,
  SettingsIcon,
  CheckCircleIcon,
} from 'lucide-vue-next'

const BASE_URL = '/api'
const getToken = () => localStorage.getItem('token')

const now = new Date()
const currentMonth = now.getMonth() + 1
const currentYear = now.getFullYear()

const last12Slots = (() => {
  const slots: { year: number; month: number; label: string }[] = []
  for (let i = 11; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1)
    slots.push({
      year: d.getFullYear(),
      month: d.getMonth() + 1,
      label: d.toLocaleDateString('tr-TR', { month: 'short' }) + ` '${String(d.getFullYear()).slice(2)}`,
    })
  }
  return slots
})()

const currentMonthLabel = computed(() =>
  now.toLocaleDateString('tr-TR', { month: 'long', year: 'numeric' }),
)

// ── Loading ────────────────────────────────────────────────
const isLoadingSales = ref(false)
const isLoadingExpenses = ref(false)
const isLoadingEmployees = ref(false)
const isLoadingCustomers = ref(false)
const isLoadingProducts = ref(false)
const isLoadingRawMaterials = ref(false)
const isLoadingProduction = ref(false)
const isLoadingChart = ref(true)

// ── Data ───────────────────────────────────────────────────
const monthlySales = ref({ totalRevenue: 0, totalItems: 0 })
const recentSales = ref<any[]>([])
const categorySalesData = ref<{ categoryName: string; totalRevenue: number; totalQuantity: number }[]>([])
const isLoadingCategory = ref(false)
const yearlySales = ref<any[]>([])
const monthlyExpense = ref(0)
const expenseSummary = ref<any[]>([])
const employeeSummary = ref({ totalEmployerCost: 0, totalNetSalary: 0, activeEmployees: 0 })
const totalDebt = ref(0)
const debtorCount = ref(0)
const allDebtors = ref<any[]>([])
const totalInventoryValue = ref(0)
const totalProductCount = ref(0)
const outOfStockCount = ref(0)
const allProducts = ref<any[]>([])
const totalRawMaterialValue = ref(0)
const totalRawMaterialCount = ref(0)
const productionSummary = ref({ totalQuantity: 0, totalCost: 0 })

// ── Computed ───────────────────────────────────────────────
const totalMonthlyExpenses = computed(
  () => monthlyExpense.value + employeeSummary.value.totalEmployerCost,
)
const estimatedProfit = computed(() => monthlySales.value.totalRevenue - totalMonthlyExpenses.value)

// ── Yeni panel computed'ları ───────────────────────────────
const topProducts = computed(() => {
  const map = new Map<string, number>()
  yearlySales.value.forEach((sale: any) => {
    const name = sale.productName || 'Bilinmeyen'
    map.set(name, (map.get(name) || 0) + (Number(sale.totalAmount) || 0))
  })
  return [...map.entries()].sort((a, b) => b[1] - a[1]).slice(0, 8)
})

const topProductsSeries = computed(() => [{ name: 'Ciro', data: topProducts.value.map(([, v]) => Math.round(v)) }])

const topProductsChartOptions = computed(() => ({
  chart: { type: 'bar', fontFamily: 'inherit', toolbar: { show: false } },
  plotOptions: { bar: { horizontal: true, borderRadius: 4, barHeight: '55%' } },
  colors: ['#6366f1'],
  dataLabels: { enabled: false },
  xaxis: {
    categories: topProducts.value.map(([k]) => k),
    labels: {
      formatter: (v: number) => (v >= 1000 ? `₺${(v / 1000).toFixed(0)}k` : `₺${v}`),
      style: { fontSize: '11px', colors: Array(8).fill('#94a3b8') },
    },
  },
  yaxis: { labels: { style: { fontSize: '11px', colors: ['#94a3b8'] }, maxWidth: 120 } },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  tooltip: { theme: 'light', y: { formatter: (v: number) => formatCurrency(v) } },
}))

const categorySeries = computed(() => categorySalesData.value.map((d) => Math.round(d.totalRevenue)))

const categoryChartOptions = computed(() => ({
  chart: { type: 'donut', fontFamily: 'inherit', toolbar: { show: false } },
  labels: categorySalesData.value.map((d) => d.categoryName),
  colors: ['#6366f1', '#10b981', '#f59e0b', '#ef4444', '#3b82f6', '#ec4899', '#8b5cf6', '#14b8a6'],
  dataLabels: { enabled: true, formatter: (val: number) => `${val.toFixed(1)}%` },
  legend: { position: 'bottom', fontSize: '11px' },
  plotOptions: { pie: { donut: { size: '65%' } } },
  tooltip: { theme: 'light', y: { formatter: (v: number) => formatCurrency(v) } },
}))

const topDebtors = computed(() =>
  [...allDebtors.value].sort((a, b) => (b.currentBalance ?? 0) - (a.currentBalance ?? 0)).slice(0, 7),
)

const maxDebt = computed(() => topDebtors.value[0]?.currentBalance ?? 1)

const criticalStock = computed(() =>
  allProducts.value
    .filter((p) => (p.remainingQuantity ?? 0) <= 5)
    .sort((a, b) => (a.remainingQuantity ?? 0) - (b.remainingQuantity ?? 0))
    .slice(0, 10),
)

// ── Grafik ─────────────────────────────────────────────────

const monthlyRevenueData = computed(() =>
  last12Slots.map((slot) =>
    yearlySales.value
      .filter((sale) => {
        const d = new Date(sale.saleDate)
        return d.getFullYear() === slot.year && d.getMonth() + 1 === slot.month
      })
      .reduce((sum: number, sale) => sum + (Number(sale.totalAmount) || 0), 0),
  ),
)

const monthlyExpenseData = computed(() =>
  last12Slots.map((slot) => {
    const item = expenseSummary.value.find(
      (e) => e.year === slot.year && e.month === slot.month,
    )
    return Number(item?.totalAmount) || 0
  }),
)

const chartSeries = computed(() => [
  { name: 'Gelir (Ciro)', data: monthlyRevenueData.value },
  { name: 'Gider', data: monthlyExpenseData.value },
])

const chartOptions = computed(() => ({
  chart: {
    type: 'bar',
    fontFamily: 'inherit',
    toolbar: { show: false },
    zoom: { enabled: false },
    stacked: true,
    stackType: 'normal',
  },
  colors: ['#10b981', '#ef4444'],
  plotOptions: {
    bar: { horizontal: false, borderRadius: 4, borderRadiusApplication: 'end', columnWidth: '50%' },
  },
  dataLabels: { enabled: false },
  xaxis: {
    categories: last12Slots.map((s) => s.label),
    axisBorder: { show: false },
    axisTicks: { show: false },
    labels: { style: { fontSize: '11px', colors: Array(12).fill('#94a3b8') }, rotate: -30 },
  },
  yaxis: {
    labels: {
      formatter: (v: number) => (v >= 1000 ? `₺${(v / 1000).toFixed(0)}k` : `₺${v}`),
      style: { fontSize: '11px', colors: ['#94a3b8'] },
    },
  },
  grid: { borderColor: '#f1f5f9', strokeDashArray: 4 },
  legend: { position: 'top', horizontalAlign: 'right', fontSize: '12px' },
  tooltip: { theme: 'light', y: { formatter: (v: number) => formatCurrency(v) } },
}))

// ── Helpers ────────────────────────────────────────────────
const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(v ?? 0)

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short' })

const paymentBadge = (type: string) => {
  const t = (type || '').toLowerCase()
  if (t === 'cash' || t === 'nakit') return 'bg-emerald-50 text-emerald-600'
  if (t === 'credit' || t === 'veresiye') return 'bg-amber-50 text-amber-600'
  return 'bg-slate-100 text-slate-500'
}

// ── API ────────────────────────────────────────────────────
const fetchMonthlySales = async () => {
  isLoadingSales.value = true
  try {
    const start = new Date(currentYear, currentMonth - 1, 1).toISOString()
    const end = new Date(currentYear, currentMonth, 0, 23, 59, 59).toISOString()
    const res = await fetch(
      `${BASE_URL}/Sales/history?startDate=${start}&endDate=${end}&page=1&pageSize=5&sortBy=date_desc`,
      { headers: { Authorization: `Bearer ${getToken()}` } },
    )
    if (res.ok) {
      const data = await res.json()
      monthlySales.value = {
        totalRevenue: data.totalRevenue ?? 0,
        totalItems: data.totalItems ?? 0,
      }
      recentSales.value = data.items ?? []
    }
  } catch {
    console.error('Satışlar çekilemedi.')
  } finally {
    isLoadingSales.value = false
  }
}

const fetchYearlySales = async () => {
  isLoadingChart.value = true
  try {
    const firstSlot = last12Slots[0]!
    const start = new Date(firstSlot.year, firstSlot.month - 1, 1).toISOString()
    const end = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59).toISOString()
    const res = await fetch(
      `${BASE_URL}/Sales/history?startDate=${start}&endDate=${end}&page=1&pageSize=9999`,
      { headers: { Authorization: `Bearer ${getToken()}` } },
    )
    if (res.ok) {
      const data = await res.json()
      yearlySales.value = data.items ?? []
    }
  } catch {
    console.error('Yıllık satışlar çekilemedi.')
  } finally {
    isLoadingChart.value = false
  }
}

const fetchExpenses = async () => {
  isLoadingExpenses.value = true
  try {
    const uniqueYears = [...new Set(last12Slots.map((s) => s.year))]
    const requests: Promise<Response>[] = [
      fetch(`${BASE_URL}/Expenses?page=1&pageSize=1&month=${currentMonth}&year=${currentYear}`, {
        headers: { Authorization: `Bearer ${getToken()}` },
      }),
      ...uniqueYears.map((y) =>
        fetch(`${BASE_URL}/Expenses/summary?year=${y}`, {
          headers: { Authorization: `Bearer ${getToken()}` },
        }),
      ),
    ]
    const results = await Promise.all(requests)
    const r1 = results[0]
    const yearResults = results.slice(1)
    if (r1?.ok) {
      const d = await r1.json()
      monthlyExpense.value = d.totalAmount ?? 0
    }
    const allSummaries: { month: number; year: number; totalAmount: number }[] = []
    for (let i = 0; i < yearResults.length; i++) {
      const yr = yearResults[i]
      const y = uniqueYears[i]
      if (yr?.ok && y !== undefined) {
        const items: { month: number; totalAmount: number }[] = await yr.json()
        allSummaries.push(...items.map((item) => ({ ...item, year: y })))
      }
    }
    expenseSummary.value = allSummaries
  } catch {
    console.error('Giderler çekilemedi.')
  } finally {
    isLoadingExpenses.value = false
  }
}

const fetchEmployeeSummary = async () => {
  isLoadingEmployees.value = true
  try {
    const res = await fetch(
      `${BASE_URL}/Employees/summary?month=${currentMonth}&year=${currentYear}`,
      { headers: { Authorization: `Bearer ${getToken()}` } },
    )
    if (res.ok) employeeSummary.value = await res.json()
  } catch {
    console.error('Personel özeti çekilemedi.')
  } finally {
    isLoadingEmployees.value = false
  }
}

const fetchDebtors = async () => {
  isLoadingCustomers.value = true
  try {
    const res = await fetch(`${BASE_URL}/Customers?debtFilter=has_debt&page=1&pageSize=999`, {
      headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (res.ok) {
      const data = await res.json()
      const items = data.items ?? []
      allDebtors.value = items
      totalDebt.value = items.reduce((s: number, c: any) => s + (c.currentBalance ?? 0), 0)
      debtorCount.value = items.length
    }
  } catch {
    console.error('Müşteri borçları çekilemedi.')
  } finally {
    isLoadingCustomers.value = false
  }
}

const fetchProducts = async () => {
  isLoadingProducts.value = true
  try {
    const res = await fetch(`${BASE_URL}/Products/search?page=1&pageSize=999`, {
      headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (res.ok) {
      const data = await res.json()
      const items: any[] = data.items ?? []
      allProducts.value = items
      totalProductCount.value = items.length
      totalInventoryValue.value = items.reduce((s, p) => s + (p.totalCostValue ?? 0), 0)
      outOfStockCount.value = items.filter((p) => (p.remainingQuantity ?? 0) === 0).length
    }
  } catch {
    console.error('Ürünler çekilemedi.')
  } finally {
    isLoadingProducts.value = false
  }
}

const fetchRawMaterials = async () => {
  isLoadingRawMaterials.value = true
  try {
    const res = await fetch(`${BASE_URL}/Products/raw-materials?page=1&pageSize=999`, {
      headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (res.ok) {
      const data = await res.json()
      const items: any[] = data.items ?? []
      totalRawMaterialCount.value = items.length
      totalRawMaterialValue.value = items.reduce((s, p) => s + (p.totalCostValue ?? 0), 0)
    }
  } catch {
    console.error('Hammaddeler çekilemedi.')
  } finally {
    isLoadingRawMaterials.value = false
  }
}

const fetchProduction = async () => {
  isLoadingProduction.value = true
  try {
    const start = new Date(currentYear, currentMonth - 1, 1).toISOString()
    const end = new Date(currentYear, currentMonth, 0, 23, 59, 59).toISOString()
    const res = await fetch(
      `${BASE_URL}/Production/logs?page=1&pageSize=999&startDate=${start}&endDate=${end}`,
      { headers: { Authorization: `Bearer ${getToken()}` } },
    )
    if (res.ok) {
      const data = await res.json()
      productionSummary.value = {
        totalQuantity: data.summaryTotalQuantity ?? 0,
        totalCost: data.summaryTotalCost ?? 0,
      }
    }
  } catch {
    console.error('Üretim özeti çekilemedi.')
  } finally {
    isLoadingProduction.value = false
  }
}

const fetchCategorySales = async () => {
  isLoadingCategory.value = true
  try {
    const res = await fetch(
      `${BASE_URL}/Sales/category-summary?month=${currentMonth}&year=${currentYear}`,
      { headers: { Authorization: `Bearer ${getToken()}` } },
    )
    if (res.ok) categorySalesData.value = await res.json()
  } catch {
    console.error('Kategori özeti çekilemedi.')
  } finally {
    isLoadingCategory.value = false
  }
}


onMounted(() => {
  fetchMonthlySales()
  fetchYearlySales()
  fetchExpenses()
  fetchEmployeeSummary()
  fetchDebtors()
  fetchProducts()
  fetchRawMaterials()
  fetchProduction()
  fetchCategorySales()
})
</script>
