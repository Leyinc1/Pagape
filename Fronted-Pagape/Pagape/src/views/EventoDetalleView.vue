<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { getEventById } from '@/services/api';
import type { AxiosError } from 'axios';

// Interfaz para el objeto del evento
interface Evento {
  id: number;
  nombre: string;
  fechaCreacion: string;
  // Añade aquí más propiedades si las necesitas, como participantes, gastos, etc.
}

const route = useRoute();
const evento = ref<Evento | null>(null);
const isLoading = ref(true);
const errorMessage = ref('');

onMounted(async () => {
  const eventId = route.params.id as string;
  if (!eventId) return;

  try {
    const response = await getEventById(eventId);
    evento.value = response.data;
  } catch (err) {
    const error = err as AxiosError<{ message: string }>;
    errorMessage.value = error.response?.data?.message || 'Error al cargar los detalles del evento.';
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div>
    <div v-if="isLoading">Cargando...</div>
    <div v-else-if="errorMessage" style="color: red;">{{ errorMessage }}</div>
    <div v-else-if="evento">
      <h1>{{ evento.nombre }}</h1>
      <p>ID del Evento: {{ evento.id }}</p>
      <p>Creado el: {{ new Date(evento.fechaCreacion).toLocaleDateString() }}</p>
      <!-- Aquí mostraremos más detalles como participantes y gastos -->
    </div>
    <div v-else>
      <p>No se encontró el evento.</p>
    </div>
  </div>
</template>

<style scoped>
/* Estilos para la vista de detalle del evento */
</style>
