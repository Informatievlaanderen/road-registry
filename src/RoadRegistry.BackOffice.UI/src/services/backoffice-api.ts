import apiClient from "./api-client";
import RoadRegistry from "@/types/road-registry";

export const BackOfficeApi = {
    ChangeFeed: {
        getHead: async (maxEntryCount: number): Promise<RoadRegistry.GetHeadApiResponse> => {
            const path = "/roads/v1/changefeed/head";
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { maxEntryCount })
            return response.data as RoadRegistry.GetHeadApiResponse;
        },
        getContent: async (id: number) => {
            const path = `/roads/v1/changefeed/entry/${id}/content`;
            const response = await apiClient.get<RoadRegistry.ChangeFeedContent>(path)
            return response.data;
        },
        getNext: async (afterEntry?: number, maxEntryCount?: number) => {
            const path = `/roads/v1/changefeed/next`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { afterEntry, maxEntryCount })
            return response.data;
        },
        getPrevious: async (beforeEntry?: number, maxEntryCount?: number) => {
            const path = `/roads/v1/changefeed/previous`;
            const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { beforeEntry, maxEntryCount })
            return response.data;
        }
    },
    Downloads: {
        getForEditor: async () => {
            const path = `/roads/v1/download/for-editor`;
            await apiClient.download("application/zip", "wegenregister.zip", path, "GET")
        },
        getForProduct: async (date: string) => {
            const path = `/roads/v1/download/for-product/${date}`;
            await apiClient.download("application/zip", `wegenregister-${date}.zip`, path, "GET")
        },
    },
    Uploads: {
        upload: async (file: string | Blob, filename: string): Promise<number> => {
            const path = `/roads/v1/upload`;
            const data = new FormData();
            data.append("archive", file, filename);
            const response = await apiClient.post(path,data)
            return response.status;
        },
        download: async(identifier: string): Promise<void> => {
            const path = `/roads/v1/upload/${identifier}`;
            await apiClient.download("application/zip", `${identifier}.zip`, path, "GET")
        }
    },
    Extracts: {
        download: async (downloadid: string) => {
            const path = `/roads/v1/extracts/download/${downloadid}`;
            await apiClient.download("application/zip", `${downloadid}.zip`, path, "GET")
        },
        upload: async (downloadid: string, file: string | Blob, filename: string) => {
            const path = `/roads/v1/extracts/download/${downloadid}/uploads`;
            const data = new FormData();
            data.append(downloadid, file, filename);
            const response = await apiClient.post<RoadRegistry.UploadExtractResponseBody>(path,data)
            return response.data;
        },
        getUploadStatus: async (uploadid: string): Promise<{status: string}> => {
            const path = `/roads/v1/extracts/upload/${uploadid}/status`;
            const response = await apiClient.get<{status: string}>(path);
            return response.data;
        },
        postDownloadRequest: async (downloadRequest: RoadRegistry.DownloadExtractRequest): Promise<RoadRegistry.DownloadExtractResponse> => {
            const path = `/roads/v1/extracts/downloadrequests`;
            const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest)
            return response.data;
        },
        postDownloadRequestByContour: async (downloadRequest: RoadRegistry.DownloadExtractByContourRequest): Promise<RoadRegistry.DownloadExtractResponse> => {
            const path = `/roads/v1/extracts/downloadrequests/bycontour`;
            const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest)
            return response.data;
        },
        postDownloadRequestByNisCode: async (downloadRequest: RoadRegistry.DownloadExtractByNisCodeRequest): Promise<RoadRegistry.DownloadExtractResponse> => {
            const path = `/roads/v1/extracts/downloadrequests/byniscode`;
            const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest)
            return response.data;
        }
    },
    Information: {
        getInformation: async (): Promise<RoadRegistry.RoadNetworkInformationResponse> => {
            const path = `/roads/v1/information`;
            const response = await apiClient.get<RoadRegistry.RoadNetworkInformationResponse>(path)
            return response.data;
        },
    }
}
export default BackOfficeApi;