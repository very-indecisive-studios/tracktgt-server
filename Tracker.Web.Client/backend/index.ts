import { BackendAPIClient } from "backend/backendAPIClient";

export const backendAPIClientInstance = new BackendAPIClient(
    process.env.BACKEND_URL,
    { fetch }
);

export * from "backend/backendAPIClient";
