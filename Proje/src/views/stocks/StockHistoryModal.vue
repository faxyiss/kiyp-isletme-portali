<template>
  <Teleport to="body">
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-5xl overflow-hidden transform transition-all z-10 flex flex-col max-h-[90vh]"
    >
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50 flex-shrink-0"
      >
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <HistoryIcon :size="18" class="text-blue-600" />
          Stok Hareketleri ve Parti Geçmişi
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <div
        class="p-6 bg-slate-50 border-b border-slate-100 flex-shrink-0 flex flex-col sm:flex-row sm:items-center justify-between gap-4"
      >
        <div class="flex items-center gap-3" v-if="product">
          <div
            class="w-12 h-12 rounded-xl bg-white shadow-sm border border-slate-100 flex items-center justify-center text-slate-500"
          >
            <PackageIcon :size="24" />
          </div>
          <div>
            <div class="text-xs font-bold text-slate-400 uppercase tracking-wide">
              SKU-{{ product.productNo }}
            </div>
            <div class="font-extrabold text-slate-800 text-base">{{ product.name }}</div>
          </div>
        </div>

        <div
          class="bg-white px-4 py-2 rounded-xl border border-slate-200 shadow-sm flex flex-col justify-center"
        >
          <span class="text-[10px] font-bold text-slate-400 uppercase">Toplam Mevcut Stok</span>
          <span class="text-lg font-black text-blue-600"
            >{{ product?.remainingQuantity ?? 0 }} Adet</span
          >
        </div>
      </div>

      <div class="flex-1 overflow-y-auto p-6 min-h-[300px] custom-scrollbar">
        <div v-if="isLoading" class="text-center py-12 text-sm text-slate-400 font-medium">
          Veriler çekiliyor...
        </div>
        <div
          v-else-if="stocks.length === 0"
          class="text-center py-12 text-sm text-slate-400 font-medium"
        >
          Bu ürüne ait stok girişi bulunamadı.
        </div>

        <div v-else class="overflow-x-auto border border-slate-100 rounded-xl shadow-sm">
          <table class="w-full text-left border-collapse text-sm">
            <thead>
              <tr
                class="bg-slate-50/70 border-b border-slate-100 text-xs font-bold text-slate-500 uppercase tracking-wider"
              >
                <th class="py-3 px-4">Giriş Tarihi</th>
                <th class="py-3 px-4">İlk Giriş</th>
                <th class="py-3 px-4">Kalan (Mevcut)</th>
                <th class="py-3 px-4">Birim Maliyet</th>
                <th class="py-3 px-4 text-emerald-600 font-extrabold">Toplam Değeri</th>
                <th class="py-3 px-4 text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr
                v-for="stock in stocks"
                :key="stock.id"
                class="hover:bg-slate-50/40 transition-colors"
              >
                <td class="py-3.5 px-4 font-medium text-slate-600">
                  {{ formatDate(stock.inflowDate) }}
                </td>
                <td class="py-3.5 px-4 font-bold text-slate-700">
                  {{ stock.inflowQuantity }} Adet
                </td>
                <td class="py-3.5 px-4">
                  <span
                    :class="[
                      'inline-flex items-center px-2 py-0.5 rounded-md text-xs font-bold border',
                      stock.remainingQuantity === 0
                        ? 'bg-red-50 text-red-600 border-red-100'
                        : 'bg-emerald-50 text-emerald-600 border-emerald-100',
                    ]"
                  >
                    {{ stock.remainingQuantity }} Adet
                  </span>
                </td>
                <td class="py-3.5 px-4 font-semibold text-slate-600">
                  ₺{{ stock.unitCost.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) }}
                </td>
                <td class="py-3.5 px-4 font-extrabold text-slate-900 bg-emerald-50/30">
                  ₺{{
                    (stock.remainingQuantity * stock.unitCost).toLocaleString('tr-TR', {
                      minimumFractionDigits: 2,
                    })
                  }}
                </td>

                <td class="py-3.5 px-4 text-right">
                  <div class="flex justify-end gap-2">
                    <button
                      @click="handleDeleteStock(stock)"
                      class="flex items-center gap-1 text-xs font-bold bg-red-50 hover:bg-red-100 border border-red-200 text-red-600 px-2.5 py-1.5 rounded-lg transition-all"
                      title="Bu stok kaydını tamamen sil"
                    >
                      <Trash2Icon :size="14" /> Sil
                    </button>

                    <button
                      @click="openDeductModal(stock)"
                      :disabled="stock.remainingQuantity <= 0"
                      class="flex items-center gap-1 text-xs font-bold bg-amber-50 hover:bg-amber-100 border border-amber-200 text-amber-700 px-2.5 py-1.5 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed transition-all shadow-sm"
                    >
                      <MinusCircleIcon :size="14" /> Eksilt
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div
        v-if="totalPages > 1"
        class="border-t border-slate-100 px-6 py-4 flex items-center justify-between bg-slate-50/30 flex-shrink-0"
      >
        <span class="text-xs text-slate-500 font-medium"
          >Toplam <b>{{ totalCount }}</b> parti kaydı bulundu.</span
        >
        <div class="flex items-center gap-1">
          <button
            @click="changePage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="p-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40 transition-colors"
          >
            <ChevronLeftIcon :size="16" class="text-slate-600" />
          </button>
          <button
            v-for="pNo in totalPages"
            :key="pNo"
            @click="changePage(pNo)"
            :class="[
              'px-2.5 py-1 rounded-md text-xs font-bold transition-all',
              currentPage === pNo
                ? 'bg-blue-600 text-white'
                : 'border border-slate-200 bg-white hover:bg-slate-50 text-slate-600',
            ]"
          >
            {{ pNo }}
          </button>
          <button
            @click="changePage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="p-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-40 transition-colors"
          >
            <ChevronRightIcon :size="16" class="text-slate-600" />
          </button>
        </div>
      </div>
    </div>

    <StockDeductModal
      :is-open="isDeductModalOpen"
      :stock="selectedStockForDeduct"
      @close="isDeductModalOpen = false"
      @success="handleDeductSuccess"
    />
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import {
  HistoryIcon,
  PackageIcon,
  MinusCircleIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  XIcon,
  Trash2Icon,
} from 'lucide-vue-next'

