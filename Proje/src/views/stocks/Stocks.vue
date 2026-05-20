<template>
  <div class="space-y-6">
    <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm space-y-4">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="relative w-full md:w-80">
          <SearchIcon class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
          <input
            v-model="queryParams.searchText"
            @input="handleFilterChange"
            type="text"
            placeholder="Ürün adı veya ürün no ara..."
            class="w-full pl-10 pr-4 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm"
          />
        </div>

        <div class="flex flex-wrap items-center gap-3 w-full md:w-auto z-30">
          <div class="relative w-full sm:w-52">
            <button
              @click="toggleDropdown('category')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedCategoryName }}</span>
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

          <div class="relative w-full sm:w-48">
            <button
              @click="toggleDropdown('sort')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedSortLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isSortDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isSortDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isSortDropdownOpen = false"
            ></div>
            <div
              v-if="isSortDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                v-for="option in sortOptions"
                :key="option.value"
                @click="selectSort(option.value)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  queryParams.sortBy === option.value
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                {{ option.label }}
              </button>
            </div>
          </div>

          <div class="relative w-full sm:w-44">
            <button
              @click="toggleDropdown('direction')"
              type="button"
              class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <span class="truncate font-medium">{{ selectedDirectionLabel }}</span>
              <ChevronDownIcon
                class="text-slate-400 transition-transform duration-200"
                :class="{ 'rotate-180': isDirectionDropdownOpen }"
                :size="16"
              />
            </button>
            <div
              v-if="isDirectionDropdownOpen"
              class="fixed inset-0 z-40"
              @click="isDirectionDropdownOpen = false"
            ></div>
            <div
              v-if="isDirectionDropdownOpen"
              class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 space-y-0.5"
            >
              <button
                @click="selectDirection(true)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  queryParams.isDescending === true
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                Büyükten Küçüğe ↓
              </button>
              <button
                @click="selectDirection(false)"
                type="button"
                class="w-full text-left px-2.5 py-1.5 rounded-lg text-xs font-medium transition-colors"
                :class="
                  queryParams.isDescending === false
                    ? 'bg-blue-50 text-blue-600 font-bold'
                    : 'text-slate-600 hover:bg-slate-50'
                "
              >
                Küçükten Büyüğe ↑
              </button>
            </div>
          </div>
        </div>

        <button
          @click="onNewProductClick"
          class="w-full md:w-auto flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-xl font-medium transition-colors text-sm shadow-sm shadow-blue-200"
        >
          <PlusIcon :size="18" />
          Yeni Ürün Tanımla
        </button>
      </div>
    </div>

    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
      <div class="overflow-x-auto custom-scrollbar">
        <table class="w-full text-left border-collapse table-auto">
          <thead>
            <tr
              class="bg-slate-50/60 border-b border-slate-100 text-slate-500 text-[11px] font-bold uppercase tracking-wider"
            >
              <th class="py-4 px-4 text-center w-14">No</th>
              <th class="py-4 px-4 min-w-[280px]">Ürün Bilgisi</th>
              <th class="py-4 px-4 min-w-[120px]">Kategori</th>
              <th class="py-4 px-4 min-w-[110px]">Satış Fiyatı</th>
              <th class="py-4 px-4 min-w-[110px]">Mevcut Stok</th>
              <th class="py-4 px-4 min-w-[120px] text-amber-700">Toplam Maliyeti</th>
              <th class="py-4 px-4 min-w-[120px] text-blue-700">Potansiyel Ciro</th>
              <th class="py-4 px-4 min-w-[100px] text-emerald-700">Kâr Marjı</th>
              <th class="py-4 px-4 text-right min-w-[310px]">İşlemler</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-slate-700 text-sm">
            <tr v-if="isLoading">
              <td colspan="9" class="py-12 text-center text-slate-400 font-medium">
                Veriler sunucudan çekiliyor...
              </td>
            </tr>
            <tr v-else-if="products.length === 0">
              <td colspan="9" class="py-12 text-center text-slate-400 font-medium">
                Aradığınız kriterlere uygun ürün bulunamadı.
              </td>
            </tr>

            <tr
              v-else
              v-for="product in products"
              :key="product.id"
              class="hover:bg-slate-50/40 transition-colors"
            >
              <td class="py-4 px-4 text-center font-bold text-slate-400 align-middle">
                #{{ product.productNo }}
              </td>

              <td class="py-4 px-4 align-middle">
                <div class="flex items-center gap-3">
                  <div
                    class="flex-shrink-0 w-9 h-9 rounded-xl bg-slate-100 border border-slate-200/50 flex items-center justify-center text-slate-500"
                  >
                    <PackageIcon :size="18" />
                  </div>
                  <div class="flex flex-col min-w-0 max-w-[240px]">
                    <span class="font-bold text-slate-800 text-sm truncate" :title="product.name">
                      {{ product.name }}
                    </span>
                    <span
                      v-if="product.description"
                      class="text-xs text-slate-400 truncate mt-0.5"
                      :title="product.description"
                    >
                      {{ product.description }}
                    </span>

                    <span
                      class="text-[11px] text-slate-400/90 mt-1 flex items-center gap-1 font-medium whitespace-nowrap"
                    >
                      <ClockIcon :size="12" class="text-slate-400" />
                      S.G: {{ product.updatedAt }}
                    </span>
                  </div>
                </div>
              </td>

              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <span
                  class="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-semibold bg-blue-50/70 text-blue-600 border border-blue-100/50"
                >
                  {{ product.categoryName }}
                </span>
              </td>

              <td class="py-4 px-4 align-middle font-bold text-slate-800 whitespace-nowrap">
                ₺{{ product.unitPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) }}
              </td>

              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <div class="flex items-center gap-1.5">
                  <span
                    :class="[
                      'inline-flex items-center px-2 py-0.5 rounded-md text-xs font-bold border',
                      product.remainingQuantity === 0
                        ? 'bg-red-50 text-red-600 border-red-100'
                        : 'bg-emerald-50 text-emerald-600 border-emerald-100',
                    ]"
                  >
                    {{ product.remainingQuantity }} Adet
                  </span>
                  <span
                    v-if="product.remainingQuantity === 0"
                    class="text-[10px] text-red-500 font-bold uppercase tracking-wide"
                    >Bitti</span
                  >
                </div>
              </td>

              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <div
                  class="px-2 py-0.5 rounded-md bg-amber-50 border border-amber-100 text-amber-700 font-bold text-xs inline-block"
                >
                  ₺{{
                    product.totalCostValue.toLocaleString('tr-TR', { minimumFractionDigits: 2 })
                  }}
                </div>
              </td>

              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <div
                  class="px-2 py-0.5 rounded-md bg-blue-50 border border-blue-100 text-blue-700 font-bold text-xs inline-block"
                >
                  ₺{{
                    (product.remainingQuantity * product.unitPrice).toLocaleString('tr-TR', {
                      minimumFractionDigits: 2,
                    })
                  }}
                </div>
              </td>

              <td class="py-4 px-4 align-middle whitespace-nowrap">
                <div
                  v-if="product.totalCostValue > 0"
                  class="flex items-center gap-0.5 font-bold text-xs"
                  :class="calculateProfitMargin(product) >= 0 ? 'text-emerald-600' : 'text-red-500'"
                >
                  <TrendingUpIcon v-if="calculateProfitMargin(product) >= 0" :size="12" />
                  <TrendingDownIcon v-else :size="12" />
                  %{{
                    Math.abs(calculateProfitMargin(product)).toLocaleString('tr-TR', {
                      maximumFractionDigits: 1,
                    })
                  }}
                </div>
                <span v-else class="text-slate-300 font-bold text-xs">-</span>
              </td>

              <td class="py-4 px-4 align-middle text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button
                    @click="onStockInflowClick(product)"
                    class="inline-flex items-center gap-1 bg-emerald-600 hover:bg-emerald-700 text-white px-2.5 py-1.5 rounded-lg text-xs font-bold transition-colors shadow-sm"
                  >
                    <TrendingUpIcon :size="13" /> Stok Gir
                  </button>

                  <div class="h-4 w-[1px] bg-slate-200 mx-0.5"></div>

                  <button
                    @click="onEditClick(product)"
                    class="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                    title="Düzenle"
                  >
                    <EditIcon :size="15" />
                  </button>
                  <button
                    @click="handleDeleteProduct(product)"
                    class="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                    title="Sil"
                  >
                    <Trash2Icon :size="15" />
                  </button>

                  <div class="h-4 w-[1px] bg-slate-200 mx-0.5"></div>

                  <button
                    @click="openHistory(product)"
                    class="inline-flex items-center gap-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 border border-slate-200 px-2.5 py-1.5 rounded-lg text-xs font-bold transition-colors shadow-sm"
                  >
                    <ListIcon :size="13" /> Stok Detayı
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div
        class="border-t border-slate-100 px-6 py-4 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-400 bg-slate-50/30 font-medium"
      >
        <span
          >Filtrelere uygun toplam <b class="text-slate-600">{{ totalCount }}</b> ürün içerisinden
          <b class="text-slate-600">{{ products.length }}</b> tanesi listeleniyor.</span
        >

        <div v-if="totalPages > 1" class="flex items-center gap-1">
          <button
            @click="changePage(queryParams.page - 1)"
            :disabled="queryParams.page === 1"
            class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 disabled:opacity-40 disabled:hover:bg-white font-semibold transition-colors"
          >
            Önceki
          </button>
          <button
            v-for="pNo in totalPages"
            :key="pNo"
            @click="changePage(pNo)"
            :class="[
              'px-2.5 py-1 rounded-lg font-bold transition-all',
              queryParams.page === pNo
                ? 'bg-blue-600 text-white border border-blue-600 shadow-sm'
                : 'border border-slate-200 bg-white hover:bg-slate-50 text-slate-600',
            ]"
          >
            {{ pNo }}
          </button>
          <button
            @click="changePage(queryParams.page + 1)"
            :disabled="queryParams.page === totalPages"
            class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 disabled:opacity-40 disabled:hover:bg-white font-semibold transition-colors"
          >
            Sonraki
          </button>
        </div>
      </div>
    </div>

    <AddProductModal
      :is-open="isNewProductModalOpen"
      :categories="categories"
      @close="isNewProductModalOpen = false"
      @add-category="(newCat) => categories.push(newCat)"
      @success="fetchProducts"
    />
    <EditProductModal
      :is-open="isEditProductModalOpen"
      :categories="categories"
      :product="selectedProduct"
      @close="isEditProductModalOpen = false"
      @success="fetchProducts"
    />
    <StockInflowModal
      :is-open="isStockInflowModalOpen"
      :product="selectedProduct"
      @close="isStockInflowModalOpen = false"
      @success="fetchProducts"
    />
    <StockHistoryModal
      :is-open="isStockHistoryModalOpen"
      :product="selectedProduct"
      @close="isStockHistoryModalOpen = false"
      @success="fetchProducts"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  SearchIcon,
  PlusIcon,
  PackageIcon,
  EditIcon,
  Trash2Icon,
  TrendingUpIcon,
  TrendingDownIcon,
  ChevronDownIcon,
  ClockIcon,
  ListIcon,
} from 'lucide-vue-next'

