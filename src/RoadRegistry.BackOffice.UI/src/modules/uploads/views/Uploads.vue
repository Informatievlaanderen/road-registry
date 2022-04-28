<template>
  <div>
    <div v-if="Object.keys(uploadResult).length !== 0">
      <div class="vl-alert vl-alert--success" role="alert" v-if="uploadResult.uploadResponseCode == 200">
        <div class="vl-alert__icon">
          <i class="vl-vi vl-vi-check-circle" aria-hidden="true"></i>
        </div>
        <div class="vl-alert__content">
          <p class="vl-alert__title">Gelukt!</p>
          <div class="vl-alert__message">
            <p>
              Oplading is gelukt. We gaan nu het bestand inhoudelijk controleren en daarna de wijzigingen toepassen. U
              kan de vooruitgang volgen via Activiteit.
            </p>
          </div>
        </div>
      </div>

      <div class="vl-alert vl-alert--warning" role="alert" v-else-if="uploadResult.uploadResponseCode == 400">
        <div class="vl-alert__icon">
          <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
        </div>
        <div class="vl-alert__content">
          <p class="vl-alert__title">Technische storing</p>
          <div class="vl-alert__message">
            <p>Door een technische storing is dit loket tijdelijk niet beschikbaar.</p>
          </div>
        </div>
      </div>

      <div class="vl-alert vl-alert--warning" role="alert" v-else-if="uploadResult.uploadResponseCode == 408">
        <div class="vl-alert__icon">
          <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
        </div>
        <div class="vl-alert__content">
          <p class="vl-alert__title">Technische storing</p>
          <div class="vl-alert__message">
            <p>Er was een probleem bij het opladen - de operatie nam teveel tijd in beslag.</p>
          </div>
        </div>
      </div>

      <div class="vl-alert vl-alert--warning" role="alert" v-else-if="uploadResult.uploadResponseCode == 415">
        <div class="vl-alert__icon">
          <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
        </div>
        <div class="vl-alert__content">
          <p class="vl-alert__title">Technische storing</p>
          <div class="vl-alert__message">
            <p>Opladen is enkel mogelijk op basis van zip bestanden. Probeer het opnieuw met een correct bestand.</p>
          </div>
        </div>
      </div>

      <div class="vl-alert vl-alert--warning" role="alert" v-else-if="uploadResult.uploadResponseCode == 503">
        <div class="vl-alert__icon">
          <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
        </div>
        <div class="vl-alert__content">
          <p class="vl-alert__title">Technische storing</p>
          <div class="vl-alert__message">
            <p>Opladen is momenteel niet mogelijk omdat we bezig zijn met importeren. Probeer het later nog eens opnieuw.</p>
          </div>
        </div>
      </div>

      <div class="vl-alert vl-alert--warning" role="alert" v-else>
        <div class="vl-alert__icon">
          <i class="vl-vi vl-vi-warning" aria-hidden="true"></i>
        </div>
        <div class="vl-alert__content">
          <p class="vl-alert__title">Technische storing</p>
          <div class="vl-alert__message">
            <p>Er was een probleem bij het opladen - dit kan duiden op een probleem met de website.</p>
          </div>
        </div>
      </div>

    </div>

    <div class="vl-typography">
      <h2>Opladen</h2>
    </div>

    <div class="vl-doormat__graphic-wrapper">
      <img class="vl-doormat__graphic" src="https://picsum.photos/1600/400?image=1048" alt="Bouwen in Brussel" />
    </div>

    <div class="vl-doormat vl-doormat--graphic js-vl-equal-height"  v-if="isUploading">
      <h2 class="vl-doormat__title" data-vl-clamp="2">Upload bezig</h2>
          <div class="vl-doormat__text" data-vl-clamp="3">
            Het door u geselecteerde zip‑bestand wordt geupload.
          </div>
    </div>
    <div class="vl-doormat__content" v-else>
      <a class="vl-doormat vl-doormat--graphic js-vl-equal-height" href="#">
        <label for="uploadInput">
          <span class="vl-doormat__content__arrow" aria-hidden="true"></span>
          <h2 class="vl-doormat__title" data-vl-clamp="2">Feature compare</h2>
          <div class="vl-doormat__text" data-vl-clamp="3">
            Selecteer het zip‑bestand met de op te laden verschillen.
          </div>
        </label>
      </a>
    </div>
    <input
      id="uploadInput"
      v-on:change="uploadFile($event)"
      type="file"
      style="display: none"
      accept="application/zip"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { BackOfficeApi } from "../../../services";

export default Vue.extend({
  data() {
    return {
      isUploading: false,
      uploadResult: {},
    };
  },
  methods: {
    async uploadFile(event: any) {
      const file = event.target.files[0];

      this.isUploading = true;
      this.uploadResult = {};
      const uploadResponseCode = await BackOfficeApi.Uploads.upload(file, file.name);
      this.uploadResult = {
        uploadResponseCode: uploadResponseCode,
      };
      this.isUploading = false;
    },
  },
});
</script>

<style lang="scss">
</style>
