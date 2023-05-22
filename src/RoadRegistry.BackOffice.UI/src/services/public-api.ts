import apiClient from "./api-client";
import RoadRegistry from "@/types/road-registry";
import RoadRegistryExceptions from "@/types/road-registry-exceptions";
import Municipalities from "@/types/municipalities";
import axios from "axios";

import { API_ENDPOINT } from "@/environment";

export const PublicApi = {
    ChangeFeed: {
        getHead: async (maxEntryCount: number): Promise<RoadRegistry.GetHeadApiResponse> => {
            const path = `${API_ENDPOINT}/v1/wegen/activiteit/begin`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { maxEntryCount })
            return response.data as RoadRegistry.GetHeadApiResponse;
        },
        getContent: async (id: number): Promise<RoadRegistry.ChangeFeedContent> => {
            const path = `${API_ENDPOINT}/v1/wegen/activiteit/gebeurtenis/${id}/inhoud`;
            const response = await apiClient.get<RoadRegistry.ChangeFeedContent>(path)
            return response.data;
        },
        getNext: async (afterEntry?: number, maxEntryCount?: number) => {
            const path = `${API_ENDPOINT}/v1/wegen/activiteit/volgende`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { afterEntry, maxEntryCount })
            return response.data;
        },
        getPrevious: async (beforeEntry?: number, maxEntryCount?: number) => {
            const path = `${API_ENDPOINT}/v1/wegen/activiteit/vorige`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { beforeEntry, maxEntryCount })
            return response.data;
        }
    },
    Downloads: {
        getForEditor: async () => {
            const path = `${API_ENDPOINT}/v1/wegen/download/voor-editor`;
            await apiClient.download("application/zip", "wegenregister.zip", path, "GET")
        },
        getForProduct: async (date: string) => {
            const path = `${API_ENDPOINT}/v1/wegen/download/voor-product/${date}`;
            await apiClient.download("application/zip", `wegenregister-${date}.zip`, path, "GET")
        },
    },
    Uploads: {
        upload: async (file: string | Blob, filename: string): Promise<boolean> => {
            const path = `${API_ENDPOINT}/v1/wegen/upload`;
            const data = new FormData();
            data.append("archive", file, filename);
            const response = await apiClient.post(path,data)
            return response.status == 200;
        },
        download: async(identifier: string): Promise<void> => {
            const path = `${API_ENDPOINT}/v1/wegen/upload/${identifier}`;
            await apiClient.download("application/zip", `${identifier}.zip`, path, "GET")
        }
    },
    Extracts: {
        download: async (downloadid: string) => {
            const path = `${API_ENDPOINT}/v1/wegen/extract/download/${downloadid}`;
            await apiClient.download("application/zip", `${downloadid}.zip`, path, "GET")
        },
        upload: async (downloadid: string, file: string | Blob, filename: string) => {
            const path = `${API_ENDPOINT}/v1/wegen/extract/download/${downloadid}/uploads`;
            const data = new FormData();
            data.append(downloadid, file, filename);
            const response = await apiClient.post<RoadRegistry.UploadExtractResponseBody>(path,data)
            return response.data;
        },
        getUploadStatus: async (uploadid: string): Promise<{status: string}> => {
            const path = `${API_ENDPOINT}/v1/wegen/extract/upload/${uploadid}/status`;
            const response = await apiClient.get<{status: string}>(path);
            return response.data;
        },
        postDownloadRequest: async (downloadRequest: RoadRegistry.DownloadExtractRequest): Promise<RoadRegistry.DownloadExtractResponse> => {
            const path = `${API_ENDPOINT}/v1/wegen/extract/downloadaanvragen`;
            const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest)
            return response.data;
        },
        postDownloadRequestByContour: async (downloadRequest: RoadRegistry.DownloadExtractByContourRequest): Promise<RoadRegistry.DownloadExtractResponse> => {
            try {
                const path = `${API_ENDPOINT}/v1/wegen/extract/downloadaanvragen/percontour`;
                const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest)
                return response.data;
            } catch (exception) {
                if (axios.isAxiosError(exception)) {
                    const response = exception?.response;
                    if (response && response.status === 400) { // HTTP Bad Request
                        const error = response?.data as RoadRegistry.PerContourErrorResponse;
                        throw new RoadRegistryExceptions.RequestExtractPerContourError(error);
                    }
                }

                throw new Error('Unknown error');
            }
        },
        postDownloadRequestByNisCode: async (downloadRequest: RoadRegistry.DownloadExtractByNisCodeRequest): Promise<RoadRegistry.DownloadExtractResponse> => {
            const path = `${API_ENDPOINT}/v1/wegen/extract/downloadaanvragen/perniscode`;
            const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest)
            return response.data;
        }
    },
    Information: {
        getInformation: async (): Promise<RoadRegistry.RoadNetworkInformationResponse> => {
            const path = `${API_ENDPOINT}/v1/wegen/informatie`;
            const response = await apiClient.get<RoadRegistry.RoadNetworkInformationResponse>(path)
            return response.data;
        },
    },
    Municipalities: {
        getAll: async (): Promise<Municipalities.Gemeenten[]> => {
            const municipalities = [] as Municipalities.Gemeenten[];

            const headers = {Accept: "application/ld+json"};
            const query = {
              offset: 0,
              limit: 500,
              status: "InGebruik",
              isFlemishRegion: true
            };

            while(true) {
                const response = (await apiClient.get<Municipalities.GetMunicipalitiesAPIResponse>(`${API_ENDPOINT}/v2/gemeenten`,query,headers)).data;
                municipalities.push(...response.gemeenten)
                if(!response.volgende) 
                    break;
                query.offset += query.limit;
            }
            
            return municipalities;
        }
    }
}
export default PublicApi;