import AddProductModal from './AddProductModal.vue'
import EditProductModal from './EditProductModal.vue'
import StockInflowModal from './StockInflowModal.vue'
import StockHistoryModal from './StockHistoryModal.vue'
import { useAlert } from '@/composables/useAlert'

const { showSuccess, showError, showConfirm } = useAlert()

const BASE_URL = '/api'

const products = ref<any[]>([])
const categories = ref<any[]>([])
const isLoading = ref(false)

const totalCount = ref(0)
const totalPages = ref(0)

const isNewProductModalOpen = ref(false)
const isEditProductModalOpen = ref(false)
const isStockInflowModalOpen = ref(false)
const isStockHistoryModalOpen = ref(false)
const selectedProduct = ref<any>(null)

const isCategoryDropdownOpen = ref(false)
const isSortDropdownOpen = ref(false)
const isDirectionDropdownOpen = ref(false)
const categorySearchText = ref('')

const sortOptions = [
  { value: 'productno', label: "Ürün No'ya Göre" },
  { value: 'quantity', label: 'Stok Adedine Göre' },
  { value: 'price', label: 'Satış Fiyatına Göre' },
  { value: 'date', label: 'Son Güncellemeye Göre' },
]

const queryParams = ref({
  searchText: '',
  categoryId: null as string | null,
  sortBy: 'productno',
  isDescending: true,
  page: 1,
  pageSize: 10,
})

