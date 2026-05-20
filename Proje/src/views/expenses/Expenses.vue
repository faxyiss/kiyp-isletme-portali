<template>
  <div class="space-y-6">
    <!-- Özet Kartlar -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-red-50 p-3 rounded-xl">
          <WalletIcon :size="20" class="text-red-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Bu Ay Toplam</p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">
            {{ isLoading ? '...' : formatCurrency(pagedData.totalAmount) }}
          </p>
        </div>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-blue-50 p-3 rounded-xl">
          <TagIcon :size="20" class="text-blue-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Kategori</p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">{{ categories.length }}</p>
        </div>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 flex items-center gap-4"
      >
        <div class="bg-emerald-50 p-3 rounded-xl">
          <ReceiptIcon :size="20" class="text-emerald-500" />
        </div>
        <div>
          <p class="text-xs font-semibold text-slate-400 uppercase tracking-wide">Kayıt Sayısı</p>
          <p class="text-xl font-bold text-slate-800 mt-0.5">
            {{ isLoading ? '...' : pagedData.totalCount }}
          </p>
        </div>
      </div>
    </div>

    <!-- Filtre & Aksiyon Barı -->
    <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <!-- Sol: Ay/Yıl ve Filtreler -->
        <div class="flex flex-wrap items-center gap-3 w-full md:w-auto z-30">
          <!-- Ay Seçici -->
          <div class="relative w-full sm:w-44">
            <button
              @click="toggleDropdown('month')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedMonthLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isMonthDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isMonthDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isMonthDropdownOpen = false"
            ></div>
            <div
              v-if="isMonthDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5 max-h-60 overflow-y-auto custom-scrollbar"
            >
              <button
                v-for="m in months"
                :key="m.value"
                @click="selectMonth(m.value)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  queryParams.month === m.value
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ m.label }}
              </button>
            </div>
          </div>

          <!-- Yıl Seçici -->
          <div class="relative w-full sm:w-32">
            <button
              @click="toggleDropdown('year')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ queryParams.year }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isYearDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isYearDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isYearDropdownOpen = false"
            ></div>
            <div
              v-if="isYearDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                v-for="y in years"
                :key="y"
                @click="selectYear(y)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  queryParams.year === y
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ y }}
              </button>
            </div>
          </div>

          <!-- Kategori Filtresi -->
          <div class="relative w-full sm:w-52">
            <button
              @click="toggleDropdown('category')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedCategoryLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isCategoryDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isCategoryDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isCategoryDropdownOpen = false"
            ></div>
            <div
              v-if="isCategoryDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-2 space-y-2 max-h-64 flex flex-col"
            >
              <div class="relative flex-shrink-0">
                <SearchIcon
                  class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400"
                  :size="14"
                />
                <input
                  v-model="categorySearchText"
                  type="text"
                  placeholder="Kategori ara..."
                  class="w-full pl-8 pr-3 py-1.5 border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 text-xs transition-all"
                  @click.stop
                />
              </div>
              <div class="overflow-y-auto flex-1 space-y-0.5 max-h-44 custom-scrollbar">
                <button
                  @click="selectCategory(null)"
                  type="button"
                  class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                  :class="
                    queryParams.categoryId === null
                      ? 'bg-blue-50 text-blue-600 font-bold'
                      : 'text-slate-600 hover:bg-slate-50'
                  "
                >
                  Tüm Kategoriler
                </button>
                <button
                  v-for="cat in filteredCategories"
                  :key="cat.id"
                  @click="selectCategory(cat.id)"
                  type="button"
                  class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors truncate"
                  :class="
                    queryParams.categoryId === cat.id
                      ? 'bg-blue-50 text-blue-600 font-bold'
                      : 'text-slate-600 hover:bg-slate-50'
                  "
                >
                  {{ cat.name }}
                </button>
                <div
                  v-if="filteredCategories.length === 0"
                  class="text-center py-3 text-xs text-slate-400"
                >
                  Kategori bulunamadı.
                </div>
              </div>
            </div>
          </div>

          <!-- Tip Filtresi -->
          <div class="relative w-full sm:w-44">
            <button
              @click="toggleDropdown('type')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedTypeLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isTypeDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isTypeDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isTypeDropdownOpen = false"
            ></div>
            <div
              v-if="isTypeDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                v-for="t in typeOptions"
                :key="String(t.value)"
                @click="selectType(t.value)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  queryParams.expenseType === t.value
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ t.label }}
              </button>
            </div>
          </div>
        </div>

        <!-- Sağ: Aksiyon Butonları -->
        <div class="flex items-center gap-3 w-full md:w-auto">
          <button
            @click="isCategoryModalOpen = true"
            class="flex items-center justify-center gap-2 border border-slate-200 hover:border-slate-300 hover:bg-slate-50 text-slate-600 px-4 py-2.5 rounded-xl font-medium transition-colors text-sm"
          >
            <TagIcon :size="16" />
            Kategori Ekle
          </button>
          <button
            @click="isExpenseModalOpen = true"
            class="flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-xl font-medium transition-colors text-sm shadow-sm shadow-blue-200"
          >
            <PlusIcon :size="18" />
            Gider Ekle
          </button>
        </div>
      </div>
    </div>

    <!-- Tablo -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
      <div class="overflow-x-auto custom-scrollbar">
        <table class="w-full text-left border-collapse table-auto">
          <thead>
            <tr
              class="bg-slate-50/60 border-b border-slate-100 text-slate-500 text-[11px] font-bold uppercase tracking-wider"
            >
              <th class="py-4 px-4 min-w-[200px]">Gider Başlığı</th>
              <th class="py-4 px-4 min-w-[140px]">Kategori</th>
              <th class="py-4 px-4 min-w-[130px]">Tip</th>
              <th class="py-4 px-4 min-w-[130px] text-red-600">Tutar</th>
              <th class="py-4 px-4 min-w-[130px]">Başlangıç</th>
              <th class="py-4 px-4 min-w-[130px]">Bitiş / Gün</th>
              <th class="py-4 px-4 min-w-[180px]">Notlar</th>
              <th class="py-4 px-4 text-right min-w-[100px]">İşlem</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-slate-700 text-sm">
            <tr v-if="isLoading">
              <td colspan="8" class="py-12 text-center text-slate-400 font-medium">
                Veriler sunucudan çekiliyor...
              </td>
            </tr>

            <tr v-else-if="expenses.length === 0">
              <td colspan="8" class="py-16 text-center">
                <div class="flex flex-col items-center gap-3 text-slate-400">
                  <div class="bg-slate-100 rounded-2xl p-5">
                    <ReceiptIcon :size="32" class="text-slate-300" />
                  </div>
                  <p class="font-semibold text-slate-500 text-sm">Gider kaydı bulunamadı</p>
                  <p class="text-xs text-slate-400">Seçili döneme ait kayıt yok.</p>
                </div>
              </td>
            </tr>

            <tr
              v-else
              v-for="expense in expenses"
              :key="expense.id"
              class="hover:bg-slate-50/60 transition-colors"
            >
              <!-- Başlık -->
              <td class="py-3.5 px-4">
                <span class="font-semibold text-slate-800 text-sm line-clamp-1">{{
                  expense.title
                }}</span>
              </td>

              <!-- Kategori -->
              <td class="py-3.5 px-4">
                <span
                  class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-semibold bg-slate-100 text-slate-600"
                >
                  {{ expense.categoryName }}
                </span>
              </td>

              <!-- Tip -->
              <td class="py-3.5 px-4">
                <span
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-semibold"
                  :class="expenseTypeBadge(expense.expenseType)"
                >
                  <component :is="expenseTypeIcon(expense.expenseType)" :size="11" />
                  {{ expense.expenseTypeLabel }}
                </span>
              </td>

              <!-- Tutar -->
              <td class="py-3.5 px-4">
                <span class="font-bold text-red-600 text-sm">{{
                  formatCurrency(expense.amount)
                }}</span>
              </td>

              <!-- Başlangıç -->
              <td class="py-3.5 px-4 text-slate-500 text-xs font-medium">
                {{ formatDate(expense.startDate) }}
              </td>

              <!-- Bitiş / Gün -->
              <td class="py-3.5 px-4 text-slate-500 text-xs font-medium">
                <span v-if="expense.expenseType === 2 && expense.endDate">{{
                  formatDate(expense.endDate)
                }}</span>
                <span v-else-if="expense.expenseType === 1 && expense.recurringDay">
                  Her ayın {{ expense.recurringDay }}. günü
                </span>
                <span v-else class="text-slate-300">—</span>
              </td>

              <!-- Notlar -->
              <td class="py-3.5 px-4 text-slate-400 text-xs max-w-[180px]">
                <span class="line-clamp-1">{{ expense.notes || '—' }}</span>
              </td>

              <!-- İşlem -->
              <td class="py-3.5 px-4 text-right">
                <button
                  @click="handleDelete(expense)"
                  class="p-2 rounded-lg text-slate-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                  title="Sil"
                >
                  <Trash2Icon :size="15" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Sayfalama -->
      <div
        v-if="pagedData.totalPages > 1"
        class="flex items-center justify-between px-5 py-3.5 border-t border-slate-100"
      >
        <p class="text-xs text-slate-400 font-medium">
          Toplam <span class="text-slate-600 font-bold">{{ pagedData.totalCount }}</span> kayıt
        </p>
        <div class="flex items-center gap-1">
          <button
            @click="changePage(queryParams.page - 1)"
            :disabled="queryParams.page <= 1"
            class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <ChevronLeftIcon :size="16" />
          </button>
          <button
            v-for="p in visiblePages"
            :key="p"
            @click="changePage(p)"
            class="min-w-[32px] h-8 px-2 rounded-lg text-xs font-semibold transition-colors"
            :class="
              queryParams.page === p
                ? 'bg-blue-600 text-white shadow-sm'
                : 'text-slate-500 hover:bg-slate-100'
            "
          >
            {{ p }}
          </button>
          <button
            @click="changePage(queryParams.page + 1)"
            :disabled="queryParams.page >= pagedData.totalPages"
            class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <ChevronRightIcon :size="16" />
          </button>
        </div>
      </div>
    </div>

    <!-- Modaller -->
    <AddExpenseModal
      :is-open="isExpenseModalOpen"
      :categories="categories"
      @close="isExpenseModalOpen = false"
      @success="onExpenseAdded"
      @open-category-modal="isCategoryModalOpen = true"
    />

    <AddExpenseCategoryModal
      :is-open="isCategoryModalOpen"
      @close="isCategoryModalOpen = false"
      @success="onCategoryAdded"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  WalletIcon,
  TagIcon,
  ReceiptIcon,
  PlusIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  SearchIcon,
  Trash2Icon,
  ZapIcon,
  RepeatIcon,
  CalendarRangeIcon,
} from 'lucide-vue-next'

