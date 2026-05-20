<template>
  <Teleport to="body">
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>

    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-md overflow-hidden transform transition-all z-10"
    >
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50"
      >
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <TrendingUpIcon :size="18" class="text-emerald-600" />
          Stok Girişi Yap
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-red-50 hover:text-red-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-5">
        <div class="p-3 bg-slate-50 border border-slate-100 rounded-xl flex items-center gap-3">
          <div
            class="w-10 h-10 rounded-lg bg-white shadow-sm flex items-center justify-center text-slate-400"
          >
            <PackageIcon :size="20" />
          </div>
          <div v-if="product">
            <div class="text-xs font-semibold text-slate-400 uppercase">Seçili Ürün</div>
            <div class="font-bold text-slate-700 text-sm truncate max-w-[250px]">
              {{ product.name }}
            </div>
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Giriş Miktarı (Adet)</label
          >
          <input
            v-model.number="form.inflowQuantity"
            type="number"
            min="1"
            step="1"
            required
            :disabled="isLoading"
            placeholder="Örn: 100"
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Birim Maliyet (₺)</label
          >
          <div class="relative">
            <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 font-medium"
              >₺</span
            >
            <input
              v-model.number="form.unitCost"
              type="number"
              min="0.01"
              step="0.01"
              required
              :disabled="isLoading"
              placeholder="Örn: 15.50"
              class="w-full pl-8 pr-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
          <p class="text-[10px] text-slate-400 mt-1">
            Bu fiyat satış fiyatı değil, depoya alış/üretim maliyetinizdir.
          </p>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >İşlem Tarihi (Opsiyonel)</label
          >
          <input
            v-model="form.inflowDate"
            type="datetime-local"
            :disabled="isLoading"
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-sm text-slate-600 disabled:opacity-60"
          />
          <p class="text-[10px] text-slate-400 mt-1">Boş bırakırsanız şu anki zaman baz alınır.</p>
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-100 mt-6">
          <button
            type="button"
            @click="emit('close')"
            :disabled="isLoading"
            class="px-4 py-2 rounded-xl text-sm font-medium text-slate-600 hover:bg-slate-50 transition-colors disabled:opacity-50"
          >
            İptal
          </button>
          <button
            type="submit"
            :disabled="isLoading"
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-emerald-600 hover:bg-emerald-700 text-white shadow-sm shadow-emerald-200 transition-colors flex items-center gap-2 disabled:opacity-60"
          >
            <TrendingUpIcon v-if="!isLoading" :size="16" />
            {{ isLoading ? 'İşleniyor...' : 'Stoğa Ekle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { TrendingUpIcon, XIcon, PackageIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showSuccess, showError } = useAlert()

const props = defineProps<{
  isOpen: boolean
  product: any // Seçilen ürün verisi buraya gelecek
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const isLoading = ref(false)
const BASE_URL = '/api'

const defaultForm = {
  inflowQuantity: '' as number | '',
  unitCost: '' as number | '',
  inflowDate: '',
}

const form = ref({ ...defaultForm })

// Modal kapandığında formu sıfırla
watch(
  () => props.isOpen,
  (newVal) => {
    if (!newVal) {
      form.value = { ...defaultForm }
    }
  },
)

const handleSubmit = async () => {
  if (!props.product || isLoading.value) return

  isLoading.value = true
  try {
    const token = localStorage.getItem('token')

    // API'ye gönderilecek veriyi hazırla
    const payload: any = {
      productId: props.product.id,
      inflowQuantity: Number(form.value.inflowQuantity),
      unitCost: Number(form.value.unitCost),
    }

    // Eğer tarih girilmişse ISO formatına çevirerek ekle
    if (form.value.inflowDate) {
      payload.inflowDate = new Date(form.value.inflowDate).toISOString()
    }

    const response = await fetch(`${BASE_URL}/Stocks/inflow`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    const data = await response.json()

    if (response.ok) {
      showSuccess(data.message || 'Stok başarıyla eklendi.')
      emit('success')
      emit('close')
    } else {
      showError(data.message || 'Stok eklenirken bir hata oluştu.')
    }
  } catch (error) {
    console.error('Stok ekleme hatası:', error)
    showError('İşlem sırasında sunucu hatası meydana geldi.')
  } finally {
    isLoading.value = false
  }
}
</script>
