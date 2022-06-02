export const AuthService = {
    login: async (apiKey: string): Promise<boolean> => {
        return true;
    },
    isAuthenticated: async (): Promise<boolean> => {
        return false;
    }
}