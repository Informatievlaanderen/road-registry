<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column v-if="uploadResult.uploadResponseCode && alertInfo.title">
            <vl-alert
              :title="alertInfo.title"
              :mod-success="alertInfo.success"
              :mod-warning="alertInfo.warning"
              :mod-error="alertInfo.error"
            >
              <p>{{ alertInfo.text }}</p>
              <p v-if="uploadResult.changeRequestId">
                U kan de vooruitgang volgen via <a :href="redirectToActivityPageUrl">Activiteit</a>. U wordt binnen 5
                seconden doorverwezen.
              </p>
            </vl-alert>
          </vl-column>

          <vl-column>
            <wr-h2>Opladen</wr-h2>
          </vl-column>

          <!-- 
          <vl-column>
            <vl-image src="https://picsum.photos/1600/400?image=1048" alt="Bouwen in Brussel" />
          </vl-column>
         -->

          <vl-column>
            <!-- <vl-doormat v-if="isUploading" title="Upload bezig">
              Het door u geselecteerde zip‑bestand wordt geupload.
            </vl-doormat> -->
            <vl-upload
              ref="vlUpload"
              id="upload-component"
              name="upload-component"
              url="#"
              upload-drag-text="Selecteer het zip-bestand met bronbestanden en doelbestanden voor het extract."
              upload-label="Archief opladen"
              :auto-process="false"
              :options="options"
              :mod-success="uploadResult.uploadResponseCode && alertInfo.success"
              :mod-error="uploadResult.uploadResponseCode && (alertInfo.error || alertInfo.warning)"
              :mod-disabled="isUploading || isProcessing"
              @upload-success="isUploading = false"
              @upload-complete="isUploading = false"
              @upload-canceled="isUploading = false"
              @upload-file-added="processing"
              @upload-file-added-manually="processing"
            />

            <div v-if="alertInfo.fileProblems && alertInfo.fileProblems.length > 0">
              <div v-for="fileProblem in alertInfo.fileProblems" :key="fileProblem.file">
                <br />
                <h3>
                  <strong>{{ fileProblem.file.toUpperCase() }}</strong>
                </h3>

                <ActivityProblems :problems="fileProblem.problems" />
              </div>
            </div>
            <!-- -->
          </vl-column>
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import Vue from "vue";
import { orderBy } from "lodash";
import { BackOfficeApi } from "../../../services";
import ActivityProblems from "../../activity/components/ActivityProblems.vue";
import { featureToggles } from "@/environment";
import RoadRegistry from "@/types/road-registry";

