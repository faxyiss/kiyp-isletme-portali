<template>
  <div class="space-y-5 animate-fade-in">
    <!-- Başlık -->
    <div
      class="flex flex-col md:flex-row items-start md:items-center justify-between p-4 md:p-6 bg-white rounded-2xl border border-slate-100 shadow-sm gap-4"
    >
      <div class="flex items-center gap-3">
        <div class="bg-blue-600/10 p-2.5 rounded-xl">
          <PackageIcon class="text-blue-600" :size="24" />
        </div>
        <div>
          <h2 class="text-xl font-bold text-slate-800">Hammadde Deposu</h2>
          <p class="text-sm text-slate-500 mt-0.5">
            Üretime girecek materyallerin stok ve maliyet takibi
          </p>
        </div>
      </div>
      <div class="flex items-center gap-3 w-full md:w-auto">
        <button
          @click="isCategoryModalOpen = true"
          class="flex-1 md:flex-none flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium text-slate-700 bg-white border border-slate-200 rounded-xl hover:bg-slate-50 hover:border-slate-300 transition-all shadow-sm"
        >
          <FolderPlusIcon :size="16" class="text-slate-500" /> Kategori Ekle
        </button>
        <button
          @click="isAddModalOpen = true"
          class="flex-1 md:flex-none flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium text-white bg-blue-600 rounded-xl hover:bg-blue-700 transition-all shadow-sm shadow-blue-200"
        >
          <PlusIcon :size="16" /> Yeni Hammadde
        </button>
      </div>
    </div>

    <!-- Filtreler -->
    <div class="p-4 md:p-5 bg-white rounded-2xl border border-slate-100 shadow-sm flex flex-col sm:flex-row gap-4">
      <div class="flex-1 relative">
        <SearchIcon class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
        <input
          v-model="searchText"
          @input="handleFilters"
          type="text"
          placeholder="Hammadde adı veya kodu ile ara..."
          class="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all"
        />
      </div>

      <!-- Stok Durumu Filtresi -->
      <CustomDropdown
        v-model="stockFilter"
        :options="stockFilterOptions"
        width-class="w-full sm:w-52"
        @update:model-value="handleFilters"
      />

      <!-- Kategori Dropdown -->
      <div class="w-full sm:w-64 relative" @click.stop>
        <div
          @click="toggleCategoryDropdown"
          class="w-full pl-10 pr-8 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm flex items-center justify-between cursor-pointer hover:border-slate-300 transition-all"
          :class="{ 'ring-2 ring-blue-500/20 border-blue-500 bg-white': isCategoryDropdownOpen }"
        >
          <FilterIcon class="absolute left-3 text-slate-400" :size="18" />
          <span
            class="truncate select-none font-medium"
            :class="selectedCategory ? 'text-blue-700' : 'text-slate-600'"
          >
            {{ selectedCategoryName }}
          </span>
          <ChevronDownIcon
            class="text-slate-400 transition-transform duration-200"
            :class="{ 'rotate-180': isCategoryDropdownOpen }"
            :size="16"
          />
        </div>
        <div
          v-if="isCategoryDropdownOpen"
          class="absolute top-full left-0 mt-1.5 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-30 overflow-hidden flex flex-col origin-top animate-fade-in"
        >
          <div class="p-2 border-b border-slate-100 relative bg-slate-50/50">
            <SearchIcon
              class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
              :size="14"
            />
            <input
              v-model="categorySearchQuery"
              type="text"
              placeholder="Kategori ara..."
              class="w-full pl-8 pr-3 py-1.5 bg-white border border-slate-200 rounded-lg text-xs focus:outline-none focus:border-blue-500 transition-all"
            />
          </div>
          <div class="max-h-56 overflow-y-auto p-1.5 custom-scrollbar">
            <button
              @click="selectCategoryFilter('')"
              class="w-full text-left px-3 py-2 rounded-lg text-sm transition-colors flex items-center justify-between"
              :class="
                selectedCategory === ''
                  ? 'bg-blue-50 text-blue-700 font-bold'
                  : 'text-slate-600 hover:bg-slate-50'
              "
            >
              Tüm Kategoriler
              <CheckIcon v-if="selectedCategory === ''" :size="14" class="text-blue-600" />
            </button>
            <div class="h-px bg-slate-100 my-1 mx-2"></div>
            <button
              v-for="cat in filteredCategoryList"
              :key="cat.id"
              @click="selectCategoryFilter(cat.id)"
              class="w-full text-left px-3 py-2 rounded-lg text-sm transition-colors flex items-center justify-between truncate"
              :class="
                selectedCategory === cat.id
                  ? 'bg-blue-50 text-blue-700 font-bold'
                  : 'text-slate-600 hover:bg-slate-50'
              "
            >
              {{ cat.name }}
              <CheckIcon
                v-if="selectedCategory === cat.id"
                :size="14"
                class="text-blue-600 flex-shrink-0"
              />
            </button>
            <div
              v-if="filteredCategoryList.length === 0"
              class="px-3 py-4 text-center text-xs text-slate-400 font-medium"
            >
              Eşleşen kategori bulunamadı
            </div>
          </div>
        </div>
      </div>

      <!-- Sıralama Dropdown -->
      <div class="w-full sm:w-64 relative" @click.stop>
        <div
          @click="toggleSortDropdown"
          class="w-full pl-10 pr-8 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm flex items-center justify-between cursor-pointer hover:border-slate-300 transition-all"
          :class="{ 'ring-2 ring-blue-500/20 border-blue-500 bg-white': isSortDropdownOpen }"
        >
          <ArrowUpDownIcon class="absolute left-3 text-slate-400" :size="18" />
          <span class="truncate select-none font-medium text-slate-600">{{
            selectedSortName
          }}</span>
          <ChevronDownIcon
            class="text-slate-400 transition-transform duration-200"
            :class="{ 'rotate-180': isSortDropdownOpen }"
            :size="16"
          />
        </div>
        <div
          v-if="isSortDropdownOpen"
          class="absolute top-full left-0 mt-1.5 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-30 p-1.5 flex flex-col gap-0.5 origin-top animate-fade-in"
        >
          <button
            v-for="option in sortOptions"
            :key="option.value"
            @click="selectSortFilter(option.value)"
            class="w-full text-left px-3 py-2.5 rounded-lg text-sm transition-colors flex items-center justify-between"
            :class="
              sortBy === option.value
                ? 'bg-blue-50 text-blue-700 font-bold'
                : 'text-slate-600 hover:bg-slate-50'
            "
          >
            {{ option.label }}
            <CheckIcon v-if="sortBy === option.value" :size="14" class="text-blue-600" />
          </button>
        </div>
      </div>
    </div>

    <!-- Tablo -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col">
      <div class="overflow-x-auto relative">
      <div
        v-if="isLoading"
        class="absolute inset-0 bg-slate-50/80 backdrop-blur-sm z-10 flex items-center justify-center"
      >
        <div
          class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
        ></div>
      </div>

      <div>
        <table v-if="rawMaterials.length > 0" class="w-full text-left border-collapse table-fixed">
          <thead>
            <tr
              class="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider"
            >
              <th class="px-6 py-4 w-24">Kodu</th>
              <th class="px-6 py-4 min-w-[180px]">Hammadde Adı</th>
              <th class="px-6 py-4 w-36">Kategori</th>
              <th class="px-6 py-4 w-32 text-center">Mevcut Stok</th>
              <th class="px-6 py-4 w-36 text-right">Birim Maliyet</th>
              <th class="px-6 py-4 w-40 text-right">Toplam Değer</th>
              <th class="px-6 py-4 w-40 text-center">Son İşlem</th>
              <th class="px-6 py-4 w-[300px] text-right">İşlemler</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="item in rawMaterials"
              :key="item.id"
              class="hover:bg-slate-50/60 transition-colors group"
            >
              <td class="px-6 py-4 text-sm font-mono text-slate-500 whitespace-nowrap">
                #{{ item.productNo }}
              </td>

              <td class="px-6 py-4 text-sm font-bold text-slate-800 truncate" :title="item.name">
                {{ item.name }}
              </td>

              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium bg-slate-100 text-slate-600 border border-slate-200 truncate max-w-full"
                >
                  {{ item.categoryName }}
                </span>
              </td>

              <!-- Stok Durumu — 3 seviyeli badge -->
              <td class="px-6 py-4 text-center whitespace-nowrap">
                <span
                  class="inline-flex items-center justify-center px-2.5 py-1 rounded-md text-xs font-bold border min-w-[70px]"
                  :class="
                    item.remainingQuantity === 0
                      ? 'bg-red-100 text-red-800 border-red-300'
                      : item.remainingQuantity <= 10
                        ? 'bg-amber-50 text-amber-700 border-amber-200'
                        : 'bg-blue-50 text-blue-700 border-blue-200'
                  "
                >
                  {{ item.remainingQuantity }}
                  <span v-if="item.remainingQuantity === 0" class="ml-1 text-[10px]">⚠</span>
                </span>
              </td>

              <!-- Birim Maliyet (YENİ) -->
              <td
                class="px-6 py-4 text-sm font-semibold text-slate-600 text-right whitespace-nowrap"
              >
                {{
                  item.remainingQuantity > 0
                    ? (item.totalCostValue / item.remainingQuantity).toLocaleString('tr-TR', {
                        style: 'currency',
                        currency: 'TRY',
                      })
                    : '—'
                }}
              </td>

              <!-- Toplam Değer -->
              <td
                class="px-6 py-4 text-sm font-semibold text-slate-700 text-right whitespace-nowrap"
              >
                {{
                  item.totalCostValue.toLocaleString('tr-TR', {
                    style: 'currency',
                    currency: 'TRY',
                  })
                }}
              </td>

              <td
                class="px-6 py-4 text-xs font-medium text-slate-400 text-center whitespace-nowrap"
              >
                {{ item.updatedAt }}
              </td>

              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center justify-end gap-2">
                  <button
                    @click="openInflow(item)"
                    title="Stok Ekle"
                    class="flex items-center gap-1 px-2.5 py-1.5 text-xs font-bold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200/60 rounded-xl transition-all shadow-sm"
                  >
                    <DownloadIcon :size="13" /> <span>Ekle</span>
                  </button>
                  <button
                    @click="openHistory(item)"
                    title="Hareket Geçmişi"
                    class="flex items-center gap-1 px-2.5 py-1.5 text-xs font-bold text-purple-700 bg-purple-50 hover:bg-purple-100 border border-purple-200/60 rounded-xl transition-all shadow-sm"
                  >
                    <HistoryIcon :size="13" /> <span>Detay</span>
                  </button>
                  <button
                    @click="openEdit(item)"
                    title="Hammaddeyi Düzenle"
                    class="flex items-center justify-center p-2 text-slate-600 bg-slate-50 hover:bg-slate-200 border border-slate-200 rounded-xl transition-all shadow-sm"
                  >
                    <Edit2Icon :size="13" />
                  </button>
                  <button
                    @click="triggerDelete(item)"
                    title="Hammaddeyi Sil"
                    class="flex items-center gap-1 px-2.5 py-1.5 text-xs font-bold text-red-700 bg-red-50 hover:bg-red-100 border border-red-200/60 rounded-xl transition-all shadow-sm"
                  >
                    <Trash2Icon :size="13" /> <span>Sil</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <div
          v-if="!isLoading && rawMaterials.length === 0"
          class="flex flex-col items-center justify-center py-20 px-4 bg-white"
        >
          <div
            class="w-16 h-16 bg-slate-50 border border-slate-200 rounded-full flex items-center justify-center mb-4"
          >
            <PackageIcon class="text-slate-300" :size="32" />
          </div>
          <h3 class="text-base font-bold text-slate-700 mb-1">Hammadde Bulunamadı</h3>
          <p class="text-sm text-slate-500 text-center max-w-sm">
            Filtrelerinize uygun hammadde bulunamadı veya depo henüz boş.
          </p>
        </div>
      </div>
      </div>

      <!-- Sayfalama -->
      <div
        class="border-t border-slate-100 px-6 py-4 flex flex-col sm:flex-row items-center justify-between bg-slate-50/50 gap-4 shrink-0"
      >
      <span class="text-sm text-slate-500 font-medium">
        Toplam <span class="font-bold text-slate-700">{{ totalCount }}</span> hammaddeden
        <span class="font-bold text-slate-700"
          >{{ rawMaterials.length > 0 ? (page - 1) * pageSize + 1 : 0 }} -
          {{ Math.min(page * pageSize, totalCount) }}</span
        >
        arası listeleniyor.
      </span>
      <div class="flex items-center gap-1">
        <button
          @click="changePage(page - 1)"
          :disabled="page === 1"
          class="px-3 py-1.5 text-sm font-semibold text-slate-600 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
        >
          Önceki
        </button>
        <button
          v-for="p in totalPages"
          :key="p"
          @click="changePage(p)"
          :class="[
            'px-3 py-1.5 text-sm font-bold rounded-lg border transition-all',
            p === page
              ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
              : 'bg-white text-slate-600 border-slate-200 hover:bg-slate-50',
          ]"
        >
          {{ p }}
        </button>
        <button
          @click="changePage(page + 1)"
          :disabled="page === totalPages || totalPages === 0"
          class="px-3 py-1.5 text-sm font-semibold text-slate-600 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
        >
          Sonraki
        </button>
      </div>
    </div>
    </div>

    <!-- Modaller -->
    <AddRawMaterialModal
      :isOpen="isAddModalOpen"
      @close="isAddModalOpen = false"
      @success="handleSuccess"
    />
    <AddRawMaterialCategoryModal
      :isOpen="isCategoryModalOpen"
      @close="isCategoryModalOpen = false"
      @success="handleCategorySuccess"
    />
    <StockInflowModal
      :isOpen="isInflowModalOpen"
      :product="selectedMaterial"
      @close="isInflowModalOpen = false"
      @success="handleSuccess"
    />
    <StockHistoryModal
      :isOpen="isHistoryModalOpen"
      :product="selectedMaterial"
      @close="isHistoryModalOpen = false"
    />
    <EditProductModal
      :isOpen="isEditModalOpen"
      :product="selectedMaterial"
      :categories="categories"
      @close="isEditModalOpen = false"
      @success="handleSuccess"
    />
    <DeleteRawMaterialModal
      :isOpen="isDeleteModalOpen"
      :rawMaterial="selectedMaterial"
      @close="isDeleteModalOpen = false"
      @success="handleSuccess"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import {
  PackageIcon,
  PlusIcon,
  SearchIcon,
  FilterIcon,
  ArrowUpDownIcon,
  Edit2Icon,
  DownloadIcon,
  HistoryIcon,
  FolderPlusIcon,
  Trash2Icon,
  ChevronDownIcon,
  CheckIcon,
} from 'lucide-vue-next'

