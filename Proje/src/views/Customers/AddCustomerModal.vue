<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="$emit('close')"></div>
    
    <div class="relative w-full max-w-md bg-white rounded-2xl shadow-2xl border border-slate-100 overflow-hidden animate-fade-in-up">
      <div class="p-6 border-b border-slate-100 flex items-center justify-between">
        <h3 class="text-lg font-bold text-slate-800">Yeni Müşteri Ekle</h3>
        <button @click="$emit('close')" class="text-slate-400 hover:text-slate-600 bg-slate-50 hover:bg-slate-100 p-2 rounded-lg transition-colors cursor-pointer">
          <XIcon :size="20" />
        </button>
      </div>
      
      <div class="p-6 space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Ad</label>
            <input v-model="form.firstName" type="text" placeholder="Müşteri Adı" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-50 transition-all" />
          </div>
          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Soyad</label>
            <input v-model="form.lastName" type="text" placeholder="Müşteri Soyadı" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-50 transition-all" />
          </div>
        </div>

        <div class="space-y-1.5">
          <label class="text-sm font-semibold text-slate-700">Telefon Numarası</label>
          <input v-model="form.phoneNumber" type="text" placeholder="0555 555 55 55" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-50 transition-all" />
        </div>

        <div class="space-y-1.5">
          <label class="text-sm font-semibold text-slate-700">Başlangıç Borcu (₺)</label>
          <div class="relative">
            <span class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 font-bold">₺</span>
            <input v-model.number="form.initialDebt" type="number" min="0" class="w-full pl-9 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-50 transition-all" />
          </div>
          <p class="text-xs text-slate-500 mt-1">Eğer müşterinin size önceden kalan bir borcu varsa buraya girebilirsiniz.</p>
        </div>
      </div>

      <div class="p-6 bg-slate-50 border-t border-slate-100 flex justify-end gap-3">
        <button @click="$emit('close')" type="button" class="px-5 py-2.5 rounded-xl font-medium text-sm text-slate-600 bg-white border border-slate-200 hover:bg-slate-50 transition-colors cursor-pointer">
          İptal
        </button>
        <button @click="submitForm" type="button" class="px-5 py-2.5 rounded-xl font-medium text-sm text-white bg-blue-600 hover:bg-blue-700 shadow-md shadow-blue-200 transition-all cursor-pointer">
          Müşteriyi Kaydet
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { XIcon } from 'lucide-vue-next';
import { useAlert } from '@/composables/useAlert'

const { showWarning } = useAlert()

defineProps<{ isOpen: boolean; }>();
const emit = defineEmits(['close', 'save']);

// Backend'deki CustomerCreateDto ile birebir aynı model
const form = ref({
  firstName: '',
  lastName: '',
  phoneNumber: '',
  initialDebt: 0
});

const submitForm = () => {
  if (!form.value.firstName || !form.value.lastName) {
    showWarning("Ad ve Soyad zorunludur!");
    return;
  }
  emit('save', { ...form.value });
  
  // Formu temizle
  form.value = { firstName: '', lastName: '', phoneNumber: '', initialDebt: 0 };
};
</script>

<style scoped>
.animate-fade-in-up { animation: fadeInUp 0.3s ease-out forwards; }
@keyframes fadeInUp { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>