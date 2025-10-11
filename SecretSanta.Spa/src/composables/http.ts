import axios, { AxiosInstance } from "axios";

export const http: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URI,
  headers: {
    "Content-type": "application/json",
  },
});