import CustomDropdown from '@/components/CustomDropdown.vue'
import AddRawMaterialModal from './AddRawMaterialModal.vue'
import AddRawMaterialCategoryModal from './AddRawMaterialCategoryModal.vue'
import DeleteRawMaterialModal from './DeleteRawMaterialModal.vue'
import StockInflowModal from '../stocks/StockInflowModal.vue'
import StockHistoryModal from '../stocks/StockHistoryModal.vue'
import EditProductModal from '../stocks/EditProductModal.vue'

const BASE_URL = '/api'

const rawMaterials = ref<any[]>([])
const categories = ref<any[]>([])
const isLoading = ref(false)

const searchText = ref('')
const selectedCategory = ref('')
const stockFilter = ref('')

const stockFilterOptions = [
  { label: 'Tüm Stok Durumları', value: '' },
  { label: 'Kritik Stok (≤10)', value: 'low' },
  { label: 'Yeterli Stok (>10)', value: 'ok' },
  { label: 'Stok Yok (0)', value: 'empty' },
]
const sortBy = ref('date_desc')
const page = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const totalPages = ref(0)

const isAddModalOpen = ref(false)
const isCategoryModalOpen = ref(false)
const isInflowModalOpen = ref(false)
const isHistoryModalOpen = ref(false)
const isEditModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const selectedMaterial = ref<any>(null)

