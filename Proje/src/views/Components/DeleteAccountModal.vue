<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-950/60 backdrop-blur-xs" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-2xl border border-red-100 w-full max-w-sm overflow-hidden transform transition-all z-10 animate-in fade-in zoom-in-95 duration-200"
    >
      <div class="p-6 text-center space-y-4">
        <div
          class="w-12 h-12 rounded-full bg-red-50 text-red-600 flex items-center justify-center mx-auto shadow-sm"
        >
          <AlertTriangleIcon :size="24" />
        </div>

        <div>
          <h3 class="text-base font-bold text-slate-800">Hesabınızı Silmek İstiyor Musunuz?</h3>
          <p class="text-xs text-slate-500 mt-2 leading-relaxed">
            Bu işlem geri alınamaz. Hesabınız silindiğinde tanımladığınız tüm ürün kartları,
            kategoriler ve ambar geçmişiniz <strong>kalıcı olarak yok edilecektir.</strong>
          </p>
        </div>

        <div class="flex flex-col gap-2 pt-2">
          <button
            @click="handleDelete"
            :disabled="isLoading"
            class="w-full py-2.5 rounded-xl text-xs font-semibold bg-red-600 hover:bg-red-700 text-white transition-colors shadow-sm shadow-red-200 disabled:opacity-60"
          >
            {{ isLoading ? 'Hesap Yok Ediliyor...' : 'Evet, Hesabımı Kalıcı Olarak Sil' }}
          </button>
          <button
            @click="emit('close')"
            :disabled="isLoading"
            class="w-full py-2.5 rounded-xl text-xs font-semibold bg-slate-100 hover:bg-slate-200 text-slate-700 transition-colors disabled:opacity-50"
          >
            Vazgeç, İptal Et
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { AlertTriangleIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const { showSuccess, showError } = useAlert()

const isLoading = ref(false)
const router = useRouter()
const BASE_URL = '/api'

const handleDelete = async () => {
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')

    // Backend tarafındaki hesap silme ucuna DELETE isteği gönderiyoruz
    const response = await fetch(`${BASE_URL}/Auth/delete-account`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })

    if (response.ok) {
      showSuccess('Hesabınız ve tüm verileriniz başarıyla silindi.')
      localStorage.removeItem('token')
      router.push('/login')
    } else {
      const err = await response.json()
      showError(err.message || 'Hesap silinirken bir hata meydana geldi.')
    }
  } catch (error) {
    console.error('Hesap silme hatası:', error)
    showError('Sunucu bağlantısı başarısız.')
  } finally {
    isLoading.value = false
  }
}
</script>
