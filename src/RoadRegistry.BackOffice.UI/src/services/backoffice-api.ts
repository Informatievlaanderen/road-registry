import apiClient, { AxiosHttpApiClient } from "./api-client";
import RoadRegistry from "@/types/road-registry";
import RoadRegistryExceptions from "@/types/road-registry-exceptions";
import axios from "axios";
import { trimEnd } from "lodash";
import { featureToggles, API_OLDENDPOINT } from "@/environment";

const directApiEndpoint = trimEnd(API_OLDENDPOINT, "/");
const apiEndpoint = trimEnd(featureToggles.useDirectApiCalls ? directApiEndpoint : "/roads", "/");

export const BackOfficeApi = {
  ChangeFeed: {
    getHead: async (maxEntryCount: number, filter?: string): Promise<RoadRegistry.GetHeadApiResponse> => {
      const path = `${apiEndpoint}/v1/changefeed/head`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { maxEntryCount, filter });
      return response.data as RoadRegistry.GetHeadApiResponse;
    },
    getContent: async (id: number) => {
      const path = `${apiEndpoint}/v1/changefeed/entry/${id}/content`;
      const response = await apiClient.get<RoadRegistry.ChangeFeedContent>(path);
      return response.data;
    },
    getNext: async (afterEntry?: number, maxEntryCount?: number, filter?: string) => {
      const path = `${apiEndpoint}/v1/changefeed/next`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, {
        afterEntry,
        maxEntryCount,
        filter,
      });
      return response.data;
    },
    getPrevious: async (beforeEntry?: number, maxEntryCount?: number, filter?: string) => {
      const path = `${apiEndpoint}/v1/changefeed/previous`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, {
        beforeEntry,
        maxEntryCount,
        filter,
      });
      return response.data;
    },
  },
  Downloads: {
    getForEditor: async () => {
      const path = `${apiEndpoint}/v1/download/for-editor`;
      await apiClient.download("application/zip", "wegenregister.zip", path, "GET");
    },
    getForProduct: async (date: string) => {
      const path = `${apiEndpoint}/v1/download/for-product/${date}`;
      await apiClient.download("application/zip", `wegenregister-${date}.zip`, path, "GET");
    },
  },
  Uploads: {
    uploadFeatureCompare: async (
      file: string | Blob,
      filename: string
    ): Promise<RoadRegistry.UploadExtractResponseBody> => {
      const path = `${apiEndpoint}/v1/upload/fc`;
      const data = new FormData();
      data.append("archive", file, filename);
      const response = await apiClient.post<RoadRegistry.UploadExtractResponseBody>(path, data);
      if (response.data) {
        response.data.status = response.status;
      }
      return response.data;
    },
    uploadUsingPresignedUrl: async (
      file: string | Blob,
      filename: string
    ): Promise<RoadRegistry.UploadPresignedUrlResponse | null> => {
      const path = `${apiEndpoint}/v1/upload/jobs`;
      const response = await apiClient.post<RoadRegistry.UploadPresignedUrlResponse>(path);
      
      const data = new FormData();
      if (response.data.uploadUrlFormData) {
        for (let key in response.data.uploadUrlFormData) {
          data.append(key, response.data.uploadUrlFormData[key]);
        }
      }
      data.append("file", file, filename);
      
      let awsHttp = axios.create();
      var uploadFileResponse = await awsHttp.post(response.data.uploadUrl, data);
      
      let status = uploadFileResponse.status as any;
      if (status !== 204) {
        return null;
      }

      return response.data;
    },
    download: async (identifier: string): Promise<void> => {
      const path = `${apiEndpoint}/v1/upload/${identifier}`;
      await apiClient.download("application/zip", `${identifier}.zip`, path, "GET");
    },
  },
  Extracts: {
    download: async (downloadid: string) => {
      const path = `${apiEndpoint}/v1/extracts/download/${downloadid}`;
      await apiClient.download("application/zip", `${downloadid}.zip`, path, "GET");
    },
    upload: async (downloadid: string, file: string | Blob, filename: string) => {
      const path = `${apiEndpoint}/v1/extracts/download/${downloadid}/uploads`;
      const data = new FormData();
      data.append(downloadid, file, filename);
      const response = await apiClient.post<RoadRegistry.UploadExtractResponseBody>(path, data);
      return response.data;
    },
    getUploadStatus: async (uploadid: string): Promise<{ status: string }> => {
      const path = `${apiEndpoint}/v1/extracts/upload/${uploadid}/status`;
      const response = await apiClient.get<{ status: string }>(path);
      return response.data;
    },
    postDownloadRequest: async (
      downloadRequest: RoadRegistry.DownloadExtractRequest
    ): Promise<RoadRegistry.DownloadExtractResponse> => {
      const path = `${apiEndpoint}/v1/extracts/downloadrequests`;
      const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest);
      return response.data;
    },
    postDownloadRequestByContour: async (
      downloadRequest: RoadRegistry.DownloadExtractByContourRequest
    ): Promise<RoadRegistry.DownloadExtractResponse> => {
      const path = `${apiEndpoint}/v1/extracts/downloadrequests/bycontour`;
      const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest);
      return response.data;
    },
    postDownloadRequestByFile: async (
      downloadRequest: RoadRegistry.DownloadExtractByFileRequest
    ): Promise<RoadRegistry.DownloadExtractResponse> => {
      const path = `${apiEndpoint}/v1/extracts/downloadrequests/byfile`;

      const data = new FormData();
      data.append("description", downloadRequest.description);
      data.append("isInformative", downloadRequest.isInformative.toString());
      downloadRequest.files.forEach((file) => {
        data.append("files", file, file.name);
      });

      try {
        const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, data);
        return response.data;
      } catch (exception) {
        if (axios.isAxiosError(exception)) {
          const response = exception?.response;
          if (response && response.status === 400) {
            // HTTP Bad Request
            const error = response?.data as RoadRegistry.PerContourErrorResponse;
            throw new RoadRegistryExceptions.RequestExtractPerContourError(error);
          }
        }

        throw new Error("Unknown error");
      }
    },
    postDownloadRequestByNisCode: async (
      downloadRequest: RoadRegistry.DownloadExtractByNisCodeRequest
    ): Promise<RoadRegistry.DownloadExtractResponse> => {
      const path = `${apiEndpoint}/v1/extracts/downloadrequests/byniscode`;
      const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest);
      return response.data;
    },
    getOverlappingExtractRequestsByNisCode: async (
      nisCode: String
    ): Promise<RoadRegistry.ListOverlappingExtractsResponse> => {
      const request = {
        nisCode
      } as RoadRegistry.ListOverlappingExtractsByNisCodeRequest;
      const path = `${apiEndpoint}/v1/extracts/overlapping/byniscode`;
      const response = await apiClient.post<RoadRegistry.ListOverlappingExtractsResponse>(path, request);
      return response.data;
    },
    getOverlappingExtractRequestsByContour: async (
      contour: String
    ): Promise<RoadRegistry.ListOverlappingExtractsResponse> => {
      const request = {
        contour
      } as RoadRegistry.ListOverlappingExtractsByContourRequest;
      const path = `${apiEndpoint}/v1/extracts/overlapping/bycontour`;
      const response = await apiClient.post<RoadRegistry.ListOverlappingExtractsResponse>(path, request);
      return response.data;
    },
    getOverlappingTransactionZonesGeoJsonUrl() : String {
      return `${directApiEndpoint}/v1/extracts/overlappingtransactionzones.geojson`;
    },
    getTransactionZonesGeoJsonUrl() : String {
      return `${directApiEndpoint}/v1/extracts/transactionzones.geojson`;
    }
  },
  Information: {
    getInformation: async (): Promise<RoadRegistry.RoadNetworkInformationResponse> => {
      const path = `${apiEndpoint}/v1/information`;
      const response = await apiClient.get<RoadRegistry.RoadNetworkInformationResponse>(path);
      return response.data;
    },
    postValidateWkt: async (wkt: String): Promise<RoadRegistry.ValidateWktResponse> => {
      const path = `${apiEndpoint}/v1/information/validate-wkt`;
      const response = await apiClient.post(path, { contour: wkt });
      return response.data;
    },
  },
  Security: {
    getInfo: async (): Promise<RoadRegistry.SecurityInfo> => {
      const path = `${apiEndpoint}/v1/security/info`;
      const response = await apiClient.get<RoadRegistry.SecurityInfo>(path);
      return response.data;
    },
    getExchangeCode: async (code: string, verifier: string, redirectUri: string): Promise<string> => {
      const path = `${apiEndpoint}/v1/security/exchange?code=${code}&verifier=${verifier}&redirectUri=${redirectUri}`;
      const response = await apiClient.get(path);
      return response.data;
    },
    getAuthenticatedUser: async (): Promise<RoadRegistry.UserInfo> => {
      const apiClient = new AxiosHttpApiClient({
        noRedirectOnUnauthorized: true,
      });
      const path = `${apiEndpoint}/v1/security/user`;
      const response = await apiClient.get<RoadRegistry.UserInfo>(path);
      return response.data;
    },
  },
};
export default BackOfficeApi;