import AddExpenseModal from './AddExpenseModal.vue'
import AddExpenseCategoryModal from './AddExpenseCategoryModal.vue'
import { useAlert } from '@/composables/useAlert'

const { showError, showConfirm } = useAlert()

const BASE_URL = '/api'

// ── State ──────────────────────────────────────────────────
const isLoading = ref(false)
const isExpenseModalOpen = ref(false)
const isCategoryModalOpen = ref(false)

const expenses = ref<any[]>([])
const categories = ref<{ id: string; name: string }[]>([])

const pagedData = ref({
  totalCount: 0,
  totalPages: 1,
  currentPage: 1,
  totalAmount: 0,
})

const now = new Date()
const queryParams = ref({
  page: 1,
  pageSize: 10,
  month: now.getMonth() + 1,
  year: now.getFullYear(),
  categoryId: null as string | null,
  expenseType: null as number | null,
})

// ── Dropdown State ─────────────────────────────────────────
const isMonthDropdownOpen = ref(false)
const isYearDropdownOpen = ref(false)
const isCategoryDropdownOpen = ref(false)
const isTypeDropdownOpen = ref(false)
const categorySearchText = ref('')

const toggleDropdown = (type: 'month' | 'year' | 'category' | 'type') => {
  isMonthDropdownOpen.value = type === 'month' ? !isMonthDropdownOpen.value : false
  isYearDropdownOpen.value = type === 'year' ? !isYearDropdownOpen.value : false
  isCategoryDropdownOpen.value = type === 'category' ? !isCategoryDropdownOpen.value : false
  isTypeDropdownOpen.value = type === 'type' ? !isTypeDropdownOpen.value : false
}

