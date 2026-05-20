<template>
  <div class="space-y-5 animate-fade-in">
    <!-- Başlık + Filtreler -->
    <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm space-y-4">
      <div class="flex flex-col md:flex-row items-start md:items-center justify-between gap-3">
        <div>
          <h3 class="text-lg font-bold text-gray-900">Üretim Geçmişi</h3>
          <p class="text-sm text-gray-500 mt-0.5">Tamamlanan tüm üretim emirlerinin kayıtları.</p>
        </div>
        <button
          @click="resetFilters"
          class="text-xs font-bold text-slate-500 hover:text-slate-700 bg-slate-100 hover:bg-slate-200 border border-slate-200 px-3.5 py-2 rounded-xl flex items-center gap-1.5 transition-all"
        >
          <RotateCcwIcon :size="13" /> Filtreleri Sıfırla
        </button>
      </div>

      <div class="flex flex-wrap gap-3">
        <div class="relative flex-1 min-w-[180px]">
          <SearchIcon class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="14" />
          <input
            v-model="filters.searchText"
            @input="onFilterChange"
            type="text"
            placeholder="Ürün adı ile ara..."
            class="pl-9 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-blue-500 focus:bg-white transition-all w-full font-medium"
          />
        </div>

        <div class="relative">
          <CalendarIcon
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            :size="14"
          />
          <input
            v-model="filters.startDate"
            @change="onFilterChange"
            type="date"
            class="pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-blue-500 focus:bg-white transition-all font-medium text-slate-700"
          />
        </div>

        <div class="relative">
          <CalendarIcon
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            :size="14"
          />
          <input
            v-model="filters.endDate"
            @change="onFilterChange"
            type="date"
            class="pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-blue-500 focus:bg-white transition-all font-medium text-slate-700"
          />
        </div>

        <CustomDropdown
          v-model="filters.sortBy"
          :options="sortByOptions"
          width-class="w-44"
          @update:model-value="onFilterChange"
        />

        <button
          @click="toggleSortDir"
          class="py-2 px-3.5 bg-slate-50 border border-slate-200 rounded-xl text-sm font-bold text-slate-600 hover:bg-slate-100 transition-all flex items-center gap-1.5"
        >
          <ArrowUpDownIcon :size="14" />
          {{ filters.isDescending ? 'Azalan' : 'Artan' }}
        </button>

        <CustomDropdown
          v-model="filters.pageSize"
          :options="pageSizeOptions"
          width-class="w-36"
          @update:model-value="onFilterChange"
        />
      </div>
    </div>

    <!-- İstatistik Kartları -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
      <div class="bg-blue-50 border border-blue-100 rounded-2xl p-3.5">
        <p class="text-[11px] font-bold text-blue-500 uppercase tracking-wider">Toplam Emir</p>
        <p class="text-2xl font-extrabold text-blue-700 mt-1">{{ summary.totalCount }}</p>
        <p class="text-[11px] text-blue-400 mt-0.5">kayıt</p>
      </div>
      <div class="bg-emerald-50 border border-emerald-100 rounded-2xl p-3.5">
        <p class="text-[11px] font-bold text-emerald-500 uppercase tracking-wider">Toplam Adet</p>
        <p class="text-2xl font-extrabold text-emerald-700 mt-1">{{ summary.totalQuantity }}</p>
        <p class="text-[11px] text-emerald-400 mt-0.5">üretildi</p>
      </div>
      <div class="bg-violet-50 border border-violet-100 rounded-2xl p-3.5">
        <p class="text-[11px] font-bold text-violet-500 uppercase tracking-wider">Toplam Maliyet</p>
        <p class="text-xl font-extrabold text-violet-700 mt-1">
          {{ formatCurrency(summary.totalCost) }}
        </p>
        <p class="text-[11px] text-violet-400 mt-0.5">hammadde</p>
      </div>
      <div class="bg-amber-50 border border-amber-100 rounded-2xl p-3.5">
        <p class="text-[11px] font-bold text-amber-500 uppercase tracking-wider">Farklı Ürün</p>
        <p class="text-2xl font-extrabold text-amber-700 mt-1">{{ summary.uniqueProducts }}</p>
        <p class="text-[11px] text-amber-400 mt-0.5">çeşit</p>
      </div>
    </div>

    <!-- Tablo -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col">
      <div class="overflow-x-auto custom-scrollbar">
        <table class="w-full text-left border-collapse table-auto">
          <thead>
            <tr
              class="bg-slate-50/60 border-b border-slate-100 text-slate-500 text-[11px] font-bold uppercase tracking-wider"
            >
              <th class="py-4 px-6">Üretilen Ürün</th>
              <th class="py-4 px-6 text-right">Üretilen Miktar</th>
              <th class="py-4 px-6 text-right">Toplam Maliyet</th>
              <th class="py-4 px-6 text-right">Birim Maliyet</th>
              <th class="py-4 px-6">Üretim Tarihi</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-slate-700 text-sm">
            <tr v-if="isLoading">
              <td colspan="5" class="py-12 text-center text-slate-400 font-medium">
                <div class="flex items-center justify-center gap-2">
                  <RefreshCwIcon :size="16" class="animate-spin" />
                  Yükleniyor...
                </div>
              </td>
            </tr>
            <tr v-else-if="logs.length === 0">
              <td colspan="5" class="py-16 text-center">
                <div class="flex flex-col items-center justify-center text-gray-400">
                  <HistoryIcon :size="44" class="mb-3 text-gray-300" />
                  <p class="font-medium text-gray-600">
                    {{
                      filters.searchText || filters.startDate || filters.endDate
                        ? 'Filtreye uyan kayıt bulunamadı.'
                        : 'Henüz hiç üretim yapılmamış.'
                    }}
                  </p>
                  <p class="text-xs mt-1 text-gray-400">
                    {{
                      filters.searchText || filters.startDate || filters.endDate
                        ? 'Farklı filtreler deneyin.'
                        : 'Üretim Paneli sekmesinden üretim emirleri oluşturabilirsiniz.'
                    }}
                  </p>
                </div>
              </td>
            </tr>
            <tr
              v-else
              v-for="log in logs"
              :key="log.id"
              class="hover:bg-slate-50/40 transition-colors"
            >
              <td class="py-4 px-6 align-middle">
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center shrink-0"
                  >
                    <FactoryIcon :size="18" />
                  </div>
                  <span class="font-bold text-gray-900">{{ log.productName }}</span>
                </div>
              </td>
              <td class="py-4 px-6 align-middle text-right">
                <span
                  class="inline-flex items-center gap-1 bg-emerald-50 text-emerald-700 border border-emerald-200 px-2.5 py-1 rounded-lg text-xs font-extrabold"
                >
                  {{ log.quantity }} Adet
                </span>
              </td>
              <td class="py-4 px-6 align-middle text-right font-bold text-slate-800">
                {{ formatCurrency(log.totalCost) }}
              </td>
              <td class="py-4 px-6 align-middle text-right text-slate-500 font-medium text-xs">
                {{ formatCurrency(log.unitCost) }} / adet
              </td>
              <td class="py-4 px-6 align-middle">
                <div class="flex flex-col">
                  <span class="font-semibold text-slate-700 text-xs">{{
                    formatDate(log.producedAt)
                  }}</span>
                  <span class="text-slate-400 text-[11px]">{{ formatTime(log.producedAt) }}</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Sayfalama -->
      <div class="border-t border-slate-100 px-6 py-4 flex items-center justify-between bg-slate-50/50">
      <p class="text-xs text-slate-500 font-medium">
        Toplam <span class="font-bold text-slate-700">{{ summary.totalCount }}</span> kayıt &mdash;
        Sayfa <span class="font-bold text-slate-700">{{ currentPage }}</span> /
        <span class="font-bold text-slate-700">{{ totalPages }}</span>
      </p>

      <div class="flex items-center gap-1.5">
        <button
          @click="goToPage(1)"
          :disabled="currentPage === 1 || isLoading"
          class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 text-slate-500 hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed transition-all text-xs font-bold"
        >
          «
        </button>
        <button
          @click="goToPage(currentPage - 1)"
          :disabled="currentPage === 1 || isLoading"
          class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 text-slate-500 hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed transition-all"
        >
          <ChevronLeftIcon :size="14" />
        </button>
        <button
          v-for="page in visiblePages"
          :key="page"
          @click="goToPage(page)"
          :disabled="isLoading"
          :class="[
            'w-8 h-8 flex items-center justify-center rounded-lg text-xs font-bold transition-all border',
            page === currentPage
              ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
              : 'border-slate-200 text-slate-600 hover:bg-slate-50',
          ]"
        >
          {{ page }}
        </button>
        <button
          @click="goToPage(currentPage + 1)"
          :disabled="currentPage === totalPages || isLoading"
          class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 text-slate-500 hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed transition-all"
        >
          <ChevronRightIcon :size="14" />
        </button>
        <button
          @click="goToPage(totalPages)"
          :disabled="currentPage === totalPages || isLoading"
          class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 text-slate-500 hover:bg-slate-50 disabled:opacity-30 disabled:cursor-not-allowed transition-all text-xs font-bold"
        >
          »
        </button>
      </div>
    </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  SearchIcon,
  RefreshCwIcon,
  HistoryIcon,
  FactoryIcon,
  CalendarIcon,
  ArrowUpDownIcon,
  RotateCcwIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from 'lucide-vue-next'
