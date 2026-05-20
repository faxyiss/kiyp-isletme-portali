<!-- AddLeaveModal.vue -->
<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-sm overflow-hidden z-10"
    >
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100">
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <CalendarIcon :size="18" class="text-amber-500" />
          İzin Kaydı Ekle
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <!-- Personel -->
        <div
          v-if="employee"
          class="flex items-center gap-3 bg-slate-50 rounded-xl p-3 border border-slate-100"
        >
          <div
            class="w-9 h-9 rounded-xl bg-linear-to-r from-blue-500 to-indigo-500 text-white flex items-center justify-center font-bold text-sm flex-shrink-0"
          >
            {{ employee.fullName?.charAt(0) }}
          </div>
          <div>
            <p class="font-bold text-slate-800 text-sm">{{ employee.fullName }}</p>
            <p class="text-xs text-slate-400">{{ employee.position }}</p>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
              >Başlangıç</label
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
              >Bitiş</label
            >
            <input
              v-model="form.endDate"
              type="date"
              required
              :disabled="isLoading"
              :min="form.startDate"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
        </div>

        <!-- Gün hesabı -->
        <div
          v-if="dayCount > 0"
          class="flex items-center justify-center gap-2 bg-amber-50 border border-amber-100 rounded-xl py-2.5"
        >
          <CalendarIcon :size="14" class="text-amber-500" />
          <span class="text-sm font-bold text-amber-700">{{ dayCount }} gün izin</span>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Açıklama (Opsiyonel)</label
          >
          <textarea
            v-model="form.reason"
            rows="2"
            :disabled="isLoading"
            placeholder="İzin sebebi..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60 resize-none"
          ></textarea>
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
            :disabled="isLoading || !form.startDate || !form.endDate"
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-amber-500 hover:bg-amber-600 text-white shadow-sm shadow-amber-200 transition-colors flex items-center gap-1.5 disabled:opacity-60"
          >
            <CheckIcon v-if="!isLoading" :size="15" />
            {{ isLoading ? 'Kaydediliyor...' : 'İzin Ekle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { CalendarIcon, XIcon, CheckIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showError } = useAlert()

const props = defineProps<{ isOpen: boolean; employee: any }>()
const emit = defineEmits<{ (e: 'close'): void; (e: 'success'): void }>()

const BASE_URL = '/api'
const isLoading = ref(false)

const defaultForm = () => ({ startDate: '', endDate: '', reason: '' })
const form = ref(defaultForm())

const dayCount = computed(() => {
  if (!form.value.startDate || !form.value.endDate) return 0
  const diff = new Date(form.value.endDate).getTime() - new Date(form.value.startDate).getTime()
  return Math.max(0, Math.floor(diff / 86400000) + 1)
})

watch(
  () => props.isOpen,
  (v) => {
    if (!v) form.value = defaultForm()
  },
)

const handleSubmit = async () => {
  if (isLoading.value || !props.employee) return
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Employees/${props.employee.id}/leaves`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        startDate: new Date(form.value.startDate).toISOString(),
        endDate: new Date(form.value.endDate).toISOString(),
        reason: form.value.reason || null,
      }),
    })
    const data = await res.json()
    if (res.ok) {
      emit('success')
      emit('close')
    } else showError(data.message || 'İzin kaydedilemedi.')
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>