const isCategoryDropdownOpen = ref(false)
const isSortDropdownOpen = ref(false)
const categorySearchQuery = ref('')

const sortOptions = [
  { value: 'date_desc', label: 'En Yeni Eklenenler' },
  { value: 'quantity_asc', label: 'Stok Miktarı (Azdan Çoğa)' },
  { value: 'quantity_desc', label: 'Stok Miktarı (Çoktan Aza)' },
]

const selectedCategoryName = computed(() => {
  if (!selectedCategory.value) return 'Tüm Kategoriler'
  const found = categories.value.find((c) => c.id === selectedCategory.value)
  return found ? found.name : 'Tüm Kategoriler'
})

const selectedSortName = computed(() => {
  const found = sortOptions.find((o) => o.value === sortBy.value)
  return found ? found.label : 'En Yeni Eklenenler'
})

const filteredCategoryList = computed(() => {
  const q = categorySearchQuery.value.trim().toLowerCase()
  if (!q) return categories.value
  return categories.value.filter((c: any) => c.name.toLowerCase().includes(q))
})

const toggleCategoryDropdown = () => {
  isSortDropdownOpen.value = false
  isCategoryDropdownOpen.value = !isCategoryDropdownOpen.value
  if (isCategoryDropdownOpen.value) categorySearchQuery.value = ''
}

