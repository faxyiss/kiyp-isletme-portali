<template>
  <Teleport to="body">
    <Transition name="confirm">
      <div
        v-if="confirmState.visible"
        class="fixed inset-0 z-[9999] flex items-center justify-center bg-gray-950/40 backdrop-blur-sm p-4"
      >
        <div class="relative w-full max-w-sm bg-white rounded-2xl border border-gray-100 shadow-2xl p-6 text-center">
          <div class="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <AlertCircleIcon :size="32" class="text-amber-600" />
          </div>

          <h3 class="text-lg font-bold text-gray-900 mb-2">Emin misiniz?</h3>
          <p class="text-sm text-gray-600 leading-relaxed">{{ confirmState.message }}</p>

          <div class="flex gap-3 mt-6">
            <button
              @click="_resolveConfirm(false)"
              class="flex-1 py-2.5 rounded-xl text-sm font-bold text-gray-600 bg-gray-100 hover:bg-gray-200 transition-colors"
            >
              Vazgeç
            </button>
            <button
              @click="_resolveConfirm(true)"
              class="flex-1 py-2.5 rounded-xl text-sm font-bold text-white bg-red-600 hover:bg-red-700 transition-colors"
            >
              Evet, Devam Et
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { AlertCircleIcon } from 'lucide-vue-next'
import { useAlert } from '@/composables/useAlert'

const { confirmState, _resolveConfirm } = useAlert()
</script>

<style scoped>
.confirm-enter-active,
.confirm-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.confirm-enter-from,
.confirm-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>
