<template>
  <Teleport to="body">
  <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-2xl border border-slate-100 w-full max-w-sm overflow-hidden transform transition-all z-10 animate-in fade-in zoom-in-95 duration-200"
    >
      <div
        class="flex items-center justify-between px-5 py-4 border-b border-amber-100 bg-amber-50/50"
      >
        <h3 class="text-base font-bold text-amber-800 flex items-center gap-2">
          <MinusCircleIcon :size="18" /> Stok Eksilt (Fire / Zayi)
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-amber-400 hover:bg-amber-100 hover:text-amber-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-5 space-y-4">
        <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 text-sm">
          <div class="flex justify-between mb-1.5">
            <span class="text-slate-500 font-medium">Bu Partideki Mevcut:</span>
            <span class="font-extrabold text-blue-600">{{ stock?.remainingQuantity }} Adet</span>
          </div>
          <div class="flex justify-between">
            <span class="text-slate-500 font-medium">Birim Maliyeti:</span>
            <span class="font-bold text-slate-700"
              >₺{{ stock?.unitCost?.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) }}</span
            >
          </div>
        </div>

        <div>
          <label class="block text-xs font-bold text-slate-500 uppercase tracking-wide mb-1.5"
            >Eksiltilecek Miktar</label
          >
          <input
            v-model.number="form.quantity"
            type="number"
            min="1"
            :max="stock?.remainingQuantity"
            required
            :disabled="isLoading"
            placeholder="Örn: 5"
            class="w-full px-3 py-2.5 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-all text-sm disabled:opacity-60 font-medium text-slate-700"
          />
          <p class="text-[10px] text-slate-400 mt-1.5 leading-tight">
            Sayım eksikliği, çalınma, kırılma veya bozulma gibi durumlarda stoku eşitlemek için
            kullanın.
          </p>
        </div>

        <div
          v-if="form.quantity > stock?.remainingQuantity"
          class="text-xs text-red-500 font-bold flex items-center gap-1"
        >
          Mevcut miktardan ({{ stock?.remainingQuantity }}) fazla eksiltemezsiniz!
        </div>

        <div class="flex justify-end gap-2 pt-3 border-t border-slate-100 mt-2">
          <button
            type="button"
            @click="emit('close')"
            :disabled="isLoading"
            class="px-4 py-2 rounded-xl text-sm font-bold text-slate-500 hover:bg-slate-50 transition-colors"
          >
            İptal
          </button>
          <button
            type="submit"
            :disabled="
              isLoading ||
              !form.quantity ||
              form.quantity > stock?.remainingQuantity ||
              form.quantity < 1
            "
            class="px-5 py-2 rounded-xl text-sm font-bold bg-amber-600 hover:bg-amber-700 text-white shadow-sm shadow-amber-200 transition-colors flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <MinusCircleIcon v-if="!isLoading" :size="16" />
            {{ isLoading ? 'İşleniyor...' : 'Eksilt' }}
          </button>
        </div>
      </form>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { XIcon, MinusCircleIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showSuccess, showError } = useAlert()

const props = defineProps<{ isOpen: boolean; stock: any }>()
const emit = defineEmits<{ (e: 'close'): void; (e: 'success'): void }>()

const isLoading = ref(false)
const BASE_URL = '/api'

const form = ref({ quantity: '' as number | '' })

// Modal kapandığında formu sıfırla
watch(
  () => props.isOpen,
  (newVal) => {
    if (!newVal) form.value.quantity = ''
  },
)

const handleSubmit = async () => {
  if (!props.stock || !form.value.quantity) return

  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`${BASE_URL}/Stocks/deduct`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        stockId: props.stock.id,
        quantity: Number(form.value.quantity),
      }),
    })

    const result = await response.json()

    if (response.ok) {
      showSuccess(result.message || 'Stok başarıyla eksiltildi.')
      emit('success')
      emit('close')
    } else {
      showError(result.message || 'Eksiltme işlemi başarısız.')
    }
  } catch (error) {
    console.error('Hata:', error)
    showError('Sunucu hatası.')
  } finally {
    isLoading.value = false
  }
}
</script>