export default Vue.extend({
  components: {
    ActivityProblems,
  },
  data() {
    return {
      isUploading: false,
      isProcessing: false,
      uploadResult: {
        uploadResponseCode: undefined,
        fileProblems: [],
        changeRequestId: undefined,
      } as {
        uploadResponseCode: number | undefined;
        fileProblems: Array<any> | undefined;
        changeRequestId: string | undefined;
      },
      redirectToActivityPageTimeout: null as any,
    };
  },
  computed: {
    options() {
      return {
        uploadMultiple: false,
        autoQueue: false,
        autoProcessQueue: false,
        maxFiles: 1,
        maxFilesize: 28672000, //28MB
        acceptedFiles: "application/zip",
        paramName: "archive",
        headers: {},
      };
    },
    alertInfo(): {
      success: boolean;
      warning: boolean;
      error: boolean;
      title: string;
      text: string;
      fileProblems: Array<any> | undefined;
    } {
      const status = {
        success: false,
        warning: false,
        error: false,
        title: "",
        text: "",
        fileProblems: [] as Array<any> | undefined,
      };

      if (this.uploadResult.fileProblems && this.uploadResult.fileProblems.length > 0) {
        status.fileProblems = this.uploadResult.fileProblems;
        return status;
      }

      switch (this.uploadResult.uploadResponseCode) {
        case 1:
          status.error = true;
          status.title = "Opgelet!";
          status.text = "Het max. toegelaten bestand groote is 28MB";
          break;
        case 2:
        case 415:
          status.error = true;
          status.title = "Opgelet!";
          status.text = "Enkel .zip bestanden zijn toegelaten.";
          break;
        case 200:
        case 202:
          status.success = true;
          status.title = "Gelukt!";
          status.text =
            "Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen.";
          break;
        case 400:
          status.warning = true;
          status.title = "Technische storing";
          status.text = "Door een technische storing is dit loket tijdelijk niet beschikbaar.";
          break;
        case 404:
          status.error = true;
          status.title = "Opgelet!";
          status.text = "Het extractaanvraag werd niet gevonden.";
          break;
        case 408:
          status.warning = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - de operatie nam teveel tijd in beslag.";
          break;
        case 500:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - er is een onbekende fout gebeurd.";
          break;
        case 503:
          status.error = true;
          status.title = "Technische storing";
          status.text =
            "Opladen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw.";
          break;
        default:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website.";
          break;
      }

      return status;
    },
    redirectToActivityPageUrl() {
      return this.$router.resolve({ name: "activiteit", query: { filter: this.uploadResult.changeRequestId } }).href;
    },
  },
  beforeDestroy() {
    if (this.redirectToActivityPageTimeout) {
      clearTimeout(this.redirectToActivityPageTimeout);
    }
  },
  methods: {
    async processing(file: File) {
      this.startUpload();
      try {
        const allowedFileTypes = ["application/zip", "application/x-zip-compressed"];
        if (!allowedFileTypes.includes(file.type)) {
          this.uploadResult = { uploadResponseCode: 2, fileProblems: undefined, changeRequestId: undefined };
          return;
        }

        if (file.size > this.options.maxFilesize) {
          this.uploadResult = { uploadResponseCode: 1, fileProblems: undefined, changeRequestId: undefined };
          return;
        }

        try {
          this.uploadResult = { uploadResponseCode: undefined, fileProblems: undefined, changeRequestId: undefined };

          let uploadResponse: RoadRegistry.UploadExtractResponseBody;
          if (featureToggles.useFeatureCompare) {
            uploadResponse = await BackOfficeApi.Uploads.uploadFeatureCompare(file, file.name);
          } else {
            uploadResponse = await BackOfficeApi.Uploads.upload(file, file.name);
          }

          this.uploadResult = {
            uploadResponseCode: uploadResponse.status,
            fileProblems: undefined,
            changeRequestId: uploadResponse.changeRequestId,
          };

          if (this.uploadResult.changeRequestId) {
            this.redirectToActivityPageTimeout = setTimeout(this.redirectToActivityPage, 5000);
          }
        } catch (err: any) {
          if (err?.response?.status === 400) {
            let validationErrors = err?.response?.data?.validationErrors;
            let fileProblems = Object.keys(validationErrors).map((key) => {
              let problems = validationErrors[key].map((validationError: any) => ({
                severity: (validationError.code ?? "").startsWith("Warning") ? "Warning" : "Error",
                text: validationError.reason,
              }));
              problems = orderBy(problems, "severity");

              return {
                file: key,
                problems,
              };
            });
            this.uploadResult = { uploadResponseCode: 400, fileProblems, changeRequestId: undefined };
          } else if (err?.response?.status === 404) {
            this.uploadResult = { uploadResponseCode: 404, fileProblems: undefined, changeRequestId: undefined };
          } else {
            console.error("Upload failed", err);
            this.uploadResult = { uploadResponseCode: 500, fileProblems: undefined, changeRequestId: undefined };
          }
        }
      } finally {
        this.endUpload();
      }
    },
    emptyQueue(): void {
      const dropZone = this.$refs.vlUpload as any as { hasFiles: boolean; removeFiles: () => void };
      if (dropZone.hasFiles) {
        dropZone.removeFiles();
      }
    },
    startUpload(): void {
      this.emptyQueue();
      this.isUploading = true;
      this.isProcessing = true;
    },
    endUpload(): void {
      this.emptyQueue();
      this.isUploading = false;
      this.isProcessing = false;
    },
    redirectToActivityPage() {
      this.$router.push({ name: "activiteit", query: { filter: this.uploadResult.changeRequestId } });
    },
  },
});
</script>
