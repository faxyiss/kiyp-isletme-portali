<template>
  <div
    v-if="isVisible"
    class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm"
  >
    <div class="bg-white w-full max-w-lg rounded-2xl shadow-2xl p-8 border border-slate-100">
      <div class="mb-6">
        <h2 class="text-2xl font-bold text-slate-800">İşletme Profilini Tamamla</h2>
        <p class="text-slate-500 text-sm mt-1">
          Sistemi kullanmaya başlamadan önce fatura ve raporlamalar için işletme detaylarınızı
          girmelisiniz.
        </p>
      </div>

      <form @submit.prevent="saveBusinessProfile" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1">İşletme / Firma Adı *</label>
          <input
            v-model="profileForm.companyName"
            required
            type="text"
            class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-colors"
            placeholder="Resmi İşletme Adı"
          />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Telefon Numarası</label>
            <input
              v-model="profileForm.phone"
              type="text"
              class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-colors"
              placeholder="05XX XXX XX XX"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">İletişim E-posta</label>
            <input
              v-model="profileForm.contactEmail"
              type="email"
              class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-colors"
              placeholder="bilgi@firma.com"
            />
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1">Açık Adres</label>
          <textarea
            v-model="profileForm.address"
            rows="2"
            class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-colors resize-none"
            placeholder="Mahalle, Cadde, Sokak..."
          ></textarea>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Vergi Dairesi</label>
            <input
              v-model="profileForm.taxOffice"
              type="text"
              class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-colors"
              placeholder="Örn: Şişli"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Vergi / TC No</label>
            <input
              v-model="profileForm.taxNumber"
              type="text"
              class="w-full px-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-colors"
              placeholder="10 veya 11 Haneli"
            />
          </div>
        </div>

        <div class="pt-4 mt-6 border-t border-slate-100 flex justify-end">
          <button
            type="submit"
            :disabled="isSaving"
            class="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2.5 px-6 rounded-lg transition-colors disabled:bg-blue-400 disabled:cursor-not-allowed flex items-center"
          >
            <span v-if="isSaving">Kaydediliyor...</span>
            <span v-else>Kaydet ve Başla</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAlert } from '@/composables/useAlert'

const { showError } = useAlert()

const isVisible = ref(false)
const isSaving = ref(false)

const profileForm = ref({
  companyName: '',
  phone: '',
  contactEmail: '',
  address: '',
  taxOffice: '',
  taxNumber: '',
})

const API_URL = '/api/businessprofile'

onMounted(async () => {
  await checkBusinessProfile()
})

const checkBusinessProfile = async () => {
  const token = localStorage.getItem('token')
  if (!token) return

  try {
    const response = await fetch(`${API_URL}/check`, {
      headers: { Authorization: `Bearer ${token}` },
    })

    // Eğer profil yoksa 404 döneceğini varsayıyoruz
    if (response.status === 404) {
      isVisible.value = true

      const user = JSON.parse(localStorage.getItem('user') || '{}')
      if (user.fullName) {
        profileForm.value.companyName = user.fullName
      }
    }
  } catch (error) {
    console.error('Profil kontrolü başarısız:', error)
  }
}

const saveBusinessProfile = async () => {
  if (isSaving.value) return
  isSaving.value = true

  const token = localStorage.getItem('token')

  try {
    const response = await fetch(`${API_URL}/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(profileForm.value),
    })

    if (!response.ok) {
      throw new Error('Profil kaydedilirken bir hata oluştu.')
    }

    // Başarılı olursa kendini gizler, ana sayfa kullanılabilir hale gelir
    isVisible.value = false
  } catch (error: any) {
    showError(error.message)
  } finally {
    isSaving.value = false
  }
}
</script>
