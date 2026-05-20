<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm" @click="emit('close')"></div>
    <div
      class="bg-white rounded-2xl shadow-xl border border-slate-100 w-full max-w-2xl overflow-hidden z-10 max-h-[90vh] flex flex-col"
    >
      <!-- Header -->
      <div
        class="flex items-center justify-between px-6 py-4 border-b border-slate-100 flex-shrink-0"
      >
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 rounded-xl bg-linear-to-r from-blue-500 to-indigo-500 text-white flex items-center justify-center font-bold flex-shrink-0"
          >
            {{ employee?.fullName?.charAt(0) }}
          </div>
          <div>
            <h3 class="text-base font-bold text-slate-800">{{ employee?.fullName }}</h3>
            <p class="text-xs text-slate-400">{{ employee?.position }}</p>
          </div>
        </div>
        <button
          @click="emit('close')"
          class="p-1.5 rounded-lg text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <XIcon :size="18" />
        </button>
      </div>

      <!-- Tabs -->
      <div class="flex border-b border-slate-100 px-6 flex-shrink-0">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          class="py-3 px-4 text-xs font-bold border-b-2 transition-colors -mb-px"
          :class="
            activeTab === tab.key
              ? 'border-blue-600 text-blue-600'
              : 'border-transparent text-slate-400 hover:text-slate-600'
          "
        >
          {{ tab.label }}
        </button>
      </div>

      <div class="flex-1 overflow-y-auto custom-scrollbar">
        <!-- Özet Tab -->
        <div v-if="activeTab === 'summary'" class="p-6 space-y-4">
          <div class="grid grid-cols-2 gap-3">
            <div class="bg-slate-50 rounded-xl p-4 border border-slate-100">
              <p class="text-xs text-slate-400 font-medium">Brüt Maaş</p>
              <p class="text-lg font-bold text-slate-800 mt-1">
                {{ formatCurrency(employee?.grossSalary) }}
              </p>
            </div>
            <div class="bg-emerald-50 rounded-xl p-4 border border-emerald-100">
              <p class="text-xs text-emerald-600 font-medium">Net Maaş</p>
              <p class="text-lg font-bold text-emerald-700 mt-1">
                {{ formatCurrency(employee?.netSalary) }}
              </p>
            </div>
            <div class="bg-red-50 rounded-xl p-4 border border-red-100">
              <p class="text-xs text-red-500 font-medium">İşveren Maliyeti</p>
              <p class="text-lg font-bold text-red-600 mt-1">
                {{ formatCurrency(employee?.totalEmployerCost) }}
              </p>
            </div>
            <div class="bg-amber-50 rounded-xl p-4 border border-amber-100">
              <p class="text-xs text-amber-600 font-medium">Toplam İzin Günü</p>
              <p class="text-lg font-bold text-amber-700 mt-1">{{ totalLeaveDays }} Gün</p>
            </div>
          </div>

          <!-- Vergi / SGK Detayı -->
          <div class="bg-slate-50 rounded-xl border border-slate-100 overflow-hidden">
            <div class="px-4 py-2.5 bg-slate-100/60 border-b border-slate-200">
              <p class="text-xs font-bold text-slate-600 uppercase tracking-wide">
                Aylık Vergi & SGK Dökümü
              </p>
            </div>
            <div class="p-4 space-y-2">
              <div v-for="row in taxRows" :key="row.label" class="flex justify-between text-xs">
                <span class="text-slate-500">{{ row.label }}</span>
                <span class="font-semibold" :class="row.color">{{
                  formatCurrency(row.value)
                }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Maaş Geçmişi Tab -->
        <div v-if="activeTab === 'payments'" class="p-6">
          <div v-if="isLoadingPayments" class="py-8 text-center text-slate-400 text-sm">
            Yükleniyor...
          </div>
          <div v-else-if="payments.length === 0" class="py-12 text-center">
            <div class="flex flex-col items-center gap-2 text-slate-400">
              <WalletIcon :size="28" class="text-slate-300" />
              <p class="text-sm font-medium text-slate-500">Henüz maaş ödemesi yok</p>
            </div>
          </div>
          <div v-else class="space-y-2">
            <div
              v-for="p in payments"
              :key="p.id"
              class="flex items-center justify-between p-4 bg-slate-50 rounded-xl border border-slate-100 hover:border-slate-200 transition-colors"
            >
              <div>
                <p class="font-bold text-slate-800 text-sm">{{ p.monthLabel }}</p>
                <div class="flex items-center gap-3 mt-1">
                  <span class="text-xs text-slate-500"
                    >Net:
                    <span class="font-semibold text-emerald-600">{{
                      formatCurrency(p.netSalary)
                    }}</span></span
                  >
                  <span class="text-xs text-slate-400">|</span>
                  <span class="text-xs text-slate-500"
                    >İşv. Maliyet:
                    <span class="font-semibold text-red-500">{{
                      formatCurrency(p.totalEmployerCost)
                    }}</span></span
                  >
                </div>
                <p v-if="p.notes" class="text-xs text-slate-400 mt-0.5">{{ p.notes }}</p>
              </div>
              <button
                @click="deletePayment(p)"
                class="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors flex-shrink-0"
              >
                <Trash2Icon :size="14" />
              </button>
            </div>
          </div>
        </div>

        <!-- İzin Geçmişi Tab -->
        <div v-if="activeTab === 'leaves'" class="p-6">
          <div v-if="isLoadingLeaves" class="py-8 text-center text-slate-400 text-sm">
            Yükleniyor...
          </div>
          <div v-else-if="leaves.length === 0" class="py-12 text-center">
            <div class="flex flex-col items-center gap-2 text-slate-400">
              <CalendarIcon :size="28" class="text-slate-300" />
              <p class="text-sm font-medium text-slate-500">Henüz izin kaydı yok</p>
            </div>
          </div>
          <div v-else class="space-y-2">
            <div
              v-for="l in leaves"
              :key="l.id"
              class="flex items-center justify-between p-4 bg-slate-50 rounded-xl border border-slate-100 hover:border-slate-200 transition-colors"
            >
              <div>
                <div class="flex items-center gap-2">
                  <span class="font-bold text-slate-800 text-sm"
                    >{{ formatDate(l.startDate) }} — {{ formatDate(l.endDate) }}</span
                  >
                  <span
                    class="px-2 py-0.5 rounded-md bg-amber-50 border border-amber-100 text-amber-700 text-xs font-bold"
                    >{{ l.dayCount }} gün</span
                  >
                </div>
                <p v-if="l.reason" class="text-xs text-slate-400 mt-0.5">{{ l.reason }}</p>
              </div>
              <button
                @click="deleteLeave(l)"
                class="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors flex-shrink-0"
              >
                <Trash2Icon :size="14" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { XIcon, WalletIcon, CalendarIcon, Trash2Icon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { showConfirm } = useAlert()

const props = defineProps<{ isOpen: boolean; employee: any }>()
const emit = defineEmits<{ (e: 'close'): void }>()

const activeTab = ref('summary')
const tabs = [
  { key: 'summary', label: 'Özet & Vergi' },
  { key: 'payments', label: 'Maaş Geçmişi' },
  { key: 'leaves', label: 'İzin Geçmişi' },
]

const payments = ref<any[]>([])
const leaves = ref<any[]>([])
const isLoadingPayments = ref(false)
const isLoadingLeaves = ref(false)

const RATES = {
  empSGK: 0.14,
  empUnemp: 0.01,
  incomeTax: 0.15,
  stamp: 0.00759,
  erSGK: 0.205,
  erUnemp: 0.02,
}

const taxRows = computed(() => {
  const g = props.employee?.grossSalary ?? 0
  const empSGK = Math.round(g * RATES.empSGK * 100) / 100
  const empUnemp = Math.round(g * RATES.empUnemp * 100) / 100
  const incomeTax = Math.round((g - empSGK - empUnemp) * RATES.incomeTax * 100) / 100
  const stamp = Math.round(g * RATES.stamp * 100) / 100
  const erSGK = Math.round(g * RATES.erSGK * 100) / 100
  const erUnemp = Math.round(g * RATES.erUnemp * 100) / 100
  return [
    { label: 'SGK Primi (Çalışan %14)', value: empSGK, color: 'text-red-500' },
    { label: 'İşsizlik (Çalışan %1)', value: empUnemp, color: 'text-red-500' },
    { label: 'Gelir Vergisi (%15)', value: incomeTax, color: 'text-red-500' },
    { label: 'Damga Vergisi (%0.759)', value: stamp, color: 'text-red-500' },
    { label: 'SGK Primi (İşveren %20.5)', value: erSGK, color: 'text-slate-700' },
    { label: 'İşsizlik (İşveren %2)', value: erUnemp, color: 'text-slate-700' },
  ]
})

const totalLeaveDays = computed(() => leaves.value.reduce((s, l) => s + l.dayCount, 0))

const formatCurrency = (v: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(v ?? 0)

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' })

const fetchPayments = async () => {
  if (!props.employee) return
  isLoadingPayments.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`/api/Employees/${props.employee.id}/payments`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) payments.value = await res.json()
  } finally {
    isLoadingPayments.value = false
  }
}

const fetchLeaves = async () => {
  if (!props.employee) return
  isLoadingLeaves.value = true
  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`/api/Employees/${props.employee.id}/leaves`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (res.ok) leaves.value = await res.json()
  } finally {
    isLoadingLeaves.value = false
  }
}

const deletePayment = async (p: any) => {
  if (!(await showConfirm(`${p.monthLabel} ödemesini silmek istiyor musunuz?`))) return
  const token = localStorage.getItem('token')
  const res = await fetch(`/api/Employees/payments/${p.id}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  })
  if (res.ok) fetchPayments()
}

const deleteLeave = async (l: any) => {
  if (!(await showConfirm('Bu izin kaydını silmek istiyor musunuz?'))) return
  const token = localStorage.getItem('token')
  const res = await fetch(`/api/Employees/leaves/${l.id}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  })
  if (res.ok) fetchLeaves()
}

watch(
  () => props.isOpen,
  (v) => {
    if (v) {
      activeTab.value = 'summary'
      fetchPayments()
      fetchLeaves()
    } else {
      payments.value = []
      leaves.value = []
    }
  },
)
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
