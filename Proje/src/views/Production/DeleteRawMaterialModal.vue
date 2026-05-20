<template>
  <Teleport to="body">
  <div
    v-if="isOpen"
    class="fixed inset-0 z-[80] flex items-center justify-center p-4 animate-fade-in"
  >
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="closeModal"></div>

    <div
      class="relative w-full max-w-md bg-white rounded-2xl shadow-xl border border-slate-100 overflow-hidden transform transition-all z-10"
    >
      <div class="p-6 text-center">
        <div
          class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-50 border border-red-100 text-red-600 mb-4"
        >
          <AlertTriangleIcon :size="24" />
        </div>

        <h3 class="text-lg font-bold text-slate-800 mb-2">Hammaddeyi Sil</h3>
        <p class="text-sm text-slate-500 mb-6 leading-relaxed">
          <span class="font-bold text-slate-700">"{{ rawMaterial?.name }}"</span> isimli hammaddeyi
          tamamen silmek istediğinize emin misiniz?<br />
          <span class="text-red-500 font-medium text-xs"
            >Bu işlem geri alınamaz ve üretim reçetelerinizi etkileyebilir.</span
          >
        </p>

        <div
          v-if="errorMessage"
          class="mb-4 p-3 text-xs text-red-700 bg-red-50 border border-red-100 rounded-xl text-left"
        >
          ⚠️ {{ errorMessage }}
        </div>

        <div class="flex items-center justify-center gap-3">
          <button
            type="button"
            @click="closeModal"
            :disabled="isSubmitting"
            class="flex-1 px-4 py-2.5 rounded-xl text-sm font-medium text-slate-600 bg-slate-50 hover:bg-slate-100 border border-slate-200 transition-colors disabled:opacity-50"
          >
            Vazgeç
          </button>
          <button
            type="button"
            @click="handleDelete"
            :disabled="isSubmitting"
            class="flex-1 px-4 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm shadow-red-100 transition-colors flex items-center justify-center gap-1.5 disabled:opacity-60"
          >
            <Trash2Icon v-if="!isSubmitting" :size="16" />
            <span>{{ isSubmitting ? 'Siliniyor...' : 'Evet, Sil' }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { AlertTriangleIcon, Trash2Icon } from 'lucide-vue-next'

const props = defineProps({
  isOpen: Boolean,
  rawMaterial: Object,
})

const emit = defineEmits(['close', 'success'])

const isSubmitting = ref(false)
const errorMessage = ref('')
const BASE_URL = '/api'

// Pencere her açıldığında eski hata mesajlarını temizle
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) errorMessage.value = ''
  },
)

const handleDelete = async () => {
  if (!props.rawMaterial?.id) return

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const token = localStorage.getItem('token')

    // DİKKAT: Endpoint buradaki "delete/" takısı ile C# tarafındaki yapıya uyarlandı!
    const response = await fetch(`${BASE_URL}/Products/delete/${props.rawMaterial.id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    })

    if (response.ok) {
      emit('success')
      emit('close')
    } else {
      const errData = await response.json()
      errorMessage.value = errData.message || 'Silme işlemi sırasında bir hata oluştu.'
    }
  } catch (error) {
    errorMessage.value = 'Sunucu bağlantı hatası gerçekleşti.'
  } finally {
    isSubmitting.value = false
  }
}

const closeModal = () => {
  if (!isSubmitting.value) emit('close')
}
</script>
