// Base URL of the Function App with the API, set at build time by the deploy workflow;
// empty locally, where Vite proxies /api
export const apiUrl = import.meta.env.VITE_API_URL ?? ''
