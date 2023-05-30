<template>
  <div>
    <wr-h2>Afmelden</wr-h2>

    <div class="vl-form-grid vl-form-grid--is-stacked">
      <div class="vl-form-col--12-12">
        <vl-alert icon="warning" title="Opgelet!" mod-warning role="alertdialog">
          <p>U bent afgemeld van onze diensten.</p>
        </vl-alert>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { AuthService } from "@/services/auth-service";

export default Vue.extend({
  data() {
    return {
      isLogoutInProgress: false,
    };
  },
  props: {
    redirect: String,
  },
  methods: {
    logout() {
      this.isLogoutInProgress = true;
      AuthService.logout();
      this.isLogoutInProgress = false;
    },
  },
});
</script>