// ── Statik Veriler ─────────────────────────────────────────
const months = [
  { value: 1, label: 'Ocak' },
  { value: 2, label: 'Şubat' },
  { value: 3, label: 'Mart' },
  { value: 4, label: 'Nisan' },
  { value: 5, label: 'Mayıs' },
  { value: 6, label: 'Haziran' },
  { value: 7, label: 'Temmuz' },
  { value: 8, label: 'Ağustos' },
  { value: 9, label: 'Eylül' },
  { value: 10, label: 'Ekim' },
  { value: 11, label: 'Kasım' },
  { value: 12, label: 'Aralık' },
]

const years = Array.from({ length: 5 }, (_, i) => now.getFullYear() - 2 + i)

const typeOptions = [
  { value: null, label: 'Tüm Tipler' },
  { value: 0, label: 'Tek Seferlik' },
  { value: 1, label: 'Aylık Sabit' },
  { value: 2, label: 'Dönemsel' },
]

// ── Computed ───────────────────────────────────────────────
const selectedMonthLabel = computed(
  () => months.find((m) => m.value === queryParams.value.month)?.label ?? 'Ay Seç',
)

const selectedCategoryLabel = computed(() => {
  if (!queryParams.value.categoryId) return 'Tüm Kategoriler'
  return (
    categories.value.find((c) => c.id === queryParams.value.categoryId)?.name ?? 'Tüm Kategoriler'
  )
})

