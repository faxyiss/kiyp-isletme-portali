<template>
  <div
    class="bg-white rounded-2xl border border-slate-100 shadow-sm p-4 flex flex-col h-[calc(100vh-190px)] justify-between"
  >
    <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 mb-4 space-y-3 shrink-0">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
        <div class="relative md:col-span-2">
          <svg
            class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            ></path>
          </svg>
          <input
            v-model="filters.searchText"
            @input="triggerFilterChange"
            type="text"
            placeholder="Ürün adı, kodu veya açıklaması ile ara..."
            class="w-full pl-9 pr-3 py-2 rounded-lg border border-slate-200 text-xs focus:ring-2 focus:ring-blue-500 bg-white"
          />
        </div>
        <div>
          <CustomDropdown
            v-model="filters.sortBy"
            :options="sortOptions"
            @update:model-value="triggerFilterChange"
          />
        </div>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-5 gap-3 items-center">
        <div class="md:col-span-2">
          <CustomDropdown
            v-model="filters.categoryId"
            :options="categoryOptions"
            :searchable="true"
            search-placeholder="Kategori ara..."
            @update:model-value="triggerFilterChange"
          />
        </div>

        <input
          v-model.number="filters.minPrice"
          @input="triggerFilterChange"
          type="number"
          placeholder="Min ₺"
          class="w-full px-3 py-2 rounded-lg border border-slate-200 text-xs bg-white"
        />
        <input
          v-model.number="filters.maxPrice"
          @input="triggerFilterChange"
          type="number"
          placeholder="Max ₺"
          class="w-full px-3 py-2 rounded-lg border border-slate-200 text-xs bg-white"
        />

        <div
          class="flex items-center justify-center bg-white border border-slate-200 rounded-lg py-2 cursor-pointer transition-colors hover:bg-slate-50"
          @click.prevent="toggleInStock"
        >
          <input
            type="checkbox"
            :checked="filters.inStock === true"
            class="w-3.5 h-3.5 text-blue-600 rounded border-gray-300 mr-2 cursor-pointer pointer-events-none"
          />
          <span class="text-[11px] font-bold text-slate-600 select-none">Stokta Olanlar</span>
        </div>
      </div>
    </div>

    <div
      class="flex-1 overflow-y-auto pr-2 grid grid-cols-2 lg:grid-cols-3 gap-3 content-start custom-scrollbar"
    >
      <div
        v-if="productsList.length === 0"
        class="col-span-full py-10 text-center text-slate-400 text-sm"
      >
        Filtrelere uygun ürün bulunamadı.
      </div>

      <button
        v-for="product in productsList"
        :key="product.id"
        @click="$emit('select', product)"
        :disabled="product.remainingQuantity <= 0"
        class="text-left p-3 rounded-xl border transition-all group flex flex-col justify-between h-[160px] relative overflow-hidden"
        :class="[
          product.remainingQuantity <= 0
            ? 'bg-slate-50/50 border-slate-200 opacity-60 cursor-not-allowed grayscale-[20%]'
            : selectedProductId === product.id
              ? 'border-blue-500 bg-blue-50 ring-2 ring-blue-100 cursor-pointer shadow-sm'
              : 'border-slate-200 hover:border-blue-300 hover:bg-white cursor-pointer bg-white shadow-sm',
        ]"
      >
        <div class="w-full flex justify-between items-start mb-2 shrink-0">
          <span
            class="text-[10px] font-bold text-slate-500 bg-white px-1.5 py-0.5 rounded border border-slate-100"
          >
            #{{ product.productNo }}
          </span>
          <span
            class="text-[10px] font-bold px-1.5 py-0.5 rounded"
            :class="
              product.remainingQuantity > 0
                ? 'bg-emerald-50 text-emerald-600'
                : 'bg-red-50 text-red-600'
            "
          >
            Stok: {{ product.remainingQuantity }}
          </span>
        </div>

        <div class="flex-1 flex flex-col justify-center min-h-0">
          <div
            class="flex items-center gap-1 mb-1"
            :class="product.remainingQuantity > 0 ? 'text-blue-500' : 'text-slate-400'"
          >
            <PackageIcon :size="14" />
            <span class="text-[9px] font-bold uppercase tracking-wider text-slate-500 truncate">
              {{ product.categoryName || 'Genel Kategori' }}
            </span>
          </div>

          <h4
            class="font-extrabold text-base text-slate-800 line-clamp-2 leading-tight"
            :class="product.remainingQuantity > 0 ? 'group-hover:text-blue-700' : ''"
            :title="product.name"
          >
            {{ product.name }}
          </h4>

          <p
            v-if="product.description"
            class="text-[10px] text-slate-500 mt-1.5 line-clamp-2 leading-snug font-medium"
            :title="product.description"
          >
            {{ product.description }}
          </p>
        </div>

        <p
          class="text-sm font-black w-full text-right mt-2 shrink-0"
          :class="product.remainingQuantity > 0 ? 'text-blue-600' : 'text-slate-500'"
        >
          {{ formatCurrency(product.unitPrice) }}
        </p>
      </button>
    </div>

    <div
      v-if="totalPages > 0"
      class="flex justify-between items-center pt-3 mt-2 border-t border-slate-100 shrink-0"
    >
      <button
        @click="$emit('page-changed', currentPage - 1)"
        :disabled="currentPage === 1"
        class="px-4 py-1.5 text-xs font-bold rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
      >
        Önceki
      </button>
      <span class="text-xs font-bold text-slate-600 bg-slate-100 px-3 py-1 rounded-lg"
        >Sayfa {{ currentPage }} / {{ totalPages }}</span
      >
      <button
        @click="$emit('page-changed', currentPage + 1)"
        :disabled="currentPage === totalPages"
        class="px-4 py-1.5 text-xs font-bold rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
      >
        Sonraki
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { PackageIcon } from 'lucide-vue-next'
import CustomDropdown from '@/components/CustomDropdown.vue'

// INTERFACE GÜNCELLENDİ: description eklendi
interface Product {
  id: string
  productNo: number
  name: string
  description?: string
  unitPrice: number
  remainingQuantity: number
  categoryName?: string
}

interface Category {
  id: string
  name: string
}

defineProps<{
  productsList: Product[]
  selectedProductId?: string | null
  currentPage: number
  totalPages: number
}>()
const emit = defineEmits(['select', 'filter-changed', 'page-changed'])

const filters = ref({
  searchText: '',
  categoryId: null as string | null,
  inStock: true as boolean | null,
  minPrice: null as number | null,
  maxPrice: null as number | null,
  sortBy: 'productno',
})

const categories = ref<Category[]>([])

const sortOptions = [
  { label: 'Sırala: Ürün Kodu', value: 'productno' },
  { label: 'Sırala: İsim (A-Z)', value: 'name' },
  { label: 'Sırala: Fiyat', value: 'price' },
]

const categoryOptions = computed(() => [
  { label: 'Tüm Kategoriler', value: null },
  ...categories.value.map((cat) => ({ label: cat.name, value: cat.id })),
])

onMounted(async () => {
  try {
    const token = localStorage.getItem('token')
    const res = await axios.get('/Categories', { headers: { Authorization: `Bearer ${token}` } })
    categories.value = res.data
  } catch (error) {
    console.error('Kategoriler yüklenemedi', error)
  }
})

const toggleInStock = () => {
  filters.value.inStock = filters.value.inStock === true ? null : true
  triggerFilterChange()
}

const triggerFilterChange = () => {
  emit('filter-changed', { ...filters.value })
}

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value)
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}
</style>
