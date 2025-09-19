<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getEvents, createEvent, type Event as EventType } from '@/services/api';
import type { AxiosError } from 'axios';

const eventos = ref<EventType[]>([]);
const nuevoEventoNombre = ref('');
const isLoading = ref(true);
const errorMessage = ref('');
const router = useRouter();

const cargarEventos = async () => {
  try {
    isLoading.value = true;
    const response = await getEvents();
    eventos.value = response.data;
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Error al cargar los eventos.';
  } finally {
    isLoading.value = false;
  }
};

const handleCrearEvento = async () => {
  if (!nuevoEventoNombre.value.trim()) return;
  errorMessage.value = '';

  try {
    const response = await createEvent({ nombre: nuevoEventoNombre.value });
    nuevoEventoNombre.value = '';
    // Opcional: en lugar de recargar todo, podríamos añadir el nuevo evento a la lista
    // eventos.value.push(response.data);
    await cargarEventos(); // Por ahora, recargar es más simple
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Error al crear el evento.';
  }
};

const irAEvento = (id: number) => {
  router.push({ name: 'EventoDetalle', params: { id } });
};

onMounted(() => {
  cargarEventos();
});
</script>

<template>
  <div class="container">
    <h1>Mis Eventos</h1>

    <form @submit.prevent="handleCrearEvento" class="form-crear-evento">
      <input
        v-model="nuevoEventoNombre"
        type="text"
        placeholder="Nombre del nuevo evento"
        required
      />
      <button type="submit" class="btn btn-primary">Crear Evento</button>
    </form>

    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>

    <div v-if="isLoading" class="text-center">Cargando eventos...</div>

    <div v-else-if="eventos.length > 0" class="eventos-lista">
      <div v-for="evento in eventos" :key="evento.id" class="list-item" @click="irAEvento(evento.id)">
        <span>{{ evento.nombre }}</span>
        <small>Creado: {{ new Date(evento.fechaCreacion).toLocaleDateString() }}</small>
      </div>
    </div>

    <div v-else class="card text-center">
      <p>No tienes ningún evento todavía. ¡Crea el primero!</p>
    </div>
  </div>
</template>

<style scoped>
.form-crear-evento {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
}

.form-crear-evento input {
  flex-grow: 1;
  margin-bottom: 0;
}

.eventos-lista {
  margin-top: 2rem;
}

.list-item {
  cursor: pointer;
  transition: background-color var(--transition-duration), transform var(--transition-duration);
}

.list-item:hover {
  background-color: var(--color-background-soft);
  transform: translateY(-2px);
  border-color: var(--color-primary);
}
</style>
