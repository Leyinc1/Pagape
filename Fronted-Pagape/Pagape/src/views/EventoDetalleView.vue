<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import * as api from '@/services/api';
import type { AxiosError } from 'axios';

// --- STATE ---
const route = useRoute();
const router = useRouter();
const eventId = route.params.id as string;

const evento = ref<api.Event | null>(null);
const gastos = ref<api.Expense[]>([]);
const pagos = ref<api.Payment[]>([]);
const balance = ref<api.Balance | null>(null);
const participantes = ref<api.Participant[]>([]);

const isLoading = ref(true);
const errorMessage = ref('');
const activeTab = ref('resumen');

// --- LOAD DATA ---
const loadAllData = async () => {
  try {
    isLoading.value = true;
    errorMessage.value = '';
    const [eventoRes, gastosRes, pagosRes, balanceRes, participantsRes] = await Promise.all([
      api.getEventById(eventId),
      api.getExpensesForEvent(eventId),
      api.getPaymentsForEvent(eventId),
      api.getBalanceForEvent(eventId),
      api.getParticipants(eventId)
    ]);
    evento.value = eventoRes.data;
    gastos.value = gastosRes.data;
    pagos.value = pagosRes.data;
    balance.value = balanceRes.data;
    participantes.value = participantsRes.data;

    console.log('Datos de Evento:', eventoRes.data);
    console.log('Datos de Gastos:', gastosRes.data);
    console.log('Datos de Pagos:', pagosRes.data);
    console.log('Datos de Balance:', balanceRes.data);
    console.log('Datos de Participantes:', participantsRes.data);
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Error al cargar los datos del evento.';
  } finally {
    isLoading.value = false;
  }
};
onMounted(loadAllData);

// --- PAYMENTS CRUD ---
const isPaymentModalOpen = ref(false);
const editingPayment = ref<api.Payment | null>(null);
const paymentData = ref<api.CreatePaymentData>({ monto: 0, aQuienUserId: 0, deQuienUserId: 0 }); // Added deQuienUserId
const openPaymentCreateModal = () => { editingPayment.value = null; paymentData.value = { monto: 0, aQuienUserId: 0, deQuienUserId: 0 }; isPaymentModalOpen.value = true; }; // Initialize deQuienUserId
const openPaymentEditModal = (pago: api.Payment) => { editingPayment.value = pago; paymentData.value = { monto: pago.monto, aQuienUserId: pago.receptorId, deQuienUserId: pago.pagadorId }; isPaymentModalOpen.value = true; }; // Set deQuienUserId from pago.pagadorId
const closePaymentModal = () => isPaymentModalOpen.value = false;
const handleSavePayment = async () => {
  if (paymentData.value.deQuienUserId === 0 || paymentData.value.aQuienUserId === 0 || paymentData.value.monto <= 0) return alert('Completa todos los campos.');
  try {
    if (editingPayment.value) await api.updatePayment(eventId, editingPayment.value.id, paymentData.value);
    else await api.createPayment(eventId, paymentData.value);
    closePaymentModal();
    await loadAllData();
  } catch (err) { alert((err as AxiosError<{ message: string }>).response?.data?.message); }
};
const handleDeletePayment = async (pagoId: number) => {
  if (!confirm('¿Seguro que quieres eliminar este pago?')) return;
  try {
    await api.deletePayment(eventId, pagoId);
    await loadAllData();
  } catch (err) { alert((err as AxiosError<{ message: string }>).response?.data?.message); }
};

// --- EXPENSES CRUD ---
const isExpenseModalOpen = ref(false);
const editingExpense = ref<api.Expense | null>(null);
const expenseData = ref<api.CreateExpenseData>({ descripcion: '', monto: 0, pagadoPorUserId: 0, splits: [] });

// Computed property for expense split validation
const totalSplitAmount = computed(() => {
  return expenseData.value.splits.reduce((sum, split) => sum + (split.montoAdeudado || 0), 0);
});
const isSplitAmountValid = computed(() => {
  return Math.abs(totalSplitAmount.value - expenseData.value.monto) < 0.01; // Tolerance for float comparison
});

