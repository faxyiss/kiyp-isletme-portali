export enum ExpenseType {
  OneTime = 0,
  Monthly = 1,
  Periodic = 2,
}

export interface ExpenseCategory {
  id: string
  name: string
}

export interface ExpenseResponseDto {
  id: string
  title: string
  categoryName: string
  amount: number
  expenseType: ExpenseType
  expenseTypeLabel: string
  startDate: string
  endDate?: string | null
  recurringDay?: number | null
  notes?: string | null
  createdAt: string
}

export interface ExpenseQueryDto {
  page: number
  pageSize: number
  month?: number | null
  year?: number | null
  expenseType?: ExpenseType | null
  categoryId?: string | null
}

export interface ExpensePagedResponseDto {
  items: ExpenseResponseDto[]
  totalCount: number
  totalPages: number
  currentPage: number
  totalAmount: number
}

export interface ExpenseCreateDto {
  categoryId: string
  title: string
  amount: number
  expenseType: ExpenseType
  startDate: string
  endDate?: string | null
  recurringDay?: number | null
  notes?: string | null
}
