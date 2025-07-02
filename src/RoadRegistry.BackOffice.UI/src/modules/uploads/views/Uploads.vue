<template>
  <vl-page>
    <vl-main>
      <vl-layout>
        <vl-grid mod-stacked>
          <vl-column>
            <wr-h2>Opladen</wr-h2>
          </vl-column>

          <vl-column>
            <UploadComponent @upload-complete="handleUploadComplete" />
          </vl-column>
        </vl-grid>
      </vl-layout>
    </vl-main>
  </vl-page>
</template>

<script lang="ts">
import Vue from "vue";
import UploadComponent from "./UploadComponent.vue";

export default Vue.extend({
  components: {
    UploadComponent,
  },
  methods: {
    handleUploadComplete(args: any) {
      this.$router.push({ name: "uploadStatus", params: { ticketId: args.ticketId } });
    },
  },
});
</script>
