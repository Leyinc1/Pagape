import axios from 'axios';

// region: Interfaces de Datos

interface RegisterData {
  nombre: string;
  email: string;
  password: string;
}

interface LoginData {
  email: string;
  password: string;
}

export interface Event {
  id: number;
  nombre: string;
  fechaCreacion: string;
  creadorId: number;
}

export interface CreateEventData {
  nombre: string;
}

export interface Expense {
    id: number;
    descripcion: string;
    monto: number;
    fecha: string;
    pagadoPorUserId: number;
    pagadoPorNombre: string;
    splits: { deudorNombre: string; montoAdeudado: number }[];
}

export interface ExpenseSplitInput {
    userId: number;
    montoAdeudado: number;
}

export interface CreateExpenseData {
    descripcion: string;
    monto: number;
    pagadoPorUserId: number;
    splits: ExpenseSplitInput[];
}

export interface BalanceTransaction {
    deudorId: number;
    deudorNombre: string;
    acreedorId: number;
    acreedorNombre: string;
    monto: number;
}

export interface Balance {
    transaccionesSugeridas: BalanceTransaction[];
}

export interface Payment {
    id: number;
    monto: number;
    fecha: string;
    pagadorId: number;
    pagadorNombre: string;
    receptorId: number;
    receptorNombre: string;
}

export interface CreatePaymentData {
    monto: number;
    aQuienUserId: number;
    deQuienUserId: number; // Added deQuienUserId to allow specifying the payer
}

export interface Participant {
    userId: number;
    nombre: string;
    email: string;
}

export interface AddParticipantData {
    email: string;
}

export interface UserProfile {
    id: number;
    nombre: string;
    email: string;
}

export interface UpdatePasswordData {
    oldPassword: string;
    newPassword: string;
}

// endregion

const apiClient = axios.create({
  baseURL: 'http://localhost:5139/api', // Asegúrate de que el puerto sea el correcto
  headers: {
    'Content-Type': 'application/json'
  }
});

// Interceptor para añadir el token de autenticación a cada solicitud
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// region: API Calls

// Auth
export const registerUser = (userData: RegisterData) => apiClient.post('/auth/register', userData);
export const loginUser = (credentials: LoginData) => apiClient.post('/auth/login', credentials);

// User Profile
export const getUserProfile = () => apiClient.get<UserProfile>('/user/profile');
export const updatePassword = (data: UpdatePasswordData) => apiClient.put('/user/password', data);

// Eventos
export const getEvents = () => apiClient.get<Event[]>('/eventos');
export const createEvent = (eventData: CreateEventData) => apiClient.post<Event>('/eventos', eventData);
export const getEventById = (id: string) => apiClient.get<Event>(`/eventos/${id}`);

// Participantes (dentro de un evento)
export const getParticipants = (eventId: string) => apiClient.get<Participant[]>(`/eventos/${eventId}/participantes`);
export const addParticipant = (eventId: string, data: AddParticipantData) => apiClient.post<Participant>(`/eventos/${eventId}/participantes`, data);
export const removeParticipant = (eventId: string, participantUserId: number) => apiClient.delete(`/eventos/${eventId}/participantes/${participantUserId}`);

// Gastos (dentro de un evento)
export const getExpensesForEvent = (eventId: string) => apiClient.get<Expense[]>(`/eventos/${eventId}/gastos`);
export const createExpense = (eventId: string, data: CreateExpenseData) => apiClient.post<Expense>(`/eventos/${eventId}/gastos`, data);
export const updateExpense = (eventId: string, expenseId: number, data: CreateExpenseData) => apiClient.put<Expense>(`/eventos/${eventId}/gastos/${expenseId}`, data);
export const deleteExpense = (eventId: string, expenseId: number) => apiClient.delete(`/eventos/${eventId}/gastos/${expenseId}`);


// Balance (dentro de un evento)
export const getBalanceForEvent = (eventId: string) => apiClient.get<Balance>(`/eventos/${eventId}/balance`);

// Pagos (dentro de un evento)
export const getPaymentsForEvent = (eventId: string) => apiClient.get<Payment[]>(`/eventos/${eventId}/pagos`);
export const createPayment = (eventId: string, data: CreatePaymentData) => apiClient.post(`/eventos/${eventId}/pagos`, data);
export const updatePayment = (eventId: string, paymentId: number, data: CreatePaymentData) => apiClient.put(`/eventos/${eventId}/pagos/${paymentId}`, data);
export const deletePayment = (eventId: string, paymentId: number) => apiClient.delete(`/eventos/${eventId}/pagos/${paymentId}`);

// endregion

