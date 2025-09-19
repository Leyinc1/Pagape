import axios from 'axios';

// 1. Definimos las interfaces para describir nuestros datos
interface RegisterData {
  nombre: string;
  email: string;
  password: string;
}

interface LoginData {
  email: string;
  password: string;
}

// Creamos una instancia de Axios con la URL base de nuestro backend
const apiClient = axios.create({
  baseURL: 'http://localhost:5139/api', // Revisa que el puerto sea el correcto
  headers: {
    'Content-Type': 'application/json'
  }

});
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 2. Usamos las interfaces en lugar de 'any'
export const registerUser = (userData: RegisterData) => {
  return apiClient.post('/auth/register', userData);
};

export const loginUser = (credentials: LoginData) => {
  return apiClient.post('/auth/login', credentials);
};
export const getEvents = () => {
  return apiClient.get('/eventos');
};

export const createEvent = (eventData: { nombre: string }) => {
  return apiClient.post('/eventos', eventData);
};

export const getEventById = (id: string | number) => {
  return apiClient.get(`/eventos/${id}`);
};
