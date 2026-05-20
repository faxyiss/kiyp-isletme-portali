<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="$emit('close')"></div>
    
    <div class="relative w-full max-w-md bg-white rounded-2xl shadow-2xl border border-slate-100 overflow-hidden animate-fade-in-up">
      <div class="p-6 border-b border-slate-100 flex items-center justify-between bg-emerald-50/50">
        <div class="flex items-center gap-3">
          <div class="bg-emerald-100 text-emerald-600 p-2 rounded-xl">
            <WalletIcon :size="20" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-slate-800">Tahsilat Al (Borç Düş)</h3>
            <p class="text-xs text-slate-500" v-if="customer">{{ customer.firstName }} {{ customer.lastName }}</p>
          </div>
        </div>
        <button @click="$emit('close')" class="text-slate-400 hover:text-slate-600 bg-white hover:bg-slate-50 p-2 rounded-lg transition-colors cursor-pointer border border-slate-100">
          <XIcon :size="20" />
        </button>
      </div>
      
      <div class="p-6 space-y-5">
        <div class="bg-slate-50 rounded-xl p-4 flex items-center justify-between border border-slate-100">
          <span class="text-sm font-semibold text-slate-600">Toplam Borç:</span>
          <span class="text-lg font-bold" :class="customer?.currentBalance > 0 ? 'text-red-600' : 'text-emerald-600'">
            {{ formatCurrency(customer?.currentBalance || 0) }}
          </span>
        </div>

        <div class="space-y-1.5">
          <label class="text-sm font-semibold text-slate-700">Tahsil Edilen Tutar (₺)</label>
          <div class="relative">
            <span class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 font-bold">₺</span>
            <input v-model.number="paymentForm.amount" type="number" :max="customer?.currentBalance" placeholder="0.00" class="w-full pl-9 pr-4 py-3 rounded-xl border border-slate-200 font-semibold text-slate-800 focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-50 transition-all" />
          </div>
          <p class="text-[11px] text-slate-400 mt-1">Girilen tutar müşterinin toplam borcundan düşülecektir.</p>
        </div>

        <div class="space-y-1.5">
          <label class="text-sm font-semibold text-slate-700">Açıklama (Opsiyonel)</label>
          <textarea v-model="paymentForm.description" rows="2" placeholder="Örn: Nakit elden alındı, havale geldi vb." class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-50 transition-all resize-none"></textarea>
        </div>
      </div>

      <div class="p-6 bg-slate-50 border-t border-slate-100 flex justify-end gap-3">
        <button @click="$emit('close')" type="button" class="px-5 py-2.5 rounded-xl font-medium text-sm text-slate-600 bg-white border border-slate-200 hover:bg-slate-50 transition-colors cursor-pointer">
          İptal
        </button>
        <button @click="submitPayment" type="button" class="px-5 py-2.5 rounded-xl font-medium text-sm text-white bg-emerald-600 hover:bg-emerald-700 shadow-md shadow-emerald-200 transition-all cursor-pointer">
          Tahsilatı Kaydet
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { XIcon, WalletIcon } from 'lucide-vue-next';
import { useAlert } from '@/composables/useAlert'

const { showWarning } = useAlert()

const props = defineProps<{ isOpen: boolean; customer: any; }>();
const emit = defineEmits(['close', 'save']);

const paymentForm = ref({
  amount: null as number | null,
  description: ''
});

const submitPayment = () => {
  if (!paymentForm.value.amount || paymentForm.value.amount <= 0) {
    showWarning("Lütfen geçerli bir tutar girin.");
    return;
  }
  
  if (paymentForm.value.amount > props.customer.currentBalance) {
    showWarning("Tahsilat tutarı toplam borçtan büyük olamaz!");
    return;
  }

  emit('save', { 
    customerId: props.customer.id, 
    payload: { ...paymentForm.value } 
  });

  // Formu temizle
  paymentForm.value = { amount: null, description: '' };
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
};
</script>

<style scoped>
.animate-fade-in-up { animation: fadeInUp 0.3s ease-out forwards; }
@keyframes fadeInUp { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>