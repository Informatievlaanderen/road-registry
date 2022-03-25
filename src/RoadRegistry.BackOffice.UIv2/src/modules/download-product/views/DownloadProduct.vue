<template>
  <div>
    <div class="vl-typography">
      <h2>Downloaden</h2>
    </div>

    <div class="vl-form-grid vl-form-grid--is-stacked" :class="{ 'disabled' : isDownloadInProgress }">
      <div class="vl-form-col--2-12">
        <label for="date">Datum</label>
      </div>
      <div class="vl-form-col--3-12">
        <input
          type="date"
          v-model="selectedDate"
          id="date"
          name="date"
          class="vl-input-field vl-input-field--block"
          :max="today"
        />
      </div>
      <div class="vl-form-col--7-12"></div>
      <div class="vl-form-col--5-12">
        <a class="vl-doormat js-vl-equal-height" href="#" :class="{ 'disabled' : parseDate(selectedDate) == 'Invalid Date' }" v-on:click="download">
          <div class="vl-doormat__content">
            <span class="vl-doormat__content__arrow" aria-hidden="true"></span>
            <h2 class="vl-doormat__title" data-vl-clamp="2">Register download product</h2>
            <div class="vl-doormat__text" data-vl-clamp="3">
              Download het wegenregister product.
            </div>
          </div>
        </a>
      </div>
    </div>


    <div v-if="isDownloadInProgress">
      Downloading..
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { BackOfficeApi } from "../../../services";

export default Vue.extend({
  data() {
    return {
      selectedDate: new Date().toISOString().substring(0, 10),
      today: new Date().toISOString().substring(0, 10),
      isDownloadInProgress: false
    };
  },
  methods: {
    parseDate(dateString: string): Date {
      return new Date(dateString);
    },
    async download() {
      const parsedDate = this.parseDate(this.selectedDate);
      // format date as yyyyMMdd
      const formattedDate = ''.concat(
        parsedDate.getFullYear().toString(), 
        (parsedDate.getMonth() + 1).toString().padStart(2, '0'), 
        parsedDate.getDate().toString().padStart(2, '0'));

      this.isDownloadInProgress = true;
      await BackOfficeApi.Downloads.getForProduct(formattedDate);
      this.isDownloadInProgress = false;
    }
  },
});
</script>

<style lang="scss">
.disabled {
    pointer-events:none; 
    opacity:0.6;        
}
</style>