const openExpenseCreateModal = () => {
  editingExpense.value = null;
  expenseData.value = {
    descripcion: '',
    monto: 0,
    pagadoPorUserId: 0,
    splits: participantes.value.map(p => ({ userId: p.userId, montoAdeudado: 0 })) // Initialize with all participants owing 0
  };
  isExpenseModalOpen.value = true;
};

const openExpenseEditModal = (gasto: api.Expense) => {
  editingExpense.value = gasto;
  expenseData.value = {
    descripcion: gasto.descripcion,
    monto: gasto.monto,
    pagadoPorUserId: gasto.pagadoPorUserId,
    // Map existing splits, and add other participants with 0 owed
    splits: participantes.value.map(p => {
      const existingSplit = gasto.splits.find(s => s.deudorNombre === p.nombre); // Assuming deudorNombre matches p.nombre
      return { userId: p.userId, montoAdeudado: existingSplit ? existingSplit.montoAdeudado : 0 };
    })
  };
  isExpenseModalOpen.value = true;
};

const closeExpenseModal = () => isExpenseModalOpen.value = false;

const handleSaveExpense = async () => {
  if (!expenseData.value.descripcion || expenseData.value.monto <= 0 || expenseData.value.pagadoPorUserId === 0) {
    alert('Por favor, completa la descripción, el monto total y quién pagó.');
    return;
  }

  // Filter out participants who owe 0 or are not selected (if we add checkboxes later)
  const finalSplits = expenseData.value.splits.filter(s => s.montoAdeudado > 0);
  if (finalSplits.length === 0) {
    alert('Debes asignar al menos un monto a un participante.');
    return;
  }

  if (!isSplitAmountValid.value) {
    alert('La suma de los montos divididos no coincide con el monto total del gasto.');
    return;
  }

  try {
    const dataToSend = { ...expenseData.value, splits: finalSplits };
    if (editingExpense.value) {
      await api.updateExpense(eventId, editingExpense.value.id, dataToSend);
    } else {
      await api.createExpense(eventId, dataToSend);
    }
    closeExpenseModal();
    await loadAllData();
  } catch (err) {
    alert((err as AxiosError<{ message: string }>).response?.data?.message || 'Error al guardar el gasto.');
  }
};

const handleDeleteExpense = async (gastoId: number) => {
  if (!confirm('¿Seguro que quieres eliminar este gasto?')) return;
  try {
    await api.deleteExpense(eventId, gastoId);
    await loadAllData();
  } catch (err) { alert((err as AxiosError<{ message: string }>).response?.data?.message); }
};

// --- PARTICIPANTS CRUD ---
const newParticipantEmail = ref('');
const handleAddParticipant = async () => {
    if (!newParticipantEmail.value) return;
    try {
        await api.addParticipant(eventId, { email: newParticipantEmail.value });
        newParticipantEmail.value = '';
        await loadAllData();
    } catch (err) { alert((err as AxiosError<{ message: string }>).response?.data?.message); }
};
const handleRemoveParticipant = async (participantUserId: number) => {
    if (!confirm('¿Seguro que quieres eliminar a este participante? No se podrá si tiene gastos o pagos asociados.')) return;
    try {
        await api.removeParticipant(eventId, participantUserId);
        await loadAllData();
    } catch (err) { alert((err as AxiosError<{ message: string }>).response?.data?.message); }
};

const goBack = () => router.push({ name: 'Dashboard' });

</script>