// Otomatik Kâr Marjı Hesaplama Mantığı
const calculateProfitMargin = (product: any) => {
  if (!product.totalCostValue || product.totalCostValue === 0) return 0
  const totalRevenue = product.remainingQuantity * product.unitPrice
  const profit = totalRevenue - product.totalCostValue
  return (profit / product.totalCostValue) * 100
}

const toggleDropdown = (type: 'category' | 'sort' | 'direction') => {
  isCategoryDropdownOpen.value = type === 'category' ? !isCategoryDropdownOpen.value : false
  isSortDropdownOpen.value = type === 'sort' ? !isSortDropdownOpen.value : false
  isDirectionDropdownOpen.value = type === 'direction' ? !isDirectionDropdownOpen.value : false
}

const selectedCategoryName = computed(() => {
  if (!queryParams.value.categoryId) return 'Tüm Kategoriler'
  const found = categories.value.find((c) => c.id === queryParams.value.categoryId)
  return found ? found.name : 'Tüm Kategoriler'
})

const selectedSortLabel = computed(() => {
  const found = sortOptions.find((o) => o.value === queryParams.value.sortBy)
  return found ? found.label : "Ürün No'ya Göre"
})

const selectedDirectionLabel = computed(() => {
  return queryParams.value.isDescending ? 'Büyükten Küçüğe ↓' : 'Küçükten Büyüğe ↑'
})

