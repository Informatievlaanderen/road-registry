<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column v-if="uploadResult.uploadResponseCode">
            <vl-alert :title="alertInfo.title" :mod-success="alertInfo.success" :mod-warning="alertInfo.warning"
              :mod-error="alertInfo.error">
              <p>{{ alertInfo.text }}</p>
            </vl-alert>
          </vl-column>

          <vl-column>
            <h2 class="vl-title vl-title--h2">Opladen</h2>
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
            <vl-upload ref="vlUpload" id="upload-component" name="upload-component" url="#"
              upload-drag-text="Selecteer het zip‑bestand met de op te laden verschillen."
              upload-label="Archief opladen" :auto-process="false" :options="options"
              :mod-success="uploadResult.uploadResponseCode && alertInfo.success"
              :mod-error="uploadResult.uploadResponseCode && (alertInfo.error || alertInfo.warning)"
              :mod-disabled="isUploading" @upload-success="isUploading = false" @upload-complete="isUploading = false"
              @upload-canceled="isUploading = false" @upload-file-added="processing"
              @upload-file-added-manually="processing" />
            <!-- -->
          </vl-column>

        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import Vue from "vue";
import { BackOfficeApi } from "../../../services";

export default Vue.extend({
  data() {
    return {
      isUploading: false,
      uploadResult: { uploadResponseCode: undefined } as { uploadResponseCode: number | undefined },
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
    alertInfo(): { success: boolean; warning: boolean; error: boolean; title: string; text: string } {
      const status = {
        success: false,
        warning: false,
        error: false,
        title: "",
        text: "",
      };

      if (this.uploadResult.uploadResponseCode == 200) {
        status.success = true;
        status.title = "Gelukt!";
        status.text =
          "Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen. U kan de vooruitgang volgen via Activiteit.";
        return status;
      }

      if (this.uploadResult.uploadResponseCode == 400) {
        status.warning = true;
        status.title = "Technische storing";
        status.text = "Door een technische storing is dit loket tijdelijk niet beschikbaar.";
        return status;
      }

      if (this.uploadResult.uploadResponseCode == 408) {
        status.warning = true;
        status.title = "Technische storing";
        status.text = "Er was een probleem bij het opladen - de operatie nam teveel tijd in beslag.";
        return status;
      }

      if (this.uploadResult.uploadResponseCode == 1) {
        status.error = true;
        status.title = "Opgelet!";
        status.text = "Het max. toegelaten bestand groote is 28MB";
        return status;
      }

      if (this.uploadResult.uploadResponseCode == 2 || this.uploadResult.uploadResponseCode == 415) {
        status.error = true;
        status.title = "Opgelet!";
        status.text = "Enkel .zip bestanden zijn toegelaten.";
        return status;
      }

      status.error = true;
      status.title = "Technische storing";

      if (this.uploadResult.uploadResponseCode == 503) {
        status.text =
          "Opladen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw.";
      } else {
        status.text = "Er was een probleem bij het opladen - dit kan duiden op een probleem met de website.";
      }

      return status;
    },
  },
  methods: {
    async processing(file: File) {
      this.startUpload();
      const allowedFileTypes = ["application/zip", "application/x-zip-compressed"];
      if (!allowedFileTypes.includes(file.type)) {
        this.uploadResult = { uploadResponseCode: 2 };
        this.endUpload();
        return;
      }

      if (file.size > this.options.maxFilesize) {
        this.uploadResult = { uploadResponseCode: 1 };
        this.endUpload();
        return;
      }

      const uploadResponseCode = await BackOfficeApi.Uploads.upload(file, file.name);
      this.uploadResult = { uploadResponseCode };
      this.endUpload();
      return;
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
    },
    endUpload(): void {
      this.emptyQueue();
      this.isUploading = false;
    },
  },
});
</script>
