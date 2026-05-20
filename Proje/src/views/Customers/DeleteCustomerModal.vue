<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="$emit('close')"></div>
    
    <div class="relative w-full max-w-sm bg-white rounded-2xl shadow-2xl border border-slate-100 overflow-hidden animate-fade-in-up p-6 text-center">
      
      <div class="w-16 h-16 bg-red-50 text-red-500 rounded-full flex items-center justify-center mx-auto mb-4 border border-red-100">
        <AlertTriangleIcon :size="32" />
      </div>
      
      <h3 class="text-xl font-bold text-slate-800 mb-2">Müşteriyi Sil</h3>
      <p class="text-sm text-slate-500 mb-6">
        Bu müşteriyi silmek istediğinize emin misiniz? Bu işlem geri alınamaz.
      </p>
      
      <div class="flex gap-3">
        <button @click="$emit('close')" type="button" class="flex-1 px-4 py-2.5 rounded-xl font-medium text-sm text-slate-600 bg-white border border-slate-200 hover:bg-slate-50 transition-colors cursor-pointer">
          İptal
        </button>
        <button @click="$emit('confirm')" type="button" class="flex-1 px-4 py-2.5 rounded-xl font-medium text-sm text-white bg-red-600 hover:bg-red-700 shadow-md shadow-red-200 transition-all cursor-pointer">
          Evet, Sil
        </button>
      </div>
      
    </div>
  </div>
</template>

<script setup lang="ts">
import { AlertTriangleIcon } from 'lucide-vue-next';

defineProps<{ isOpen: boolean; }>();
defineEmits(['close', 'confirm']);
</script>

<style scoped>
.animate-fade-in-up { animation: fadeInUp 0.3s ease-out forwards; }
@keyframes fadeInUp { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>