// YENİ OLUŞTURDUĞUMUZ MODALI İÇERİ ALIYORUZ
import StockDeductModal from './StockDeductModal.vue'
import { useAlert } from '@/composables/useAlert'

const { showSuccess, showError, showConfirm } = useAlert()

const props = defineProps<{ isOpen: boolean; product: any }>()
const emit = defineEmits<{ (e: 'close'): void; (e: 'success'): void }>()

const BASE_URL = '/api'
const stocks = ref<any[]>([])
const isLoading = ref(false)

const currentPage = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)
const totalCount = ref(0)

// Eksiltme Modalı Durumları
const isDeductModalOpen = ref(false)
const selectedStockForDeduct = ref<any>(null)

watch(
  () => props.isOpen,
  async (newVal) => {
    if (newVal && props.product) {
      currentPage.value = 1
      await fetchStockHistory()
    }
  },
)

const fetchStockHistory = async () => {
  if (!props.product) return
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(
      `${BASE_URL}/Stocks/product/${props.product.id}?page=${currentPage.value}&pageSize=${pageSize.value}`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    )

    if (response.ok) {
      const data = await response.json()
      stocks.value = data.items
      totalPages.value = data.totalPages
      totalCount.value = data.totalCount
    }
  } catch (error) {
    console.error('Yükleme hatası:', error)
  } finally {
    isLoading.value = false
  }
}

const changePage = (pageNo: number) => {
  if (pageNo < 1 || pageNo > totalPages.value) return
  currentPage.value = pageNo
  fetchStockHistory()
}

// Modalı Açan Fonksiyon
const openDeductModal = (stock: any) => {
  selectedStockForDeduct.value = stock
  isDeductModalOpen.value = true
}

// Modal İşlemi Başarılı Olunca Çalışacak Fonksiyon
const handleDeductSuccess = async () => {
  isDeductModalOpen.value = false
  await fetchStockHistory() // Bu penceredeki parti listesini yenile
  emit('success') // Ana Stocks.vue tablosunu yenile ki ana stok düşsün
}

const handleDeleteStock = async (stock: any) => {
  const confirmDelete = await showConfirm(
    `Bu stok giriş kaydını (Tarih: ${formatDate(stock.inflowDate)}) silmek istediğinize emin misiniz?`,
  )
  if (!confirmDelete) return

  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`${BASE_URL}/Stocks/delete/${stock.id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })

    const result = await response.json()

    if (response.ok) {
      showSuccess(result.message || 'Kayıt başarıyla silindi.')
      if (props.product) props.product.remainingQuantity -= stock.remainingQuantity
      await fetchStockHistory()
      emit('success')
    } else {
      showError(result.message || 'Silme işlemi başarısız.')
    }
  } catch (error) {
    console.error('Silme hatası:', error)
    showError('Sunucuyla iletişim kurulamadı.')
  }
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleString('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
</style>
