<template>
  <Teleport to="body">
  <div
    v-if="isOpen"
    class="fixed inset-0 z-[70] flex items-center justify-center bg-gray-950/50 backdrop-blur-sm p-4 animate-fade-in"
  >
    <div
      class="relative w-full max-w-md bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden transform transition-all"
    >
      <div class="flex items-center justify-between p-5 border-b border-gray-200 bg-gray-50/50">
        <div class="flex items-center space-x-2.5">
          <span class="text-xl">📥</span>
          <div>
            <h3 class="text-lg font-bold text-gray-900">Stok Girişi Yap</h3>
            <p class="text-xs text-gray-500 font-medium mt-0.5">
              {{ rawMaterial?.name }} (#{{ rawMaterial?.productNo }})
            </p>
          </div>
        </div>
        <button
          @click="closeModal"
          type="button"
          class="text-gray-400 bg-transparent hover:bg-gray-100 hover:text-gray-900 rounded-lg text-sm w-8 h-8 inline-flex justify-center items-center"
        >
          <svg
            class="w-3 h-3"
            aria-hidden="true"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 14 14"
          >
            <path
              stroke="currentColor"
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="m1 1 6 6m0 0 6 6M7 7l6-6M7 7l-6 6"
            />
          </svg>
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <div
          v-if="errorMessage"
          class="p-3 text-sm text-red-700 bg-red-50 border border-red-200 rounded-xl"
        >
          ⚠️ {{ errorMessage }}
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="col-span-2 sm:col-span-1">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700">Gelen Miktar</label>
            <input
              v-model="form.quantity"
              type="number"
              min="1"
              required
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5"
              placeholder="Örn: 100"
            />
          </div>

          <div class="col-span-2 sm:col-span-1">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700"
              >Birim Alış Maliyeti (₺)</label
            >
            <input
              v-model="form.unitCost"
              type="number"
              step="0.01"
              min="0"
              required
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5"
              placeholder="0.00"
            />
          </div>

          <div class="col-span-2">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700"
              >Tedarikçi Firma (Opsiyonel)</label
            >
            <input
              v-model="form.supplierName"
              type="text"
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5"
              placeholder="Örn: Koçtaş, Yıldız Entegre vb."
            />
          </div>

          <div class="col-span-2">
            <label class="block mb-1.5 text-sm font-semibold text-gray-700"
              >Giriş Notu (Opsiyonel)</label
            >
            <textarea
              v-model="form.notes"
              rows="2"
              class="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5"
              placeholder="İrsaliye no, kalite durumu vb..."
            ></textarea>
          </div>
        </div>

        <div
          class="bg-blue-50 rounded-xl p-3 flex justify-between items-center border border-blue-100"
        >
          <span class="text-sm font-medium text-blue-800">Tahmini Toplam Fatura:</span>
          <span class="text-base font-bold text-blue-900">{{
            (form.quantity * form.unitCost).toLocaleString('tr-TR', {
              style: 'currency',
              currency: 'TRY',
            })
          }}</span>
        </div>

        <div class="flex items-center justify-end space-x-2 pt-4 border-t border-gray-100">
          <button
            @click="closeModal"
            type="button"
            class="px-4 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-200 rounded-xl hover:bg-gray-50"
          >
            Vazgeç
          </button>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="px-5 py-2 text-sm font-medium text-white bg-green-600 rounded-xl hover:bg-green-700 disabled:opacity-50"
          >
            <span v-if="isSubmitting">İşleniyor...</span>
            <span v-else>Stoklara Ekle</span>
          </button>
        </div>
      </form>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps({
  isOpen: Boolean,
  rawMaterial: Object,
})
const emit = defineEmits(['close', 'success'])

const isSubmitting = ref(false)
const errorMessage = ref('')

const form = ref({
  quantity: 1,
  unitCost: 0,
  supplierName: '',
  notes: '',
})

watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      errorMessage.value = ''
      // Modal açıldığında hammaddenin kayıtlı varsayılan birim fiyatını otomatik getiririz
      form.value = {
        quantity: 1,
        unitCost: props.rawMaterial?.unitPrice || 0,
        supplierName: '',
        notes: '',
      }
    }
  },
)

const handleSubmit = async () => {
  if (!props.rawMaterial?.id) return

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const token = localStorage.getItem('token')
    const response = await fetch('/api/Stocks/inflow', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        productId: props.rawMaterial.id,
        quantity: form.value.quantity,
        unitCost: form.value.unitCost,
        supplierName: form.value.supplierName,
        notes: form.value.notes,
      }),
    })

    if (response.ok) {
      emit('success')
      closeModal()
    } else {
      const errData = await response.json()
      errorMessage.value = errData.message || 'Stok girişi başarısız oldu.'
    }
  } catch (error) {
    errorMessage.value = 'Sunucu bağlantı hatası.'
  } finally {
    isSubmitting.value = false
  }
}

const closeModal = () => emit('close')
</script>
