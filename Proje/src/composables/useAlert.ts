import { reactive } from 'vue'

type AlertType = 'success' | 'error' | 'warning'

const notification = reactive({
  visible: false,
  type: 'success' as AlertType,
  message: '',
})

const confirmState = reactive({
  visible: false,
  message: '',
  resolve: null as ((val: boolean) => void) | null,
})

export function useAlert() {
  const showSuccess = (message: string) => {
    notification.visible = true
    notification.type = 'success'
    notification.message = message
  }

  const showError = (message: string) => {
    notification.visible = true
    notification.type = 'error'
    notification.message = message
  }

  const showWarning = (message: string) => {
    notification.visible = true
    notification.type = 'warning'
    notification.message = message
  }

  const showConfirm = (message: string): Promise<boolean> => {
    confirmState.visible = true
    confirmState.message = message
    return new Promise<boolean>((resolve) => {
      confirmState.resolve = resolve
    })
  }

  const _dismissNotification = () => {
    notification.visible = false
  }

  const _resolveConfirm = (val: boolean) => {
    confirmState.visible = false
    confirmState.resolve?.(val)
  }

  return {
    showSuccess,
    showError,
    showWarning,
    showConfirm,
    notification,
    confirmState,
    _dismissNotification,
    _resolveConfirm,
  }
}