<template>
  <div class="container">
    <a @click="goBack" class="back-link">&larr; Volver a Mis Eventos</a>
    <div v-if="isLoading" class="text-center">Cargando datos del evento...</div>
    <div v-else-if="errorMessage" class="error-message">{{ errorMessage }}</div>
    <div v-else-if="evento">
      <h1 class="event-title">{{ evento.nombre }}</h1>

      <div class="tabs">
        <button @click="activeTab = 'resumen'" :class="{ 'active': activeTab === 'resumen' }">Resumen</button>
        <button @click="activeTab = 'gastos'" :class="{ 'active': activeTab === 'gastos' }">Gastos</button>
        <button @click="activeTab = 'pagos'" :class="{ 'active': activeTab === 'pagos' }">Pagos</button>
        <button @click="activeTab = 'participantes'" :class="{ 'active': activeTab === 'participantes' }">Participantes</button>
      </div>

      <!-- Resumen Tab -->
      <div v-if="activeTab === 'resumen'" class="tab-content">
        <h2>Balance del Evento</h2>
        <div v-if="balance && balance.transaccionesSugeridas.length > 0" class="card">
            <div v-for="t in balance.transaccionesSugeridas" :key="`${t.deudorId}-${t.acreedorId}`" class="balance-item">
                <span>{{ t.deudorNombre }}</span> <span class="arrow"> &rarr; </span> <span>{{ t.acreedorNombre }}</span> <span class="monto">{{ t.monto.toFixed(2) }} €</span>
            </div>
        </div>
        <div v-else class="card text-center"><p>¡Todo saldado!</p></div>
      </div>

      <!-- Gastos Tab -->
      <div v-if="activeTab === 'gastos'" class="tab-content">
        <div class="header-with-button">
            <h2>Gastos</h2>
            <button class="btn btn-primary" @click="openExpenseCreateModal">+ Añadir Gasto</button>
        </div>
        <div v-if="gastos.length > 0">
            <div v-for="gasto in gastos" :key="gasto.id" class="list-item">
                <div><span>{{ gasto.descripcion }}</span><small>Pagado por: {{ gasto.pagadoPorNombre }}</small><span class="monto">{{ gasto.monto.toFixed(2) }} €</span></div>
                <div><button class="btn btn-secondary btn-sm" @click="openExpenseEditModal(gasto)">Editar</button><button class="btn btn-danger btn-sm" @click="handleDeleteExpense(gasto.id)">Eliminar</button></div>
            </div>
        </div>
        <div v-else class="card text-center"><p>No hay gastos registrados.</p></div>
      </div>

      <!-- Pagos Tab -->
      <div v-if="activeTab === 'pagos'" class="tab-content">
        <div class="header-with-button">
            <h2>Pagos Realizados</h2>
            <button class="btn btn-primary" @click="openPaymentCreateModal">+ Registrar Pago</button>
        </div>
        <div v-if="pagos.length > 0">
            <div v-for="pago in pagos" :key="pago.id" class="list-item">
                <div><span>{{ pago.pagadorNombre }} &rarr; {{ pago.receptorNombre }}</span><span class="monto">{{ pago.monto.toFixed(2) }} €</span></div>
                <div><button class="btn btn-secondary btn-sm" @click="openPaymentEditModal(pago)">Editar</button><button class="btn btn-danger btn-sm" @click="handleDeletePayment(pago.id)">Eliminar</button></div>
            </div>
        </div>
        <div v-else class="card text-center"><p>No hay pagos registrados.</p></div>
      </div>

      <!-- Participantes Tab -->
      <div v-if="activeTab === 'participantes'" class="tab-content">
        <h2>Participantes</h2>
        <form @submit.prevent="handleAddParticipant" class="form-add-participant">
            <input type="email" v-model="newParticipantEmail" placeholder="Email del nuevo participante" required />
            <button type="submit" class="btn btn-primary">Añadir</button>
        </form>
        <div v-if="participantes.length > 0">
            <div v-for="p in participantes" :key="p.userId" class="list-item">
                <div><span>{{ p.nombre }}</span><small>{{ p.email }}</small></div>
                <button v-if="evento && p.userId !== evento.creadorId" class="btn btn-danger btn-sm" @click="handleRemoveParticipant(p.userId)">Eliminar</button>
            </div>
        </div>
      </div>

    </div>

    <!-- Expense Modal -->
    <div v-if="isExpenseModalOpen" class="modal-overlay" @click.self="closeExpenseModal">
        <div class="modal-content">
            <h2>{{ editingExpense ? 'Editar' : 'Añadir' }} Gasto</h2>
            <form @submit.prevent="handleSaveExpense">
                <label for="descripcion">Descripción:</label>
                <input id="descripcion" type="text" v-model="expenseData.descripcion" required />

                <label for="montoGasto">Monto Total:</label>
                <input id="montoGasto" type="number" v-model.number="expenseData.monto" required min="0.01" step="0.01" />

                <label for="pagadoPor">Pagado por:</label>
                <select id="pagadoPor" v-model.number="expenseData.pagadoPorUserId" required>
                    <option disabled value="0">Selecciona quién pagó</option>
                    <option v-for="p in participantes" :key="p.userId" :value="p.userId">{{ p.nombre }}</option>
                </select>

                <label>Dividir entre:</label>
                <div class="split-participants-grid">
                    <div v-for="p in participantes" :key="p.userId" class="split-participant-item">
                        <label :for="`split-${p.userId}`">{{ p.nombre }}</label>
                        <input type="number" :id="`split-${p.userId}`" v-model.number="expenseData.splits.find(s => s.userId === p.userId)!.montoAdeudado" min="0" step="0.01" />
                    </div>
                </div>

                <div class="split-summary">
                    <span>Total dividido: {{ totalSplitAmount.toFixed(2) }} €</span>
                    <span :class="{ 'error-text': !isSplitAmountValid }">
                        Restante: {{ (expenseData.monto - totalSplitAmount).toFixed(2) }} €
                    </span>
                </div>

                <div class="modal-actions">
                    <button type="button" class="btn btn-secondary" @click="closeExpenseModal">Cancelar</button>
                    <button type="submit" class="btn btn-primary" :disabled="!isSplitAmountValid">Guardar</button>
                </div>
            </form>
        </div>
    </div>

    <!-- Payment Modal -->
    <div v-if="isPaymentModalOpen" class="modal-overlay" @click.self="closePaymentModal">
        <div class="modal-content">
            <h2>{{ editingPayment ? 'Editar' : 'Registrar' }} Pago</h2>
            <form @submit.prevent="handleSavePayment">
                <label for="pagador">Pagador del pago:</label>
                <select id="pagador" v-model.number="paymentData.deQuienUserId" required>
                    <option disabled value="0">Selecciona un participante</option>
                    <option v-for="p in participantes" :key="p.userId" :value="p.userId">{{ p.nombre }}</option>
                </select>

                <label for="receptor">Receptor del pago:</label>
                <select id="receptor" v-model.number="paymentData.aQuienUserId" required>
                    <option disabled value="0">Selecciona un participante</option>
                    <option v-for="p in participantes" :key="p.userId" :value="p.userId">{{ p.nombre }}</option>
                </select>

                <label for="montoPago">Monto:</label>
                <input id="montoPago" type="number" v-model.number="paymentData.monto" required min="0.01" step="0.01" />

                <div class="modal-actions">
                    <button type="button" class="btn btn-secondary" @click="closePaymentModal">Cancelar</button>
                    <button type="submit" class="btn btn-primary">Guardar</button>
                </div>
            </form>
        </div>
    </div>

  </div>
