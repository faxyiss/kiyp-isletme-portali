<template>
  <div class="space-y-6 animate-fade-in">
    <div
      class="bg-white p-4 rounded-2xl border border-slate-100 shadow-sm flex flex-col sm:flex-row justify-between items-center gap-4"
    >
      <div>
        <h2 class="text-xl font-bold text-slate-800">Satış İşlemleri</h2>
        <p class="text-sm text-slate-500 mt-1">
          Gelişmiş filtrelerle ürün seçip satışınızı tamamlayın veya geçmişi inceleyin.
        </p>
      </div>
      <div class="flex bg-slate-50 p-1 rounded-xl border border-slate-100">
        <button
          @click="activeTab = 'newSale'"
          :class="activeTab === 'newSale' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500'"
          class="px-5 py-2 rounded-lg text-sm font-semibold cursor-pointer transition-all"
        >
          Satış Yap
        </button>
        <button
          @click="activeTab = 'history'"
          :class="activeTab === 'history' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500'"
          class="px-5 py-2 rounded-lg text-sm font-semibold cursor-pointer transition-all"
        >
          Geçmiş
        </button>
      </div>
    </div>

    <div v-if="isLoading" class="flex justify-center py-20">
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
    </div>

    <SaleTerminal
      v-else-if="activeTab === 'newSale'"
      :products-list="productsList"
      :customers-list="customersList"
      :current-page="currentPage"
      :total-pages="totalPages"
      @sale-completed="loadAllData"
      @page-changed="handlePageChange"
      @filter-changed="handleFilterChange"
    />

    <SaleHistory
      v-else-if="activeTab === 'history'"
      :sales-list="salesList"
      :customers-list="customersList"
      :current-page="historyCurrentPage"
      :total-pages="historyTotalPages"
      :total-revenue="historyTotalRevenue"
      @page-changed="handleHistoryPageChange"
      @filter-changed="handleHistoryFilterChange"
    />
  </div>
</template>

<script setup lang="ts">
defineOptions({ name: 'SalesView' })
import { ref, onMounted, watch } from 'vue'
import axios from 'axios'
import SaleTerminal from './Components/SaleTerminal.vue'
import SaleHistory from './Components/SaleHistory.vue'

interface Product {
  id: string
  productNo: number
  name: string
  unitPrice: number
  remainingQuantity: number
  categoryName: string
}
interface Customer {
  id: string
  firstName: string
  lastName: string
}
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

const activeTab = ref<'newSale' | 'history'>('newSale')
const isLoading = ref(true)

const productsList = ref<Product[]>([])
const customersList = ref<Customer[]>([])
const salesList = ref<Sale[]>([])

// Satış Yap Sekmesi Sayfalama & Filtre Durumu
const currentPage = ref(1)
const totalPages = ref(1)
const activeFilters = ref({
  searchText: '',
  categoryId: null as string | null,
  inStock: true as boolean | null,
  minPrice: null as number | null,
  maxPrice: null as number | null,
  sortBy: 'productno',
})

// GEÇMİŞ SEKMESİ İÇİN YENİ DURUM DEĞİŞKENLERİ
const historyCurrentPage = ref(1)
const historyTotalPages = ref(1)
const historyTotalRevenue = ref(0)
const activeHistoryFilters = ref({
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
  await loadAllData()
})

// Sekme geçişlerini izle (Geçmişe geçince otomatik backend sorgusu atar)
watch(activeTab, async (newTab) => {
  if (newTab === 'history') {
    await loadSalesHistory()
  }
})

// --- Satış Yap Sekmesi Tetikleyicileri ---
const handlePageChange = async (newPage: number) => {
  currentPage.value = newPage
  await loadProductsOnly()
}
const handleFilterChange = async (newFilters: typeof activeFilters.value) => {
  activeFilters.value = { ...newFilters }
  currentPage.value = 1
  await loadProductsOnly()
}

// --- GEÇMİŞ SEKRESİ TETİKLEYİCİLERİ ---
const handleHistoryPageChange = async (newPage: number) => {
  historyCurrentPage.value = newPage
  await loadSalesHistory()
}
const handleHistoryFilterChange = async (newFilters: typeof activeHistoryFilters.value) => {
  activeHistoryFilters.value = { ...newFilters }
  historyCurrentPage.value = 1
  await loadSalesHistory()
}