const selectedTypeLabel = computed(
  () => typeOptions.find((t) => t.value === queryParams.value.expenseType)?.label ?? 'Tüm Tipler',
)

const filteredCategories = computed(() => {
  if (!categorySearchText.value.trim()) return categories.value
  return categories.value.filter((c) =>
    c.name.toLowerCase().includes(categorySearchText.value.toLowerCase()),
  )
})

const visiblePages = computed(() => {
  const total = pagedData.value.totalPages
  const current = queryParams.value.page
  const pages: number[] = []
  for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) pages.push(i)
  return pages
})

// ── Helpers ────────────────────────────────────────────────
const formatCurrency = (val: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val)

const formatDate = (dateStr: string) =>
  new Date(dateStr).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

const expenseTypeBadge = (type: number) => {
  if (type === 0) return 'bg-amber-50 text-amber-700'
  if (type === 1) return 'bg-blue-50 text-blue-700'
  return 'bg-emerald-50 text-emerald-700'
}

const expenseTypeIcon = (type: number) => {
  if (type === 0) return ZapIcon
  if (type === 1) return RepeatIcon
  return CalendarRangeIcon
}

// ── Seçimler ───────────────────────────────────────────────
const selectMonth = (val: number) => {
  queryParams.value.month = val
  isMonthDropdownOpen.value = false
  fetchExpenses()
}

const selectYear = (val: number) => {
  queryParams.value.year = val
  isYearDropdownOpen.value = false
  fetchExpenses()
}

const selectCategory = (id: string | null) => {
  queryParams.value.categoryId = id
  isCategoryDropdownOpen.value = false
  categorySearchText.value = ''
  fetchExpenses()
}

const selectType = (val: number | null) => {
  queryParams.value.expenseType = val
  isTypeDropdownOpen.value = false
  fetchExpenses()
}

const changePage = (p: number) => {
  if (p < 1 || p > pagedData.value.totalPages) return
  queryParams.value.page = p
  fetchExpenses()
}

// ── API Çağrıları ──────────────────────────────────────────
const fetchCategories = async () => {
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Expenses/categories`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) categories.value = await res.json()
  } catch {
    console.error('Kategoriler çekilemedi.')
  }
}

const fetchExpenses = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const params = new URLSearchParams()
    params.append('page', queryParams.value.page.toString())
    params.append('pageSize', queryParams.value.pageSize.toString())
    params.append('month', queryParams.value.month.toString())
    params.append('year', queryParams.value.year.toString())
    if (queryParams.value.categoryId) params.append('categoryId', queryParams.value.categoryId)
    if (queryParams.value.expenseType !== null)
      params.append('expenseType', queryParams.value.expenseType.toString())

    const res = await fetch(`${BASE_URL}/Expenses?${params.toString()}`, {
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    })

    if (res.ok) {
      const data = await res.json()
      expenses.value = data.items
      pagedData.value = {
        totalCount: data.totalCount,
        totalPages: data.totalPages,
        currentPage: data.currentPage,
        totalAmount: data.totalAmount,
      }
    }
  } catch {
    console.error('Giderler çekilemedi.')
  } finally {
    isLoading.value = false
  }
}

const handleDelete = async (expense: any) => {
  const ok = await showConfirm(`"${expense.title}" giderini silmek istediğinize emin misiniz?`)
  if (!ok) return

  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Expenses/${expense.id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })
    const data = await res.json()
    if (res.ok) {
      fetchExpenses()
    } else {
      showError(data.message || 'Silme işlemi başarısız.')
    }
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  }
}

// ── Modal Callbackler ──────────────────────────────────────
const onExpenseAdded = () => {
  fetchExpenses()
}

const onCategoryAdded = (cat: { id: string; name: string }) => {
  categories.value.push(cat)
  categories.value.sort((a, b) => a.name.localeCompare(b.name))
}

// ── Yaşam Döngüsü ──────────────────────────────────────────
onMounted(() => {
  fetchCategories()
  fetchExpenses()
})
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
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
