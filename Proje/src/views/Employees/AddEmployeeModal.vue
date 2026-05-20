<!-- AddEmployeeModal.vue -->
<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-md overflow-hidden z-10"
    >
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100">
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <UserPlusIcon :size="18" class="text-blue-600" />
          Yeni Personel Ekle
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
              placeholder="Örn: Ahmet Yılmaz"
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
              placeholder="Örn: Kasiyer"
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
              placeholder="05xx xxx xx xx"
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
              placeholder="0,00"
              class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
            />
          </div>
        </div>

        <!-- Hesaplama Önizlemesi -->
        <div
          v-if="form.grossSalary > 0"
          class="bg-slate-50 rounded-xl p-4 space-y-2 border border-slate-100"
        >
          <p class="text-xs font-bold text-slate-500 uppercase tracking-wide mb-2">
            Maaş Hesaplama Önizlemesi
          </p>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">SGK (Çalışan %14)</span>
            <span class="font-semibold text-slate-700"
              >-{{ formatCurrency(calc.employeeSGK) }}</span
            >
          </div>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">İşsizlik (Çalışan %1)</span>
            <span class="font-semibold text-slate-700"
              >-{{ formatCurrency(calc.employeeUnemployment) }}</span
            >
          </div>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">Gelir Vergisi (%15)</span>
            <span class="font-semibold text-slate-700">-{{ formatCurrency(calc.incomeTax) }}</span>
          </div>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">Damga Vergisi (%0.759)</span>
            <span class="font-semibold text-slate-700">-{{ formatCurrency(calc.stampTax) }}</span>
          </div>
          <div class="border-t border-slate-200 pt-2 flex justify-between text-xs">
            <span class="font-bold text-emerald-600">Net Maaş</span>
            <span class="font-bold text-emerald-600">{{ formatCurrency(calc.netSalary) }}</span>
          </div>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">SGK (İşveren %20.5)</span>
            <span class="font-semibold text-slate-700"
              >+{{ formatCurrency(calc.employerSGK) }}</span
            >
          </div>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">İşsizlik (İşveren %2)</span>
            <span class="font-semibold text-slate-700"
              >+{{ formatCurrency(calc.employerUnemployment) }}</span
            >
          </div>
          <div class="border-t border-slate-200 pt-2 flex justify-between text-xs">
            <span class="font-bold text-red-600">Toplam İşveren Maliyeti</span>
            <span class="font-bold text-red-600">{{ formatCurrency(calc.totalEmployerCost) }}</span>
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
            :disabled="isLoading || !form.fullName || !form.position || !form.grossSalary"
            class="px-5 py-2.5 rounded-xl text-sm font-medium bg-blue-600 hover:bg-blue-700 text-white shadow-sm shadow-blue-200 transition-colors flex items-center gap-1.5 disabled:opacity-60"
          >
            <CheckIcon v-if="!isLoading" :size="15" />
            {{ isLoading ? 'Kaydediliyor...' : 'Personel Ekle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { UserPlusIcon, XIcon, CheckIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showError } = useAlert()

const props = defineProps<{ isOpen: boolean }>()
const emit = defineEmits<{ (e: 'close'): void; (e: 'success'): void }>()

const BASE_URL = '/api'
const isLoading = ref(false)

const defaultForm = () => ({
  fullName: '',
  position: '',
  phone: '',
  startDate: new Date().toISOString().slice(0, 10),
  grossSalary: 0,
})
const form = ref(defaultForm())

// Anlık hesaplama
const RATES = {
  empSGK: 0.14,
  empUnemp: 0.01,
  incomeTax: 0.15,
  stamp: 0.00759,
  erSGK: 0.205,
  erUnemp: 0.02,
}

const calc = computed(() => {
  const g = form.value.grossSalary || 0
  const empSGK = Math.round(g * RATES.empSGK * 100) / 100
  const empUnemp = Math.round(g * RATES.empUnemp * 100) / 100
  const incomeTax = Math.round((g - empSGK - empUnemp) * RATES.incomeTax * 100) / 100
  const stamp = Math.round(g * RATES.stamp * 100) / 100
  const net = Math.round((g - empSGK - empUnemp - incomeTax - stamp) * 100) / 100
  const erSGK = Math.round(g * RATES.erSGK * 100) / 100
  const erUnemp = Math.round(g * RATES.erUnemp * 100) / 100
  return {
    employeeSGK: empSGK,
    employeeUnemployment: empUnemp,
    incomeTax,
    stampTax: stamp,
    netSalary: net,
    employerSGK: erSGK,
    employerUnemployment: erUnemp,
    totalEmployerCost: Math.round((g + erSGK + erUnemp) * 100) / 100,
  }
})

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(v)

watch(
  () => props.isOpen,
  (v) => {
    if (!v) form.value = defaultForm()
  },
)

const handleSubmit = async () => {
  if (isLoading.value) return
  isLoading.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE_URL}/Employees`, {
      method: 'POST',
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
    } else showError(data.message || 'Hata oluştu.')
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>
