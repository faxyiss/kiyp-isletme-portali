<template>
  <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 animate-fade-in">
    <TerminalProductList
      class="lg:col-span-2"
      :products-list="productsList"
      :selected-product-id="selectedProduct?.id"
      :current-page="currentPage"
      :total-pages="totalPages"
      @select="handleProductSelect"
      @filter-changed="$emit('filter-changed', $event)"
      @page-changed="$emit('page-changed', $event)"
    />

    <TerminalCart
      class="lg:col-span-1"
      :selected-product="selectedProduct"
      :customers-list="customersList"
      @clear-selection="selectedProduct = null"
      @sale-completed="$emit('saleCompleted')"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import TerminalProductList from './TerminalProductList.vue'
import TerminalCart from './TerminalCart.vue' // Sepet kodun aynı kalacak

interface Product {
  id: string
  productNo: number
  name: string
  unitPrice: number
  remainingQuantity: number
  categoryName?: string
}
interface Customer {
  id: string
  firstName: string
  lastName: string
}

const props = defineProps<{
  productsList: Product[]
  customersList: Customer[]
  currentPage: number
  totalPages: number
}>()
defineEmits(['saleCompleted', 'filter-changed', 'page-changed'])

const selectedProduct = ref<Product | null>(null)

const handleProductSelect = (product: Product) => {
  selectedProduct.value = product
}

watch(
  () => props.productsList,
  (newList) => {
    if (selectedProduct.value) {
      const updatedProduct = newList.find((p) => p.id === selectedProduct.value?.id)
      selectedProduct.value = updatedProduct || null
    }
  },
  { deep: true },
)
</script>
