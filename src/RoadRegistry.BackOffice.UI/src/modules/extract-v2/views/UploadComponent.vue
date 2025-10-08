<template>
  <div>
    <vl-upload
      ref="vlUpload"
      id="upload-component"
      name="upload-component"
      url="#"
      upload-drag-text="Selecteer het zip-bestand met bronbestanden en doelbestanden voor het extract."
      upload-label="Archief opladen"
      :auto-process="false"
      :options="options"
      :mod-success="uploadResult.uploadResponseCode > 0 && alertInfo.success"
      :mod-error="uploadResult.uploadResponseCode > 0 && (alertInfo.error || alertInfo.warning)"
      :mod-disabled="isUploading || isProcessing"
      @upload-success="isUploading = false"
      @upload-complete="isUploading = false"
      @upload-canceled="isUploading = false"
      @upload-file-added="processing"
      @upload-file-added-manually="processing"
    />

    <div v-if="uploadResult.uploadResponseCode && alertInfo.text">
      <vl-alert
        :title="alertInfo.title"
        :mod-success="alertInfo.success"
        :mod-warning="alertInfo.warning"
        :mod-error="alertInfo.error"
      >
        <p>{{ alertInfo.text }}</p>
      </vl-alert>
    </div>

    <div v-if="alertInfo.fileProblems && alertInfo.fileProblems.length > 0">
      <div v-for="fileProblem in alertInfo.fileProblems" :key="fileProblem.file">
        <br />
        <h3 v-if="fileProblem.file">
          <strong>{{ fileProblem.file.toUpperCase() }}</strong>
        </h3>

        <ActivityProblems :problems="fileProblem.problems" />
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { orderBy } from "lodash";
import { PublicApi } from "@/services";
import ActivityProblems from "../../activity/components/ActivityProblems.vue";
import ActivitySummary from "../../activity/components/ActivitySummary.vue";

export default Vue.extend({
  components: {
    ActivityProblems,
    ActivitySummary,
  },
  props: {
    downloadId: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      isUploading: false,
      isProcessing: false,
      uploadResult: {
        uploadResponseCode: 0,
        fileProblems: [],
      } as {
        uploadResponseCode: number;
        fileProblems: Array<any>;
      },
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
        case 1000:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er is een onbekend probleem bij het opladen gebeurd.";
          break;
        default:
          status.error = true;
          status.title = "Technische storing";
          status.text = "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website.";
          break;
      }

      return status;
    },
  },
  beforeDestroy() {
    this.endUpload();
  },
  methods: {
    async processing(file: File) {
      this.startUpload();
      try {
        this.uploadResult = {
          uploadResponseCode: 0,
          fileProblems: [],
        };
        this.uploadResult = await this.uploadFile(file);
      } finally {
        this.endUpload();
      }
    },
    async uploadFile(file: File): Promise<{
      uploadResponseCode: number;
      fileProblems: Array<any>;
    }> {
      const allowedFileTypes = ["application/zip", "application/x-zip-compressed"];
      if (!allowedFileTypes.includes(file.type)) {
        return {
          uploadResponseCode: 2,
          fileProblems: [],
        };
      }

      if (file.size > this.options.maxFilesize) {
        return {
          uploadResponseCode: 1,
          fileProblems: [],
        };
      }

      try {
        this.$emit("upload-start");

        let presignedUploadResponse = await PublicApi.Extracts.V2.upload(this.downloadId, file, file.name);
        if (!presignedUploadResponse) {
          return {
            uploadResponseCode: 1000,
            fileProblems: [],
          };
        }

        let ticketId = presignedUploadResponse.ticketUrl.split("/").reverse()[0];
        this.$emit("upload-complete", {
          ticketId: ticketId,
        });

        return {
          uploadResponseCode: 0,
          fileProblems: [],
        };
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

          return {
            uploadResponseCode: 400,
            fileProblems,
          };
        }

        if (err?.response?.status === 404) {
          return {
            uploadResponseCode: 404,
            fileProblems: [],
          };
        }

        console.error("Upload failed", err);
        return {
          uploadResponseCode: 500,
          fileProblems: [],
        };
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
  },
});
</script>
