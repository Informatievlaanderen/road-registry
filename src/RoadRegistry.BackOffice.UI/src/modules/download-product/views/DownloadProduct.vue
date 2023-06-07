<template>
  <div>
    <wr-h2>Downloaden</wr-h2>
    
    <vl-button v-if="isDownloadInProgress" mod-loading>Download het wegenregister product...</vl-button>
    <vl-button v-else v-on:click="download">Download het wegenregister product</vl-button>
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
      isDownloadInProgress: false,
    };
  },
  methods: {
    parseDate(dateString: string): Date {
      return new Date(dateString);
    },
    async download() {
      const parsedDate = this.parseDate(this.selectedDate);
      // format date as yyyyMMdd
      const formattedDate = "".concat(
        parsedDate.getFullYear().toString(),
        (parsedDate.getMonth() + 1).toString().padStart(2, "0"),
        parsedDate.getDate().toString().padStart(2, "0")
      );

      this.isDownloadInProgress = true;
      await BackOfficeApi.Downloads.getForProduct(formattedDate);
      this.isDownloadInProgress = false;
    },
  },
});
</script>

<style lang="scss">
.disabled {
  pointer-events: none;
  opacity: 0.6;
}
</style>
