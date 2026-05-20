<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-0">
    <div
      class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm transition-opacity"
      @click="$emit('close')"
    ></div>

    <div
      class="bg-white rounded-2xl shadow-xl w-full max-w-4xl overflow-hidden relative z-10 flex flex-col max-h-[90vh] animate-slide-up"
    >
      <div
        class="bg-gradient-to-r from-slate-800 to-slate-700 p-5 flex items-center justify-between shrink-0"
      >
        <div>
          <h3 class="text-lg font-bold text-white flex items-center gap-2">
            <HistoryIcon :size="20" class="text-blue-400" />
            {{ customer?.firstName }} {{ customer?.lastName }} - Alışveriş Geçmişi
          </h3>
          <p class="text-xs text-slate-300 mt-1">Müşterinin bugüne kadar yaptığı tüm işlemler.</p>
        </div>
        <div class="text-right">
          <p class="text-[10px] text-slate-300 font-bold uppercase tracking-wider mb-0.5">
            Toplam Kazanç
          </p>
          <span class="text-xl font-black text-emerald-400">{{ formatCurrency(grandTotal) }}</span>
        </div>
      </div>

      <div class="flex-1 overflow-y-auto bg-slate-50/50 p-4 custom-scrollbar">
        <div v-if="isLoading" class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>

        <div
          v-else-if="historyList.length === 0"
          class="text-center py-12 bg-white rounded-xl border border-slate-100"
        >
          <ShoppingBagIcon :size="40" class="mx-auto mb-3 text-slate-300" />
          <p class="font-medium text-slate-500">Bu müşteriye ait geçmiş alışveriş bulunamadı.</p>
        </div>

        <div v-else class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr
                class="bg-slate-50 border-b border-slate-100 text-[11px] font-bold uppercase tracking-wider text-slate-500"
              >
                <th class="py-3 px-4">Tarih</th>
                <th class="py-3 px-4">Ürün</th>
                <th class="py-3 px-4 text-center">Miktar</th>
                <th class="py-3 px-4 text-center">Ödeme</th>
                <th class="py-3 px-4 text-right">Tutar</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 text-sm text-slate-600">
              <tr
                v-for="item in historyList"
                :key="item.saleId"
                class="hover:bg-slate-50/50 transition-colors"
              >
                <td class="py-3 px-4 text-xs font-medium text-slate-500">
                  {{ formatDate(item.saleDate) }} <br />
                  <span class="text-[10px] text-slate-400">{{ formatTime(item.saleDate) }}</span>
                </td>
                <td class="py-3 px-4">
                  <div class="font-bold text-slate-700 text-xs">{{ item.productName }}</div>
                  <div class="text-[10px] text-slate-400 mt-0.5">
                    Birim: {{ formatCurrency(item.salePrice) }}
                  </div>
                </td>
                <td class="py-3 px-4 text-center font-black text-slate-700">{{ item.quantity }}</td>
                <td class="py-3 px-4 text-center">
                  <span
                    v-if="item.paymentType === 'Nakit' || item.paymentType === '0'"
                    class="px-2 py-0.5 rounded text-[10px] font-black bg-emerald-50 text-emerald-600 border border-emerald-100"
                    >Nakit</span
                  >
                  <span
                    v-else-if="item.paymentType === 'KrediKarti' || item.paymentType === '1'"
                    class="px-2 py-0.5 rounded text-[10px] font-black bg-blue-50 text-blue-600 border border-blue-100"
                    >Kart</span
                  >
                  <span
                    v-else-if="item.paymentType === 'Veresiye' || item.paymentType === '2'"
                    class="px-2 py-0.5 rounded text-[10px] font-black bg-rose-50 text-rose-600 border border-rose-100"
                    >Veresiye</span
                  >
                  <span
                    v-else
                    class="px-2 py-0.5 rounded text-[10px] font-black bg-slate-100 text-slate-600"
                    >{{ item.paymentType }}</span
                  >
                </td>
                <td class="py-3 px-4 text-right font-black text-slate-800">
                  {{ formatCurrency(item.totalAmount) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div
        class="p-4 border-t border-slate-100 bg-white flex justify-between items-center shrink-0"
      >
        <button
          @click="$emit('close')"
          class="px-5 py-2 rounded-xl text-sm font-bold text-slate-600 bg-slate-100 hover:bg-slate-200 transition-colors"
        >
          Kapat
        </button>

        <div v-if="totalPages > 1" class="flex items-center gap-2">
          <button
            @click="changePage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            Önceki
          </button>
          <span
            class="text-xs font-bold text-slate-700 bg-slate-50 px-3 py-1.5 rounded-lg border border-slate-100"
            >{{ currentPage }} / {{ totalPages }}</span
          >
          <button
            @click="changePage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            Sonraki
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import axios from 'axios'
import { HistoryIcon, ShoppingBagIcon } from 'lucide-vue-next'

const props = defineProps<{
  isOpen: boolean
  customer: any
}>()

const emit = defineEmits(['close'])

const historyList = ref<any[]>([])
const isLoading = ref(false)
const currentPage = ref(1)
const totalPages = ref(1)
const grandTotal = ref(0)

const getAuthHeaders = () => ({
  headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
})

const fetchHistory = async (page = 1) => {
  if (!props.customer?.id) return
  isLoading.value = true
  currentPage.value = page
  try {
    const res = await axios.get(
      `/Sales/customer/${props.customer.id}?page=${page}&pageSize=8`,
      getAuthHeaders(),
    )
    historyList.value = res.data.items || []
    totalPages.value = res.data.totalPages || 1
    grandTotal.value = res.data.grandTotalSpent || 0
  } catch (error) {
    console.error('Geçmiş çekilemedi:', error)
  } finally {
    isLoading.value = false
  }
}

const changePage = (newPage: number) => {
  if (newPage > 0 && newPage <= totalPages.value) fetchHistory(newPage)
}

// Modal açıldığında veriyi çek, kapandığında temizle
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      fetchHistory(1)
    } else {
      historyList.value = []
      grandTotal.value = 0
    }
  },
)

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
.animate-slide-up {
  animation: slideUp 0.3s ease-out forwards;
}
@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
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
