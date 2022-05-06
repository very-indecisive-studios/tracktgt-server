export class ExtBackendAPIClient {
    protected async transformOptions(options: RequestInit): Promise<RequestInit> {
        options.headers = {
            ...options.headers
            // TODO: Add private API token to Bearer header. 
        }

        return options;
    }
}