import CustomDropdown from '@/components/CustomDropdown.vue'

interface ProductionLog {
  id: string
  productName: string
  quantity: number
  totalCost: number
  unitCost: number
  producedAt: string
}

const logs = ref<ProductionLog[]>([])
const isLoading = ref(false)
const currentPage = ref(1)
const totalPages = ref(1)
const summary = ref({ totalCount: 0, totalQuantity: 0, totalCost: 0, uniqueProducts: 0 })

const sortByOptions = [
  { label: 'Tarihe Göre', value: 'date' },
  { label: 'Miktara Göre', value: 'quantity' },
  { label: 'Maliyete Göre', value: 'totalcost' },
]

const pageSizeOptions = [
  { label: '10 / sayfa', value: 10 },
  { label: '25 / sayfa', value: 25 },
  { label: '50 / sayfa', value: 50 },
]

const filters = ref({
  searchText: '',
  startDate: '',
  endDate: '',
  sortBy: 'date',
  isDescending: true,
  pageSize: 10,
})

let debounceTimer: ReturnType<typeof setTimeout> | null = null

const fetchLogs = async (page = 1) => {
  isLoading.value = true
  currentPage.value = page
  try {
    const token = localStorage.getItem('token')
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: filters.value.pageSize.toString(),
      sortBy: filters.value.sortBy,
      isDescending: filters.value.isDescending.toString(),
    })
    if (filters.value.searchText.trim())
      params.append('searchText', filters.value.searchText.trim())
    if (filters.value.startDate) params.append('startDate', filters.value.startDate)
    if (filters.value.endDate) params.append('endDate', filters.value.endDate)

    const res = await fetch(`/api/Production/logs?${params}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) {
      const data = await res.json()
      logs.value = data.items
      totalPages.value = data.totalPages
      summary.value = {
        totalCount: data.totalCount,
        totalQuantity: data.summaryTotalQuantity,
        totalCost: data.summaryTotalCost,
        uniqueProducts: data.summaryUniqueProducts,
      }
    }
  } catch (e) {
    console.error('Üretim geçmişi getirilemedi:', e)
  } finally {
    isLoading.value = false
  }
}

const onFilterChange = () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => fetchLogs(1), 400)
}

const toggleSortDir = () => {
  filters.value.isDescending = !filters.value.isDescending
  fetchLogs(1)
}

const goToPage = (page: number) => {
  if (page < 1 || page > totalPages.value || isLoading.value) return
  fetchLogs(page)
}

const resetFilters = () => {
  filters.value = {
    searchText: '',
    startDate: '',
    endDate: '',
    sortBy: 'date',
    isDescending: true,
    pageSize: 10,
  }
  fetchLogs(1)
}

const visiblePages = computed(() => {
  const pages: number[] = []
  const total = totalPages.value
  const current = currentPage.value
  let start = Math.max(1, current - 2)
  const end = Math.min(total, start + 4)
  if (end - start < 4) start = Math.max(1, end - 4)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val)
const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })
const formatTime = (d: string) =>
  new Date(d).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })

onMounted(() => fetchLogs(1))
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
  height: 5px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