</template>

<style>
.tabs {
    display: flex;
    margin-bottom: 1.5rem;
    border-bottom: 2px solid var(--color-border);
}

.tabs button {
    background-color: transparent;
    border: none;
    padding: 0.75rem 1.25rem;
    cursor: pointer;
    font-size: 1rem;
    font-weight: 500;
    color: var(--color-text-light);
    transition: color 0.3s ease, border-bottom 0.3s ease;
    border-bottom: 2px solid transparent;
    margin-bottom: -2px; /* Counteract border-bottom to keep alignment */
}

.tabs button:hover {
    color: var(--color-primary);
}

.tabs button.active {
    color: var(--color-primary);
    border-bottom-color: var(--color-primary);
    font-weight: 600;
}

/* ... existing styles ... */
.form-add-participant { display: flex; gap: 1rem; margin-bottom: 2rem; }
.form-add-participant input { flex-grow: 1; margin-bottom: 0; }

.split-participants-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;
}

.split-participant-item {
    display: flex;
    flex-direction: column;
}

.split-participant-item input {
    margin-bottom: 0;
}

.split-summary {
    margin-top: 1rem;
    padding: 0.5rem 0;
    border-top: 1px solid var(--color-border);
    display: flex;
    justify-content: space-between;
    font-weight: bold;
}

.error-text {
    color: var(--color-danger);
}
</style>



