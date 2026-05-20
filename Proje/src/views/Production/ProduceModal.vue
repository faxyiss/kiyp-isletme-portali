<template>
  <Teleport to="body">
  <div
    v-if="isOpen"
    class="fixed inset-0 z-[60] flex items-center justify-center bg-gray-950/60 backdrop-blur-sm p-4 animate-fade-in"
  >
    <div
      class="relative w-full max-w-sm bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden transform transition-all scale-100"
    >
      <div class="p-6 text-center">
        <div
          class="w-16 h-16 bg-emerald-100 text-emerald-600 rounded-full flex items-center justify-center mx-auto mb-4"
        >
          <PlayCircleIcon :size="32" />
        </div>

        <h3 class="text-xl font-bold text-gray-900 mb-1">Üretime Başla</h3>
        <p class="text-sm text-gray-500 mb-6">
          <b class="text-gray-800">{{ recipe?.productName }}</b> ürününden kaç adet üretmek
          istiyorsunuz?
        </p>

        <!-- Hata Mesajı -->
        <div
          v-if="errorMessage"
          class="mb-4 p-3 text-sm text-red-700 bg-red-50 border border-red-200 rounded-xl text-left"
        >
          ⚠️ {{ errorMessage }}
        </div>

        <!-- Başarı Mesajı -->
        <div
          v-if="successMessage"
          class="mb-4 p-3 text-sm text-emerald-700 bg-emerald-50 border border-emerald-200 rounded-xl text-left"
        >
          ✅ {{ successMessage }}
        </div>

        <div class="text-left mb-6">
          <label class="block mb-1.5 text-sm font-bold text-gray-700">Üretim Miktarı</label>
          <div class="flex items-center gap-2">
            <button
              type="button"
              @click="decrement"
              :disabled="quantity <= 1 || isSubmitting"
              class="w-11 h-11 flex items-center justify-center rounded-xl border border-gray-200 bg-white text-gray-600 text-xl font-bold hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-all shrink-0"
            >−</button>
            <input
              v-model.number="quantity"
              @input="clampQuantity"
              type="number"
              min="1"
              step="1"
              class="bg-gray-50 border border-gray-300 text-gray-900 text-lg font-bold rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 block w-full p-3 text-center transition-all outline-none [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
            />
            <button
              type="button"
              @click="increment"
              :disabled="isSubmitting"
              class="w-11 h-11 flex items-center justify-center rounded-xl border border-gray-200 bg-white text-gray-600 text-xl font-bold hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-all shrink-0"
            >+</button>
          </div>
        </div>

        <div class="flex gap-3">
          <button
            @click="closeModal"
            type="button"
            :disabled="isSubmitting"
            class="flex-1 px-4 py-2.5 text-sm font-bold text-gray-600 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors disabled:opacity-50"
          >
            İptal
          </button>
          <button
            @click="handleProduce"
            :disabled="isSubmitting || !isQuantityValid"
            class="flex-1 px-4 py-2.5 text-sm font-bold text-white bg-emerald-600 rounded-xl hover:bg-emerald-700 focus:ring-4 focus:outline-none focus:ring-emerald-200 disabled:opacity-50 transition-colors"
          >
            <span v-if="isSubmitting">Üretiliyor...</span>
            <span v-else>Üretimi Tamamla</span>
          </button>
        </div>
      </div>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { PlayCircleIcon } from 'lucide-vue-next'

const props = defineProps({
  isOpen: Boolean,
  recipe: Object,
})

const emit = defineEmits(['close', 'success'])

const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const quantity = ref(1)

// Miktar geçerlilik kontrolü — NaN, negatif ve sıfıra karşı koruma
const increment = () => { quantity.value = Math.floor(quantity.value || 1) + 1 }
const decrement = () => { quantity.value = Math.max(1, Math.floor(quantity.value || 1) - 1) }
const clampQuantity = () => {
  const v = Math.floor(quantity.value)
  quantity.value = isNaN(v) || v < 1 ? 1 : v
}

const isQuantityValid = computed(() => {
  return (
    quantity.value !== null &&
    quantity.value !== undefined &&
    !isNaN(quantity.value) &&
    quantity.value > 0
  )
})

// Modal her açıldığında state'i sıfırla
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      quantity.value = 1
      errorMessage.value = ''
      successMessage.value = ''
      isSubmitting.value = false
    }
  },
)

const handleProduce = async () => {
  // Ekstra güvenlik kontrolü
  if (!isQuantityValid.value) {
    errorMessage.value = 'Lütfen geçerli bir miktar girin.'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const token = localStorage.getItem('token')
    const response = await fetch('/api/Production/produce', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        productId: props.recipe?.productId,
        quantity: Number(quantity.value),
      }),
    })

    const data = await response.json()

    if (response.ok) {
      successMessage.value = 'Üretim başarıyla tamamlandı ve stoklara eklendi.'
      // isSubmitting = true kalır → buton kapalı, modal kapanana kadar tekrar basılamaz
      setTimeout(() => {
        emit('success')
      }, 800)
    } else {
      errorMessage.value =
        data.message || data.title || data.detail || 'Üretim sırasında bir hata oluştu.'
      isSubmitting.value = false
    }
  } catch (error) {
    errorMessage.value = 'Sunucu bağlantı hatası. Lütfen tekrar deneyin.'
    isSubmitting.value = false
  }
}

const closeModal = () => {
  if (!isSubmitting.value) {
    emit('close')
  }
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.15s ease-out forwards;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-4px) scale(0.99);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>
