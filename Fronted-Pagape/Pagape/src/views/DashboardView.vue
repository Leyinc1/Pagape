<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getEvents, createEvent } from '@/services/api';
import type { AxiosError } from 'axios';

// Interfaces para tipar los datos del evento
interface Evento {
  id: number;
  nombre: string;
  fechaCreacion: string;
}

const eventos = ref<Evento[]>([]);
const nuevoEventoNombre = ref('');
const isLoading = ref(true);
const errorMessage = ref('');

// Función para cargar los eventos del usuario
const cargarEventos = async () => {
  try {
    const response = await getEvents();
    eventos.value = response.data;
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Error al cargar los eventos.';
  } finally {
    isLoading.value = false;
  }
};

// Función para manejar la creación de un nuevo evento
const handleCrearEvento = async () => {
  if (!nuevoEventoNombre.value.trim()) return; // Evita crear eventos sin nombre
  errorMessage.value = '';

  try {
    await createEvent({ nombre: nuevoEventoNombre.value });
    nuevoEventoNombre.value = ''; // Limpia el campo
    await cargarEventos(); // Recarga la lista de eventos
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Error al crear el evento.';
  }
};

// Carga los eventos automáticamente cuando el componente se monta
onMounted(() => {
  cargarEventos();
});
</script>

<template>
  <div>
    <h1>Mis Eventos</h1>

    <form @submit.prevent="handleCrearEvento">
      <input
        v-model="nuevoEventoNombre"
        placeholder="Nombre del nuevo evento"
        required
      />
      <button type="submit">Crear Evento</button>
    </form>

    <hr />

    <div v-if="isLoading">Cargando eventos...</div>
    <div v-else-if="errorMessage" style="color: red;">{{ errorMessage }}</div>

    <ul v-else-if="eventos.length > 0">
      <li v-for="evento in eventos" :key="evento.id">
        <router-link :to="{ name: 'evento-detalle', params: { id: evento.id } }">
          <span>{{ evento.nombre }}</span>
          <small> (Creado: {{ new Date(evento.fechaCreacion).toLocaleDateString() }})</small>
        </router-link>
      </li>
    </ul>

    <div v-else>
      <p>No tienes ningún evento todavía. ¡Crea el primero!</p>
    </div>
  </div>
</template>

<style scoped>
ul {
  list-style: none;
  padding: 0;
}
li {
  padding: 10px;
  border: 1px solid #ccc;
  margin-bottom: 10px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
li:hover {
  background-color: #f0f0f0;
}
</style>