const filteredCategories = computed(() => {
  if (!categorySearchText.value.trim()) return categories.value
  return categories.value.filter((c) =>
    c.name.toLowerCase().includes(categorySearchText.value.toLowerCase()),
  )
})

const selectCategory = (catId: string | null) => {
  queryParams.value.categoryId = catId
  isCategoryDropdownOpen.value = false
  categorySearchText.value = ''
  handleFilterChange()
}

const selectSort = (sortValue: string) => {
  queryParams.value.sortBy = sortValue
  isSortDropdownOpen.value = false
  fetchProducts()
}

const selectDirection = (dirValue: boolean) => {
  queryParams.value.isDescending = dirValue
  isDirectionDropdownOpen.value = false
  fetchProducts()
}

const fetchProducts = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const params = new URLSearchParams()
    if (queryParams.value.searchText) params.append('SearchText', queryParams.value.searchText)
    if (queryParams.value.categoryId) params.append('CategoryId', queryParams.value.categoryId)
    params.append('SortBy', queryParams.value.sortBy)
    params.append('IsDescending', queryParams.value.isDescending.toString())
    params.append('Page', queryParams.value.page.toString())
    params.append('PageSize', queryParams.value.pageSize.toString())

    const response = await fetch(`${BASE_URL}/Products/search?${params.toString()}`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    })

    if (response.ok) {
      const data = await response.json()
      products.value = data.items
      totalCount.value = data.totalCount
      totalPages.value = data.totalPages
    }
  } catch (error) {
    console.error('Ürünler getirilirken hata oluştu:', error)
  } finally {
    isLoading.value = false
  }
}

const fetchCategories = async () => {
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`${BASE_URL}/Categories`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (response.ok) {
      const data = await response.json()
      // DİKKAT: Sadece normal ürün kategorilerini (type: 0) listeliyoruz
      categories.value = data.filter((c: any) => c.type === 0)
    }
  } catch (error) {
    console.error('Kategoriler çekilemedi:', error)
  }
}

const handleFilterChange = () => {
  queryParams.value.page = 1
  fetchProducts()
}

const changePage = (pageNo: number) => {
  if (pageNo < 1 || pageNo > totalPages.value) return
  queryParams.value.page = pageNo
  fetchProducts()
}

const handleDeleteProduct = async (product: any) => {
  const confirmDelete = await showConfirm(
    `"${product.name}" isimli ürünü silmek istediğinize emin misiniz? Bu işlem ilişkili tüm stokları da silecektir.`,
  )
  if (!confirmDelete) return

  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`${BASE_URL}/Products/delete/${product.id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })

    const data = await response.json()

    if (response.ok) {
      showSuccess(data.message || 'Ürün başarıyla silindi.')
      fetchProducts()
    } else {
      showError(data.message || 'Silme işlemi başarısız.')
    }
  } catch (error) {
    console.error('Silme işlemi sırasında hata meydana geldi:', error)
    showError('Sunucuyla bağlantı kurulamadı.')
  }
}

onMounted(() => {
  fetchCategories()
  fetchProducts()
})

const onNewProductClick = () => {
  isNewProductModalOpen.value = true
}
const onStockInflowClick = (product: any) => {
  selectedProduct.value = product
  isStockInflowModalOpen.value = true
}
const onEditClick = (product: any) => {
  selectedProduct.value = product
  isEditProductModalOpen.value = true
}
const openHistory = (product: any) => {
  selectedProduct.value = product
  isStockHistoryModalOpen.value = true
}
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
