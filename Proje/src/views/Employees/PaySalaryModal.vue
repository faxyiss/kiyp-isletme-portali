<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-md overflow-hidden z-10 max-h-[90vh] flex flex-col"
    >
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-100 flex-shrink-0"
      >
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <WalletIcon :size="18" class="text-emerald-600" />
          Maaş Ödemesi Kaydet
        </h3>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <form
        @submit.prevent="handleSubmit"
        class="p-6 space-y-4 overflow-y-auto custom-scrollbar flex-1"
      >
        <!-- Personel Bilgisi -->
        <div
          v-if="employee"
          class="flex items-center gap-3 bg-slate-50 rounded-xl p-3 border border-slate-100"
        >
          <div
            class="w-10 h-10 rounded-xl bg-linear-to-r from-blue-500 to-indigo-500 text-white flex items-center justify-center font-bold text-sm flex-shrink-0"
          >
            {{ employee.fullName?.charAt(0) }}
          </div>
          <div>
            <p class="font-bold text-slate-800 text-sm">{{ employee.fullName }}</p>
            <p class="text-xs text-slate-400">{{ employee.position }}</p>
          </div>
        </div>

        <!-- Ay / Yıl -->
        <div class="grid grid-cols-2 gap-3">
          <!-- Ay Dropdown -->
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5">Ay</label>
            <div class="relative">
              <button
                type="button"
                :disabled="isLoading"
                @click="isMonthOpen = !isMonthOpen"
                class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:opacity-60"
              >
                <span class="font-medium">{{ months.find(m => m.value === form.month)?.label }}</span>
                <ChevronDownIcon :size="16" class="text-slate-400 transition-transform duration-200" :class="{ 'rotate-180': isMonthOpen }" />
              </button>
              <div v-if="isMonthOpen" class="fixed inset-0 z-40" @click="isMonthOpen = false"></div>
              <div v-if="isMonthOpen" class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5 max-h-52 overflow-y-auto">
                <button
                  v-for="m in months"
                  :key="m.value"
                  type="button"
                  @click="form.month = m.value; isMonthOpen = false"
                  class="w-full text-left px-3 py-2 rounded-lg text-sm transition-colors"
                  :class="form.month === m.value ? 'bg-blue-50 text-blue-600 font-semibold' : 'text-slate-700 hover:bg-slate-50'"
                >
                  {{ m.label }}
                </button>
              </div>
            </div>
          </div>

          <!-- Yıl Dropdown -->
          <div>
            <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5">Yıl</label>
            <div class="relative">
              <button
                type="button"
                :disabled="isLoading"
                @click="isYearOpen = !isYearOpen"
                class="w-full flex items-center justify-between px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white text-slate-600 hover:border-slate-300 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:opacity-60"
              >
                <span class="font-medium">{{ form.year }}</span>
                <ChevronDownIcon :size="16" class="text-slate-400 transition-transform duration-200" :class="{ 'rotate-180': isYearOpen }" />
              </button>
              <div v-if="isYearOpen" class="fixed inset-0 z-40" @click="isYearOpen = false"></div>
              <div v-if="isYearOpen" class="absolute top-full left-0 mt-1 w-full bg-white border border-slate-200 rounded-xl shadow-xl z-50 p-1.5">
                <button
                  v-for="y in years"
                  :key="y"
                  type="button"
                  @click="form.year = y; isYearOpen = false"
                  class="w-full text-left px-3 py-2 rounded-lg text-sm transition-colors"
                  :class="form.year === y ? 'bg-blue-50 text-blue-600 font-semibold' : 'text-slate-700 hover:bg-slate-50'"
                >
                  {{ y }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Brüt Maaş -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5">
            Brüt Maaş (₺)
            <span class="text-slate-400 font-normal normal-case ml-1"
              >— boş bırakırsanız kayıtlı maaş kullanılır</span
            >
          </label>
          <input
            v-model.number="form.grossSalary"
            type="number"
            min="0.01"
            step="0.01"
            :disabled="isLoading"
            :placeholder="employee ? `Kayıtlı: ${formatCurrency(employee.grossSalary)}` : '0,00'"
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60"
          />
        </div>

        <!-- Canlı Hesaplama -->
        <div class="bg-slate-50 rounded-xl border border-slate-100 overflow-hidden">
          <div class="px-4 py-2.5 bg-slate-100/60 border-b border-slate-200">
            <p class="text-xs font-bold text-slate-600 uppercase tracking-wide">Hesaplama Özeti</p>
          </div>
          <div class="p-4 space-y-2">
            <div class="flex justify-between text-xs">
              <span class="text-slate-500">Brüt Maaş</span>
              <span class="font-bold text-slate-800">{{ formatCurrency(effectiveGross) }}</span>
            </div>
            <div class="border-t border-slate-200 pt-2 space-y-1.5">
              <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wide">
                Çalışan Kesintileri
              </p>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">SGK Primi (%14)</span>
                <span class="font-semibold text-red-500"
                  >-{{ formatCurrency(calc.employeeSGK) }}</span
                >
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">İşsizlik Sigortası (%1)</span>
                <span class="font-semibold text-red-500"
                  >-{{ formatCurrency(calc.employeeUnemployment) }}</span
                >
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">Gelir Vergisi (%15)</span>
                <span class="font-semibold text-red-500"
                  >-{{ formatCurrency(calc.incomeTax) }}</span
                >
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">Damga Vergisi (%0.759)</span>
                <span class="font-semibold text-red-500">-{{ formatCurrency(calc.stampTax) }}</span>
              </div>
            </div>
            <div class="border-t border-slate-200 pt-2 flex justify-between">
              <span class="text-xs font-bold text-emerald-700">Net Maaş (Çalışana)</span>
              <span class="text-sm font-bold text-emerald-700">{{
                formatCurrency(calc.netSalary)
              }}</span>
            </div>
            <div class="border-t border-slate-200 pt-2 space-y-1.5">
              <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wide">
                İşveren Ek Maliyeti
              </p>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">SGK Primi (%20.5)</span>
                <span class="font-semibold text-slate-700"
                  >+{{ formatCurrency(calc.employerSGK) }}</span
                >
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-500">İşsizlik Sigortası (%2)</span>
                <span class="font-semibold text-slate-700"
                  >+{{ formatCurrency(calc.employerUnemployment) }}</span
                >
              </div>
            </div>
            <div class="border-t-2 border-slate-300 pt-2 flex justify-between">
              <span class="text-xs font-bold text-red-600">Toplam İşveren Maliyeti</span>
              <span class="text-sm font-bold text-red-600">{{
                formatCurrency(calc.totalEmployerCost)
              }}</span>
            </div>
          </div>
        </div>

        <!-- Notlar -->
        <div>
          <label class="block text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5"
            >Notlar (Opsiyonel)</label
          >
          <textarea
            v-model="form.notes"
            rows="2"
            :disabled="isLoading"
            placeholder="Prim, kesinti gibi ek notlar..."
            class="w-full px-3 py-2 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-sm disabled:opacity-60 resize-none"
          ></textarea>
        </div>
      </form>

      <div
        class="flex items-center justify-end gap-3 px-6 py-4 border-t border-slate-100 flex-shrink-0"
      >
        <button
          type="button"
          @click="emit('close')"
          :disabled="isLoading"
          class="px-4 py-2 rounded-xl text-sm font-medium text-slate-600 hover:bg-slate-50 transition-colors disabled:opacity-50"
        >
          İptal
        </button>
        <button
          @click="handleSubmit"
          :disabled="isLoading"
          class="px-5 py-2.5 rounded-xl text-sm font-medium bg-emerald-600 hover:bg-emerald-700 text-white shadow-sm shadow-emerald-200 transition-colors flex items-center gap-1.5 disabled:opacity-60"
        >
          <CheckIcon v-if="!isLoading" :size="15" />
          {{ isLoading ? 'Kaydediliyor...' : 'Ödemeyi Kaydet' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { WalletIcon, XIcon, CheckIcon, ChevronDownIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showError } = useAlert()

const props = defineProps<{ isOpen: boolean; employee: any }>()
const emit = defineEmits<{ (e: 'close'): void; (e: 'success'): void }>()

const BASE_URL = '/api'
const isLoading = ref(false)
const isMonthOpen = ref(false)
const isYearOpen = ref(false)
const now = new Date()

const defaultForm = () => ({
  month: now.getMonth() + 1,
  year: now.getFullYear(),
  grossSalary: null as number | null,
  notes: '',
})
const form = ref(defaultForm())

const months = [
  { value: 1, label: 'Ocak' },
  { value: 2, label: 'Şubat' },
  { value: 3, label: 'Mart' },
  { value: 4, label: 'Nisan' },
  { value: 5, label: 'Mayıs' },
  { value: 6, label: 'Haziran' },
  { value: 7, label: 'Temmuz' },
  { value: 8, label: 'Ağustos' },
  { value: 9, label: 'Eylül' },
  { value: 10, label: 'Ekim' },
  { value: 11, label: 'Kasım' },
  { value: 12, label: 'Aralık' },
]
const years = Array.from({ length: 5 }, (_, i) => now.getFullYear() - 2 + i)

const RATES = {
  empSGK: 0.14,
  empUnemp: 0.01,
  incomeTax: 0.15,
  stamp: 0.00759,
  erSGK: 0.205,
  erUnemp: 0.02,
}

const effectiveGross = computed(() => form.value.grossSalary || props.employee?.grossSalary || 0)

const calc = computed(() => {
  const g = effectiveGross.value
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
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(v ?? 0)

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
    const body: any = {
      month: form.value.month,
      year: form.value.year,
      notes: form.value.notes || null,
    }
    if (form.value.grossSalary) body.grossSalary = form.value.grossSalary

    const res = await fetch(`${BASE_URL}/Employees/${props.employee.id}/payments`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
    const data = await res.json()
    if (res.ok) {
      emit('success')
      emit('close')
    } else showError(data.message || 'Ödeme kaydedilemedi.')
  } catch {
    showError('Sunucuyla bağlantı kurulamadı.')
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
</style>
