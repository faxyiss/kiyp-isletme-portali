<script setup lang="ts">
import AppNotification from '@/components/AppNotification.vue'
import AppConfirmDialog from '@/components/AppConfirmDialog.vue'
</script>

<template>
  <router-view></router-view>
  <AppNotification />
  <AppConfirmDialog />
</template>

<style>
/* CSS kodlarımızı Tailwind hallettiği için burası boş kalabilir */
</style>
