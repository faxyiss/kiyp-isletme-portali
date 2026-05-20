<template>
  <div class="space-y-5 animate-fade-in">
    <div
      class="bg-gradient-to-r from-blue-600 to-indigo-700 rounded-2xl p-5 text-white shadow-md flex justify-between items-center"
    >
      <div class="space-y-1">
        <p class="text-xs text-blue-100 font-bold uppercase tracking-wider">
          Filtrelenmiş Toplam Ciro
        </p>
        <h3 class="text-2xl md:text-3xl font-black">{{ formatCurrency(totalRevenue) }}</h3>
      </div>
      <div class="bg-white/10 p-3 rounded-xl backdrop-blur-xs">
        <TrendingUpIcon :size="28" class="text-white" />
      </div>
    </div>

    <div class="bg-white p-4 rounded-2xl border border-slate-100 shadow-sm space-y-4">
      <div class="flex flex-col md:flex-row gap-3">
        <div class="relative flex-1">
          <SearchIcon
            class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
            :size="18"
          />
          <input
            v-model="filters.search"
            @input="emitFilterChange"
            type="text"
            placeholder="Ürün adı veya müşteri adı ile ara..."
            class="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-blue-500 transition-all text-slate-700 bg-slate-50/50"
          />
        </div>

        <div class="flex gap-2">
          <button
            v-if="hasActiveFilters"
            @click="resetFilters"
            class="px-4 py-2.5 rounded-xl border border-rose-200 text-xs font-bold text-rose-600 hover:bg-rose-50 flex items-center justify-center gap-1.5 cursor-pointer transition-colors whitespace-nowrap"
          >
            <RotateCcwIcon :size="14" />
            Sıfırla
          </button>

          <button
            @click="showAdvancedFilters = !showAdvancedFilters"
            class="px-4 py-2.5 rounded-xl border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-50 flex items-center justify-center gap-2 cursor-pointer transition-colors whitespace-nowrap"
          >
            <SlidersHorizontalIcon :size="14" />
            {{ showAdvancedFilters ? 'Filtreleri Gizle' : 'Gelişmiş Filtreler' }}
          </button>
        </div>
      </div>

      <div
        v-if="showAdvancedFilters"
        class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-3 pt-3 border-t border-slate-100 animate-slide-down"
      >
        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Başlangıç Tarihi</label>
          <input
            v-model="filters.startDate"
            @change="emitFilterChange"
            type="date"
            class="w-full px-3 py-2 border border-slate-200 rounded-lg text-xs bg-white focus:outline-none focus:border-blue-500"
          />
        </div>
        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Bitiş Tarihi</label>
          <input
            v-model="filters.endDate"
            @change="emitFilterChange"
            type="date"
            class="w-full px-3 py-2 border border-slate-200 rounded-lg text-xs bg-white focus:outline-none focus:border-blue-500"
          />
        </div>

        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Ödeme Türü</label>
          <CustomDropdown
            v-model="filters.paymentType"
            :options="paymentTypeOptions"
            @update:model-value="emitFilterChange"
          />
        </div>

        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Müşteri</label>
          <CustomDropdown
            v-model="filters.customerId"
            :options="customerOptions"
            :searchable="true"
            search-placeholder="Müşteri ara..."
            @update:model-value="emitFilterChange"
          />
        </div>

        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Kategori</label>
          <CustomDropdown
            v-model="filters.categoryId"
            :options="categoryOptions"
            :searchable="true"
            search-placeholder="Kategori ara..."
            @update:model-value="emitFilterChange"
          />
        </div>

        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Min Tutar (₺)</label>
          <input
            v-model.number="filters.minAmount"
            @input="emitFilterChange"
            type="number"
            placeholder="0.00"
            class="w-full px-3 py-2 border border-slate-200 rounded-lg text-xs bg-white focus:outline-none focus:border-blue-500"
          />
        </div>
        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Max Tutar (₺)</label>
          <input
            v-model.number="filters.maxAmount"
            @input="emitFilterChange"
            type="number"
            placeholder="99999"
            class="w-full px-3 py-2 border border-slate-200 rounded-lg text-xs bg-white focus:outline-none focus:border-blue-500"
          />
        </div>

        <div class="space-y-1">
          <label class="text-[11px] font-bold text-slate-500">Sıralama Ölçütü</label>
          <CustomDropdown
            v-model="filters.sortBy"
            :options="sortByOptions"
            @update:model-value="emitFilterChange"
          />
        </div>
      </div>
    </div>

    <div
      class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col justify-between min-h-[450px]"
    >
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr
              class="bg-slate-50 border-b border-slate-100 text-xs font-bold uppercase tracking-wider text-slate-500"
            >
              <th class="py-4 px-6">Tarih / Saat</th>
              <th class="py-4 px-6">Ürün Detayı</th>
              <th class="py-4 px-6">Müşteri Bilgisi</th>
              <th class="py-4 px-6 text-center">Miktar</th>
              <th class="py-4 px-6 text-center">Ödeme Yöntemi</th>
              <th class="py-4 px-6 text-right">Toplam Tutar</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-sm text-slate-600">
            <tr v-if="salesList.length === 0">
              <td colspan="6" class="py-16 text-center text-slate-400 font-medium">
                Aradığınız filtrelere uygun herhangi bir geçmiş satış kaydı bulunamadı.
              </td>
            </tr>
            <tr
              v-for="sale in salesList"
              :key="sale.id"
              class="hover:bg-slate-50/40 transition-colors"
            >
              <td class="py-4 px-6 text-xs font-medium text-slate-500">
                {{ formatDate(sale.saleDate) }}<br />
                <span class="text-[10px] text-slate-400 font-normal">{{
                  formatTime(sale.saleDate)
                }}</span>
              </td>
              <td class="py-4 px-6">
                <div class="font-bold text-slate-800 text-xs md:text-sm">
                  {{ sale.productName }}
                </div>
                <div class="text-[11px] text-slate-400 mt-0.5">
                  Birim: {{ formatCurrency(sale.salePrice) }}
                </div>
              </td>
              <td class="py-4 px-6">
                <span
                  v-if="sale.customerName && sale.customerName !== 'Genel Müşteri'"
                  class="font-semibold text-slate-700 flex items-center gap-1.5 text-xs bg-slate-100 px-2 py-1 rounded-md w-max"
                >
                  <UserIcon :size="12" class="text-slate-500" /> {{ sale.customerName }}
                </span>
                <span v-else class="text-xs text-slate-400 italic">Genel Müşteri (Perakende)</span>
              </td>
              <td class="py-4 px-6 text-center font-black text-slate-700">{{ sale.quantity }}</td>
              <td class="py-4 px-6 text-center">
                <span
                  v-if="sale.paymentType === 0"
                  class="px-2.5 py-1 rounded-md text-[10px] font-black bg-emerald-50 text-emerald-600 border border-emerald-100 uppercase tracking-wider"
                  >Nakit</span
                >
                <span
                  v-else-if="sale.paymentType === 1"
                  class="px-2.5 py-1 rounded-md text-[10px] font-black bg-blue-50 text-blue-600 border border-blue-100 uppercase tracking-wider"
                  >Kredi Kartı</span
                >
                <span
                  v-else-if="sale.paymentType === 2"
                  class="px-2.5 py-1 rounded-md text-[10px] font-black bg-rose-50 text-rose-600 border border-rose-100 uppercase tracking-wider"
                  >Veresiye</span
                >
              </td>
              <td class="py-4 px-6 text-right font-black text-slate-900 text-sm">
                {{ formatCurrency(sale.totalAmount) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div
        v-if="totalPages > 0"
        class="flex justify-between items-center p-4 border-t border-slate-100 bg-slate-50/50 shrink-0"
      >
        <button
          @click="$emit('page-changed', currentPage - 1)"
          :disabled="currentPage === 1"
          class="px-4 py-1.5 text-xs font-bold rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer"
        >
          Önceki Sayfa
        </button>
        <span
          class="text-xs font-bold text-slate-600 bg-white shadow-xs border border-slate-200 px-4 py-1.5 rounded-xl"
        >
          Sayfa {{ currentPage }} / {{ totalPages }}
        </span>
        <button
          @click="$emit('page-changed', currentPage + 1)"
          :disabled="currentPage === totalPages"
          class="px-4 py-1.5 text-xs font-bold rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer"
        >
          Sonraki Sayfa
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, toRefs } from 'vue'
import axios from 'axios'
import CustomDropdown from '@/components/CustomDropdown.vue'
import {
  SearchIcon,
  UserIcon,
  SlidersHorizontalIcon,
  TrendingUpIcon,
  RotateCcwIcon, // Yeni eklendi: Sıfırla butonu ikonu
} from 'lucide-vue-next'

interface Sale {
  id: string
  productName: string
  customerName: string | null
  quantity: number
  salePrice: number
  totalAmount: number
  paymentType: number
  saleDate: string
}
interface Customer {
  id: string
  firstName: string
  lastName: string
}
interface Category {
  id: string
  name: string
}

const props = defineProps<{
  salesList: Sale[]
  customersList: Customer[]
  currentPage: number
  totalPages: number
  totalRevenue: number
}>()

const { salesList, customersList, currentPage, totalPages, totalRevenue } = toRefs(props)

const emit = defineEmits(['filter-changed', 'page-changed'])

const showAdvancedFilters = ref(false)
const categories = ref<Category[]>([])

const paymentTypeOptions = [
  { label: 'Tüm Ödeme Türleri', value: null },
  { label: 'Nakit', value: 0 },
  { label: 'Kredi Kartı', value: 1 },
  { label: 'Veresiye', value: 2 },
]

const sortByOptions = [
  { label: 'Tarih: En Yeniden En Eskiye', value: 'date_desc' },
  { label: 'Tarih: En Eskiden En Yeniye', value: 'date_asc' },
  { label: 'Tutar: En Yüksekten En Düşüğe', value: 'amount_desc' },
  { label: 'Tutar: En Düşükten En Yükseğe', value: 'amount_asc' },
]

const customerOptions = computed(() => [
  { label: 'Tüm Müşteriler', value: null },
  ...customersList.value.map((c) => ({ label: `${c.firstName} ${c.lastName}`, value: c.id })),
])

const categoryOptions = computed(() => [
  { label: 'Tüm Kategoriler', value: null },
  ...categories.value.map((cat) => ({ label: cat.name, value: cat.id })),
])

// Backend formatına tam uyumlu filtre state nesnesi
const filters = ref({
  search: '',
  startDate: '',
  endDate: '',
  paymentType: null as number | null,
  categoryId: null as string | null,
  customerId: null as string | null,
  minAmount: null as number | null,
  maxAmount: null as number | null,
  sortBy: 'date_desc',
})

onMounted(async () => {
  try {
    const token = localStorage.getItem('token')
    const res = await axios.get('/Categories', { headers: { Authorization: `Bearer ${token}` } })
    categories.value = res.data
  } catch (error) {
    console.error('Kategoriler yüklenemedi', error)
  }
})

// Kullanıcı herhangi bir arama, sıralama veya gelişmiş filtre yaptıysa butonu gösteren tetikleyici
const hasActiveFilters = computed(() => {
  const f = filters.value
  return (
    f.search !== '' ||
    f.startDate !== '' ||
    f.endDate !== '' ||
    f.paymentType !== null ||
    f.categoryId !== null ||
    f.customerId !== null ||
    f.minAmount !== null ||
    f.maxAmount !== null ||
    f.sortBy !== 'date_desc'
  )
})

// Tüm filtreleri varsayılan değerine döndüren fonksiyon
const resetFilters = () => {
  filters.value = {
    search: '',
    startDate: '',
    endDate: '',
    paymentType: null,
    categoryId: null,
    customerId: null,
    minAmount: null,
    maxAmount: null,
    sortBy: 'date_desc',
  }
  emitFilterChange() // Değişikliği ana bileşene bildir
}

// Değişiklikleri anında yukarı (Sales.vue'ya) bildirir
const emitFilterChange = () => {
  emit('filter-changed', { ...filters.value })
}

// Yardımcı Biçimlendirme Araçları
const formatCurrency = (value: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value)

const formatDate = (dateString: string) =>
  new Intl.DateTimeFormat('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' }).format(
    new Date(dateString),
  )

const formatTime = (dateString: string) =>
  new Intl.DateTimeFormat('tr-TR', { hour: '2-digit', minute: '2-digit' }).format(
    new Date(dateString),
  )
</script>

<style scoped>
/* Filtre açılış animasyonu */
.animate-slide-down {
  animation: slideDown 0.25s ease-out forwards;
}
@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
