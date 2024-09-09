import apiClient, { AxiosHttpApiClient } from "./api-client";
import RoadRegistry from "@/types/road-registry";
import RoadRegistryExceptions from "@/types/road-registry-exceptions";
import Municipalities from "@/types/municipalities";
import axios from "axios";
import { trimEnd } from "lodash";
import { featureToggles, API_ENDPOINT } from "@/environment";
import BackOfficeApi from "./backoffice-api";

const directApiEndpoint = trimEnd(API_ENDPOINT, "/");
const apiEndpoint = trimEnd(featureToggles.useDirectApiCalls ? directApiEndpoint : "/public", "/");
const useBackOfficeApi = process.env.NODE_ENV !== "production";

export const PublicApi = {
  ChangeFeed: {
    getHead: async (maxEntryCount: number, filter?: string): Promise<RoadRegistry.GetHeadApiResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getHead(maxEntryCount, filter);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/begin`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, { maxEntryCount, filter });
      return response.data as RoadRegistry.GetHeadApiResponse;
    },
    getContent: async (id: number): Promise<RoadRegistry.ChangeFeedContent> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getContent(id);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/gebeurtenis/${id}/inhoud`;
      const response = await apiClient.get<RoadRegistry.ChangeFeedContent>(path);
      return response.data;
    },
    getNext: async (afterEntry?: number, maxEntryCount?: number, filter?: string) => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getNext(afterEntry, maxEntryCount, filter);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/volgende`;
      const response = await apiClient.get<RoadRegistry.GetHeadApiResponse>(path, {
        afterEntry,
        maxEntryCount,
        filter,
      });
      return response.data;
    },
    getPrevious: async (beforeEntry?: number, maxEntryCount?: number, filter?: string) => {
      if (useBackOfficeApi) {
        return BackOfficeApi.ChangeFeed.getPrevious(beforeEntry, maxEntryCount, filter);
      }

      const path = `${apiEndpoint}/v2/wegen/activiteit/vorige`;
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
      const path = `${apiEndpoint}/v2/wegen/download/voor-editor`;
      await apiClient.download("application/zip", "wegenregister.zip", path, "GET");
    },
    getForProduct: async (date: string) => {
      const path = `${apiEndpoint}/v2/wegen/download/voor-product/${date}`;
      await apiClient.download("application/zip", `wegenregister-${date}.zip`, path, "GET");
    },
  },
  Uploads: {
    upload: async (file: string | Blob, filename: string): Promise<boolean> => {
      const path = `${apiEndpoint}/v2/wegen/upload`;
      const data = new FormData();
      data.append("archive", file, filename);
      const response = await apiClient.post(path, data);
      return response.status == 200 || response.status == 202;
    },
    uploadUsingPresignedUrl: async (
      file: string | Blob,
      filename: string
    ): Promise<RoadRegistry.UploadPresignedUrlResponse | null> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Uploads.uploadUsingPresignedUrl(file, filename);
      }

      const path = `${apiEndpoint}/v2/wegen/upload/jobs`;
      const response = await apiClient.post<RoadRegistry.UploadPresignedUrlResponse>(path);
      
      const data = new FormData();
      if (response.data.uploadUrlFormData) {
        for (let key in response.data.uploadUrlFormData) {
          data.append(key, response.data.uploadUrlFormData[key]);
        }
      }
      data.append("file", file, filename);
      
      var uploadFileResponse = await apiClient.post(response.data.uploadUrl, data);
      
      let status = uploadFileResponse.status as any;
      if (status !== 204) {
        return null;
      }

      return response.data;
    },
    download: async (identifier: string): Promise<void> => {
      const path = `${apiEndpoint}/v2/wegen/upload/${identifier}`;
      await apiClient.download("application/zip", `${identifier}.zip`, path, "GET");
    },
  },
  Extracts: {
    download: async (downloadid: string) => {
      const path = `${apiEndpoint}/v2/wegen/extract/download/${downloadid}`;
      await apiClient.download("application/zip", `${downloadid}.zip`, path, "GET");
    },
    upload: async (downloadid: string, file: string | Blob, filename: string) => {
      const path = `${apiEndpoint}/v2/wegen/extract/download/${downloadid}/uploads`;
      const data = new FormData();
      data.append(downloadid, file, filename);
      const response = await apiClient.post<RoadRegistry.UploadExtractResponseBody>(path, data);
      return response.data;
    },
    getUploadStatus: async (uploadid: string): Promise<{ status: string }> => {
      const path = `${apiEndpoint}/v2/wegen/extract/upload/${uploadid}/status`;
      const response = await apiClient.get<{ status: string }>(path);
      return response.data;
    },
    postDownloadRequest: async (
      downloadRequest: RoadRegistry.DownloadExtractRequest
    ): Promise<RoadRegistry.DownloadExtractResponse> => {
      const path = `${apiEndpoint}/v2/wegen/extract/downloadaanvragen`;
      const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest);
      return response.data;
    },
    postDownloadRequestByContour: async (
      downloadRequest: RoadRegistry.DownloadExtractByContourRequest
    ): Promise<RoadRegistry.DownloadExtractResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Extracts.postDownloadRequestByContour(downloadRequest);
      }

      try {
        const path = `${apiEndpoint}/v2/wegen/extract/downloadaanvragen/percontour`;
        const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest);
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
      const path = `${apiEndpoint}/v2/wegen/extract/downloadaanvragen/perniscode`;
      const response = await apiClient.post<RoadRegistry.DownloadExtractResponse>(path, downloadRequest);
      return response.data;
    },
    getOverlappingExtractRequestsByNisCode: async (
      nisCode: String,
      buffer: Number
    ): Promise<RoadRegistry.ListOverlappingExtractsResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Extracts.getOverlappingExtractRequestsByNisCode(nisCode, buffer);
      }

      const request = {
        nisCode,
        buffer
      } as RoadRegistry.ListOverlappingExtractsByNisCodeRequest;
      const path = `${apiEndpoint}/v2/wegen/extracts/overlapping/perniscode`;
      const response = await apiClient.post<RoadRegistry.ListOverlappingExtractsResponse>(path, request);
      return response.data;
    },
    getOverlappingExtractRequestsByContour: async (
      contour: String
    ): Promise<RoadRegistry.ListOverlappingExtractsResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Extracts.getOverlappingExtractRequestsByContour(contour);
      }

      const request = {
        contour
      } as RoadRegistry.ListOverlappingExtractsByContourRequest;
      const path = `${apiEndpoint}/v2/wegen/extracts/overlapping/percontour`;
      const response = await apiClient.post<RoadRegistry.ListOverlappingExtractsResponse>(path, request);
      return response.data;
    },
    getOverlappingTransactionZonesGeoJsonUrl() : String {
      return `${directApiEndpoint}/v2/wegen/extract/overlappingtransactionzones.geojson`;
    },
    getTransactionZonesGeoJsonUrl() : String {
      return `${directApiEndpoint}/v2/wegen/extract/transactionzones.geojson`;
    }
  },
  Information: {
    getInformation: async (): Promise<RoadRegistry.RoadNetworkInformationResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Information.getInformation();
      }

      const path = `${apiEndpoint}/v2/wegen/informatie`;
      const response = await apiClient.get<RoadRegistry.RoadNetworkInformationResponse>(path);
      return response.data;
    },
    postValidateWkt: async (wkt: String): Promise<RoadRegistry.ValidateWktResponse> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Information.postValidateWkt(wkt);
      }

      const path = `${apiEndpoint}/v2/wegen/informatie/valideer-wkt`;
      const response = await apiClient.post(path, { contour: wkt });
      return response.data;
    },
  },
  Security: {
    getInfo: async (): Promise<RoadRegistry.SecurityInfo> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Security.getInfo();
      }

      const path = `${apiEndpoint}/v2/wegen/security/info`;
      const response = await apiClient.get<RoadRegistry.SecurityInfo>(path);
      return response.data;
    },
    getExchangeCode: async (code: string, verifier: string, redirectUri: string): Promise<string> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Security.getExchangeCode(code, verifier, redirectUri);
      }

      const path = `${apiEndpoint}/v2/wegen/security/exchange?code=${code}&verifier=${verifier}&redirectUri=${redirectUri}`;
      const response = await apiClient.get(path);
      return response.data;
    },
    getAuthenticatedUser: async (): Promise<RoadRegistry.UserInfo> => {
      if (useBackOfficeApi) {
        return BackOfficeApi.Security.getAuthenticatedUser();
      }

      const apiClient = new AxiosHttpApiClient({
        noRedirectOnUnauthorized: true,
      });
      const path = `${apiEndpoint}/v2/wegen/security/user`;
      const response = await apiClient.get<RoadRegistry.UserInfo>(path);
      return response.data;
    },
  },
  Municipalities: {
    getAll: async (): Promise<Municipalities.Gemeenten[]> => {
      const municipalities = [] as Municipalities.Gemeenten[];

      const headers = { Accept: "application/ld+json" };
      const query = {
        offset: 0,
        limit: 500,
        status: "InGebruik",
        isFlemishRegion: true,
      };

      while (true) {
        const response = (
          await apiClient.get<Municipalities.GetMunicipalitiesAPIResponse>(
            `${apiEndpoint}/v2/gemeenten`,
            query,
            headers
          )
        ).data;
        municipalities.push(...response.gemeenten);
        if (!response.volgende) break;
        query.offset += query.limit;
      }

      return municipalities;
    },
  },
};
export default PublicApi;
