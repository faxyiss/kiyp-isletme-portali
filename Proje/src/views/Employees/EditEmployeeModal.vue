<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-md overflow-hidden z-10"
    >
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100">
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <EditIcon :size="18" class="text-blue-600" />
          Personel Düzenle
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div class="col-span-2">
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
              >Ad Soyad</label
            >
            <input
              v-model="form.fullName"
              type="text"
              required
              :disabled="isLoading"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
              >Görevi</label
            >
            <input
              v-model="form.position"
              type="text"
              required
              :disabled="isLoading"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
              >Telefon</label
            >
            <input
              v-model="form.phone"
              type="tel"
              :disabled="isLoading"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
              >İşe Başlangıç</label
            >
            <input
              v-model="form.startDate"
              type="date"
              required
              :disabled="isLoading"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
              >Brüt Maaş (₺)</label
            >
            <input
              v-model.number="form.grossSalary"
              type="number"
              min="0.01"
              step="0.01"
              required
              :disabled="isLoading"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
        </div>

        <!-- Durum -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-2"
            >Durum</label
          >
          <div class="flex gap-3">
            <button
              type="button"
              @click="form.isActive = true"
              class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-all"
              :class="
                form.isActive
                  ? 'bg-emerald-50 border-emerald-400 text-emerald-700'
                  : 'border-slate-200 text-slate-500 hover:bg-slate-50'
              "
            >
              ✓ Aktif
            </button>
            <button
              type="button"
              @click="form.isActive = false"
              class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-all"
              :class="
                !form.isActive
                  ? 'bg-slate-100 border-slate-400 text-slate-700'
                  : 'border-slate-200 text-slate-500 hover:bg-slate-50'
              "
            >
              ✗ Pasif
            </button>
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 pt-2 border-t border-slate-100">
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
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-blue-600 hover:bg-blue-700 text-white shadow-sm shadow-blue-200 transition-colors flex items-center gap-1.5 disabled:opacity-60"
          >
            <CheckIcon v-if="!isLoading" :size="15" />
            {{ isLoading ? 'Kaydediliyor...' : 'Güncelle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { EditIcon, XIcon, CheckIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showError } = useAlert()

const props = defineProps<{ isOpen: boolean; employee: any }>()
const emit = defineEmits<{ (e: 'close'): void; (e: 'success'): void }>()

const BASE_URL = '/api'
const isLoading = ref(false)

const form = ref({
  fullName: '',
  position: '',
  phone: '',
  startDate: '',
  grossSalary: 0,
  isActive: true,
})

watch(
  () => props.isOpen,
  (v) => {
    if (v && props.employee) {
      form.value = {
        fullName: props.employee.fullName,
        position: props.employee.position,
        phone: props.employee.phone || '',
        startDate: props.employee.startDate?.slice(0, 10) ?? '',
        grossSalary: props.employee.grossSalary,
        isActive: props.employee.isActive,
      }
    }
  },
)

const handleSubmit = async () => {
  if (isLoading.value || !props.employee) return
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Employees/${props.employee.id}`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ...form.value,
        startDate: new Date(form.value.startDate).toISOString(),
      }),
    })
    const data = await res.json()
    if (res.ok) {
      emit('success')
      emit('close')
    } else showError(data.message || 'Güncelleme başarısız.')
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>