// Sadece ürünleri yenileyen fonksiyon
const loadProductsOnly = async () => {
  const token = localStorage.getItem('token')
  try {
    const params = new URLSearchParams()
    params.append('page', currentPage.value.toString())
    params.append('pageSize', '12')

    if (activeFilters.value.searchText) params.append('searchText', activeFilters.value.searchText)
    if (activeFilters.value.categoryId) params.append('categoryId', activeFilters.value.categoryId)
    if (activeFilters.value.inStock !== null)
      params.append('inStock', activeFilters.value.inStock.toString())
    if (activeFilters.value.minPrice !== null)
      params.append('minPrice', activeFilters.value.minPrice.toString())
    if (activeFilters.value.maxPrice !== null)
      params.append('maxPrice', activeFilters.value.maxPrice.toString())
    params.append('sortBy', activeFilters.value.sortBy)

    const prodRes = await axios.get(`/Products/sales-terminal?${params.toString()}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    productsList.value = prodRes.data.items || prodRes.data
    totalPages.value = prodRes.data.totalPages || 1
  } catch (error) {
    console.error('Ürün arama hatası:', error)
  }
}

// YENİ GERÇEKLEŞTİRİLEN BACKEND GEÇMİŞ SORGUSU
const loadSalesHistory = async () => {
  const token = localStorage.getItem('token')
  const config = { headers: { Authorization: `Bearer ${token}` } }
  try {
    const params = new URLSearchParams()
    params.append('page', historyCurrentPage.value.toString())
    params.append('pageSize', '10') // Geçmiş tablosu için sayfa başına 10 kayıt idealdir

    if (activeHistoryFilters.value.search)
      params.append('search', activeHistoryFilters.value.search)
    if (activeHistoryFilters.value.startDate)
      params.append('startDate', activeHistoryFilters.value.startDate)
    if (activeHistoryFilters.value.endDate)
      params.append('endDate', activeHistoryFilters.value.endDate)
    if (activeHistoryFilters.value.paymentType !== null)
      params.append('paymentType', activeHistoryFilters.value.paymentType.toString())
    if (activeHistoryFilters.value.categoryId)
      params.append('categoryId', activeHistoryFilters.value.categoryId)
    if (activeHistoryFilters.value.customerId)
      params.append('customerId', activeHistoryFilters.value.customerId)
    if (activeHistoryFilters.value.minAmount !== null)
      params.append('minAmount', activeHistoryFilters.value.minAmount.toString())
    if (activeHistoryFilters.value.maxAmount !== null)
      params.append('maxAmount', activeHistoryFilters.value.maxAmount.toString())
    params.append('sortBy', activeHistoryFilters.value.sortBy)

    const res = await axios.get(`/Sales/history?${params.toString()}`, config)

    // Enum eşleşmesini güvenli hale getirme
    const salesHistoryItems = (res.data.items || []) as Array<Record<string, unknown>>
    salesList.value = salesHistoryItems.map((s) => {
      const pt = s.paymentType
      let ptNum = 0
      if (pt === 'KrediKarti' || pt === '1' || pt === 1) ptNum = 1
      if (pt === 'Veresiye' || pt === '2' || pt === 2) ptNum = 2
      return { ...s, paymentType: ptNum } as unknown as Sale
    })

    historyTotalPages.value = res.data.totalPages || 1
    historyTotalRevenue.value = res.data.totalRevenue || 0
  } catch (error) {
    console.error('Satış geçmişi yükleme hatası:', error)
  }
}

// İlk açılışta verileri topluca yükleyen fonksiyon
const loadAllData = async () => {
  isLoading.value = true
  const token = localStorage.getItem('token')
  const config = { headers: { Authorization: `Bearer ${token}` } }

  try {
    const params = new URLSearchParams({
      page: currentPage.value.toString(),
      pageSize: '12',
      inStock: 'true',
      sortBy: 'productno',
    })

    const [prodRes, custRes] = await Promise.all([
      axios.get(`/Products/sales-terminal?${params.toString()}`, config),
      axios.get('/Customers?pageSize=1000', config), // Tüm müşterileri dropdownlar için geniş çektik
    ])

    productsList.value = prodRes.data.items || prodRes.data
    totalPages.value = prodRes.data.totalPages || 1
    customersList.value = custRes.data.items || custRes.data

    if (activeTab.value === 'history') {
      await loadSalesHistory()
    }
  } catch (error) {
    console.error('Veri yükleme hatası:', error)
  } finally {
    isLoading.value = false
  }
}
</script>