const toggleSortDropdown = () => {
  isCategoryDropdownOpen.value = false
  isSortDropdownOpen.value = !isSortDropdownOpen.value
}

const selectCategoryFilter = (id: string) => {
  selectedCategory.value = id
  isCategoryDropdownOpen.value = false
  categorySearchQuery.value = ''
  handleFilters()
}

const selectSortFilter = (val: string) => {
  sortBy.value = val
  isSortDropdownOpen.value = false
  handleFilters()
}

const closeAllDropdowns = () => {
  isCategoryDropdownOpen.value = false
  isSortDropdownOpen.value = false
}

const fetchCategories = async () => {
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Categories`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) {
      const data = await res.json()
      categories.value = data.filter((c: any) => c.type === 1)
    }
  } catch (e) {
    console.error('Kategoriler çekilemedi:', e)
  }
}

const fetchRawMaterials = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')

    let sortField = 'date'
    let isDescending = true
    if (sortBy.value === 'quantity_asc') {
      sortField = 'quantity'
      isDescending = false
    } else if (sortBy.value === 'quantity_desc') {
      sortField = 'quantity'
      isDescending = true
    }

    // Stok filtresi için ayrı parametre gönderemiyoruz (backend desteklemiyor),
    // bu yüzden büyük sayfalı veri çekip frontend'de filtreliyoruz
    const fetchSize = stockFilter.value ? 1000 : pageSize.value

    let url = `${BASE_URL}/Products/raw-materials?SearchText=${searchText.value}&Page=1&PageSize=${fetchSize}&SortBy=${sortField}&IsDescending=${isDescending}`
    if (selectedCategory.value) url += `&CategoryId=${selectedCategory.value}`

    const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } })
    if (res.ok) {
      const data = await res.json()
      let items = data.items || []

      // Stok durumu frontend filtresi
      if (stockFilter.value === 'empty') items = items.filter((i: any) => i.remainingQuantity === 0)
      else if (stockFilter.value === 'low')
        items = items.filter((i: any) => i.remainingQuantity > 0 && i.remainingQuantity <= 10)
      else if (stockFilter.value === 'ok')
        items = items.filter((i: any) => i.remainingQuantity > 10)

      if (stockFilter.value) {
        // Sayfalamayı frontend'de yapıyoruz
        totalCount.value = items.length
        totalPages.value = Math.ceil(items.length / pageSize.value)
        rawMaterials.value = items.slice(
          (page.value - 1) * pageSize.value,
          page.value * pageSize.value,
        )
      } else {
        rawMaterials.value = items
        totalCount.value = data.totalCount || 0
        totalPages.value = data.totalPages || 0
      }
    }
  } catch (e) {
    console.error('Hata:', e)
  } finally {
    isLoading.value = false
  }
}

const triggerDelete = (item: any) => {
  selectedMaterial.value = item
  isDeleteModalOpen.value = true
}

let searchTimeout: any
const handleFilters = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    page.value = 1
    fetchRawMaterials()
  }, 400)
}

const changePage = (newPage: number) => {
  if (newPage >= 1 && newPage <= totalPages.value) {
    page.value = newPage
    fetchRawMaterials()
  }
}

const openInflow = (item: any) => {
  selectedMaterial.value = item
  isInflowModalOpen.value = true
}
const openHistory = (item: any) => {
  selectedMaterial.value = item
  isHistoryModalOpen.value = true
}
const openEdit = (item: any) => {
  selectedMaterial.value = item
  isEditModalOpen.value = true
}

const handleSuccess = () => fetchRawMaterials()
const handleCategorySuccess = () => fetchCategories()

onMounted(() => {
  document.addEventListener('click', closeAllDropdowns)
  fetchCategories()
  fetchRawMaterials()
})

onUnmounted(() => {
  document.removeEventListener('click', closeAllDropdowns)
})
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
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
.animate-fade-in {
  animation: fadeIn 0.12s ease-out forwards;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-4px);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
</style>
