import Vue from "vue";
import axios, { AxiosInstance, Method } from "axios";
import { AuthService } from "@/auth";
import router from "@/router";
import { featureToggles } from "@/environment";

export interface IApiClient {
  get<T = any>(url: string, query?: any, headers?: any, config?: any): Promise<IApiResponse<T>>;
  delete(url: string, headers?: any): Promise<IApiResponse>;
  head(url: string, query?: any, headers?: any): Promise<IApiResponse>;
  post<T = any>(url: string, data?: any, headers?: any, config?: any, query?: any): Promise<IApiResponse<T>>;
  put<T = any>(url: string, data?: any, headers?: any): Promise<IApiResponse<T>>;
  patch<T = any>(url: string, data?: any, headers?: any): Promise<IApiResponse<T>>;
  download(mimetype: string, filename: string, url: string, method: Method, query?: any, headers?: any): Promise<void>;
}

export interface IApiResponse<T = any> {
  data: T;
  status: number;
  statusText: string;
  headers: any;
  config: any;
  request?: any;
}

export const apiStats = Vue.observable({
  pendingRequests: 0,
});

export class AxiosHttpApiClientOptions {
  public noRedirectOnUnauthorized: boolean = false;
}

const createAxiosInstance = (options?: AxiosHttpApiClientOptions) => {
  let http = axios.create();

  http.interceptors.request.use((config: any) => {
    config.withCredentials = !featureToggles.useDirectApiCalls;
    const apiKey = AuthService.getApiKey();
    if (apiKey) {
      config.headers["x-api-key"] = apiKey;
    } else {
      const token = AuthService.getToken();
      if (token) {
        config.headers["Authorization"] = `JwtBearer ${token}`;
      } else {
        console.warn('No ApiKey or Token found');
      }
    }
    return config;
  });

  http.interceptors.request.use(
    (config) => {
      apiStats.pendingRequests++;
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  http.interceptors.response.use(
    (response) => {
      apiStats.pendingRequests--;
      return response;
    },
    (error) => {
      if (options?.noRedirectOnUnauthorized !== true) {
        if (error.response?.status == 403 || error.response?.status == 401) {
          let redirect =
            router.currentRoute?.name === "login" ? router.currentRoute.query.redirect : router.currentRoute.fullPath;
          router.push({
            name: "login",
            query: { redirect },
          });
        } else {
          console.info(error);
        }
      }

      apiStats.pendingRequests--;
      return Promise.reject(error);
    }
  );

  return http;
};

export class AxiosHttpApiClient implements IApiClient {
  private axios: AxiosInstance;

  constructor(options?: AxiosHttpApiClientOptions) {
    this.axios = createAxiosInstance(options);
  }

  public async get<T = any>(url: string, query?: any, headers?: any, config?: any): Promise<IApiResponse<T>> {
    return await this.axios.get<T>(url, Object.assign({}, { params: query, headers }, config));
  }
  public async download(
    mimetype: string,
    filename: string,
    url: string,
    method: Method = "GET",
    query?: any,
    headers?: any
  ) {
    const response = await this.axios({
      url,
      headers,
      method,
      params: query,
      responseType: "blob",
    });
    const blob = new Blob([response.data], { type: mimetype });
    const downloadUrl = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = downloadUrl;
    link.download = filename;
    link.click();
  }
  public async postUpload<T = any>(formData: FormData, url: string, config?: any, query?: any, headers?: any) {
    return await await this.axios.post<T>(url, formData, Object.assign({}, { params: query, headers }, config));
  }
  public async delete(url: string, headers?: any): Promise<IApiResponse> {
    return await this.axios.delete(url, { headers });
  }
  public async head(url: string, query?: any, headers?: any): Promise<IApiResponse> {
    return await this.axios.head(url, { headers });
  }
  public async post<T = any>(
    url: string,
    data?: any,
    headers?: any,
    config?: any,
    query?: any
  ): Promise<IApiResponse<T>> {
    return await this.axios.post<T>(url, data, Object.assign({}, { params: query, headers }, config));
  }
  public async put<T = any>(url: string, data?: any, headers?: any): Promise<IApiResponse<T>> {
    return await this.axios.put<T>(url, data, { headers });
  }
  public async patch<T = any>(url: string, data?: any, headers?: any): Promise<IApiResponse<T>> {
    return await this.axios.patch<T>(url, data, { headers });
  }
}

export const apiClient = new AxiosHttpApiClient() as IApiClient;
export default apiClient;
